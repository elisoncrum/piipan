#!/bin/bash
#
# Provisions and configures the infrastructure components for all Piipan
# subsystems. Assumes an Azure user with the Global Administrator role
# has signed in with the Azure CLI. Must be run from a trusted network.
# See install-extensions.bash for prerequisite Azure CLI extensions.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: create-resources.bash <azure-env>

source $(dirname "$0")/../tools/common.bash || exit
source $(dirname "$0")/iac-common.bash || exit

# Name of Key Vault
VAULT_NAME=secret-keeper

# Name of secret used to store the PostgreSQL server admin password
PG_SECRET_NAME=particpants-records-admin

# Name of administrator login for PostgreSQL server
PG_SUPERUSER=postgres

# Name of Azure Active Directory admin for PostgreSQL server
PG_AAD_ADMIN=piipan-admins

# Name of PostgreSQL server
PG_SERVER_NAME=participant-records

# Base name of query tool app
QUERY_TOOL_APP_NAME=piipan-query-tool
QUERY_TOOL_FRONTDOOR_NAME=querytool

# Display name of service principal account responsible for CI/CD tasks
SP_NAME_CICD=piipan-cicd

# App Service Authentication is done at the Azure tenant level
TENANT_ID=$(az account show --query homeTenantId -o tsv)

# Generate the storage account connection string for the corresponding
# blob storage account.
# XXX Uses the secondary access key (aka `key2`) for internal access, reserving
#     the primary access key (aka `key1`) for state access. Improve by replacing
#     with share access signatures (SAS URLs) via managed identities at runtime.
blob_connection_string () {
  resource_group=$1
  name=$2

  az storage account show-connection-string \
    --key secondary \
    --resource-group $resource_group \
    --name $name \
    --query connectionString \
    -o tsv
}

# From a managed identity name, generate the value for
# AzureServicesAuthConnectionString
az_connection_string () {
  resource_group=$1
  identity=$2

  client_id=$(\
    az identity show \
      --resource-group $resource_group \
      --name $identity \
      --query clientId \
      --output tsv)

    echo "RunAs=App;AppId=${client_id}"
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source $(dirname "$0")/env/${azure_env}.bash
  verify_cloud

  ./create-resource-groups.bash $azure_env

  # uniqueString is used pervasively in our ARM templates to create globally
  # identifiers from the resource group id, but it is not available in the CLI.
  # As we need to reference that unique value elsewhere, extract it out from
  # a dummy template.
  DEFAULT_UNIQ_STR=`az deployment group create \
    --resource-group $RESOURCE_GROUP \
    --template-file ./arm-templates/unique-string.json \
    --query properties.outputs.uniqueString.value \
    -o tsv`

  # Many CLI commands use a URI to identify nested resources; pre-compute the URI's prefix
  # for our default resource group
  DEFAULT_PROVIDERS=/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP}/providers

  # Create a key vault which will store credentials for use in other templates
  az deployment group create \
    --name $VAULT_NAME \
    --resource-group $RESOURCE_GROUP \
    --template-file ./arm-templates/key-vault.json \
    --parameters \
      name=$VAULT_NAME \
      location=$LOCATION \
      objectId=$CURRENT_USER_OBJID \
      resourceTags="$RESOURCE_TAGS"

  # For each participating state, create a separate storage account.
  # Each account has a blob storage container named `upload`.
  while IFS=, read -r abbr name ; do
      echo "Creating storage for $name ($abbr)"
      az deployment group create \
      --name "${abbr}-blob-storage" \
      --resource-group $RESOURCE_GROUP \
      --template-file ./arm-templates/blob-storage.json \
      --parameters \
        stateAbbreviation=$abbr \
        resourceTags="$RESOURCE_TAGS" \
        location=$LOCATION
  done < states.csv

  # Avoid echoing passwords in a manner that may show up in process listing,
  # or storing it in a temp file that may be read, or appearing in a CI/CD log.
  #
  # By default, Azure CLI will print the password set in Key Vault; instead
  # just extract and print the secret id from the JSON response.
  export PG_SECRET=`random_password`
  printenv PG_SECRET | tr -d '\n' | az keyvault secret set \
    --vault-name $VAULT_NAME \
    --name $PG_SECRET_NAME \
    --file /dev/stdin \
    --query id

  echo "Creating PostgreSQL server"
  az deployment group create \
    --name participant-records \
    --resource-group $RESOURCE_GROUP \
    --template-file ./arm-templates/participant-records.json \
    --parameters \
      administratorLogin=$PG_SUPERUSER \
      serverName=$PG_SERVER_NAME \
      secretName=$PG_SECRET_NAME \
      vaultName=$VAULT_NAME \
      resourceTags="$RESOURCE_TAGS"

  # The AD admin can't be specified in the PostgreSQL ARM template,
  # unlike in Azure SQL
  az ad group create --display-name $PG_AAD_ADMIN --mail-nickname $PG_AAD_ADMIN
  PG_AAD_ADMIN_OBJID=`az ad group show --group $PG_AAD_ADMIN --query objectId --output tsv`
  az postgres server ad-admin create \
    --resource-group $RESOURCE_GROUP \
    --server $PG_SERVER_NAME \
    --display-name $PG_AAD_ADMIN \
    --object-id $PG_AAD_ADMIN_OBJID

  # Create managed identities to admin each state's database
  while IFS=, read -r abbr name ; do
      echo "Creating managed identity for $name ($abbr)"
      abbr=`echo "$abbr" | tr '[:upper:]' '[:lower:]'`
      identity=${abbr}admin
      az identity create -g $RESOURCE_GROUP -n $identity
  done < states.csv

  exists=`az ad group member check \
    --group $PG_AAD_ADMIN \
    --member-id $CURRENT_USER_OBJID \
    --query value -o tsv`

  if [ "$exists" = "true" ]; then
    echo "$CURRENT_USER_OBJID is already a member of $PG_AAD_ADMIN"
  else
    # Temporarily add current user as a PostgreSQL AD admin
    # to allow provisioning of managed identity roles
    az ad group member add \
      --group $PG_AAD_ADMIN \
      --member-id $CURRENT_USER_OBJID
  fi

  export PGPASSWORD=$PG_SECRET
  export PGUSER=${PG_SUPERUSER}@${PG_SERVER_NAME}
  export PGHOST=`az resource show \
    --resource-group $RESOURCE_GROUP \
    --name $PG_SERVER_NAME \
    --resource-type "Microsoft.DbForPostgreSQL/servers" \
    --query properties.fullyQualifiedDomainName -o tsv`

  # Multiple PostgreSQL databases cannot be created with an ARM template;
  # detailed database/schema/role configuration can't be done with an ARM
  # template either. Instead, we access the PostgreSQL server from a trusted
  # network (as established by its ARM template firewall variable), and apply
  # various Data Definition (DDL) scripts for each state.
  ./create-databases.bash $RESOURCE_GROUP

  # Apply DDL shared between the ETL and match API subsystems.
  # XXX This should be moved out of IaC, which is not run in CI/CD,
  #     to a continuously deployable workflow that accomodates schema
  #     changes over time.
  pushd ../ddl
  ./apply-ddl.bash
  popd

  if [ "$exists" = "true" ]; then
    echo "Leaving $CURRENT_USER_OBJID as a member of $PG_AAD_ADMIN"
  else
    # Revoke temporary assignment of current user as a PostgreSQL AD admin
    az ad group member remove \
      --group $PG_AAD_ADMIN \
      --member-id $CURRENT_USER_OBJID
  fi

  # This is a subscription-level resource provider
  az provider register --wait --namespace Microsoft.EventGrid

  # Create per-state Function apps and assign corresponding managed identity for
  # access to the per-state blob-storage and database, set up system topics and
  # event subscription to bulk upload (blob creation) events
  while IFS=, read -r abbr name ; do
    echo "Creating function app for $name ($abbr)"
    abbr=`echo "$abbr" | tr '[:upper:]' '[:lower:]'`

    # Per-state Function App
    func_app=${abbr}func${DEFAULT_UNIQ_STR}

    # Storage account for the Function app for its own use;
    # matches name generated in function-storage.json
    func_stor=${abbr}fstor${DEFAULT_UNIQ_STR}

    # Managed identity to access database
    identity=${abbr}admin

    # Per-state database
    db_name=${abbr}

    # Actual Function, under the Function App, that receives an event
    # and does the work, name derived from classname in `etl` directory
    func_name=BulkUpload

    # Per-state storage account for bulk upload;
    # matches name generated in blob-storage.json
    stor_name=${abbr}state${DEFAULT_UNIQ_STR}

    # System topic for per-state upload (create blob) events
    topic_name=${abbr}-blob-topic

    # Subscription to upload events that get routed to Function
    sub_name=${abbr}-blob-subscription

    # Every Function app needs a storage account for its own internal use;
    # e.g., bindings state, keys, function code. Keep this separate from
    # the storage account used to upload data for better isolation.
    az deployment group create \
      --name "${abbr}-func-storage" \
      --resource-group $RESOURCE_GROUP \
      --template-file ./arm-templates/function-storage.json \
      --parameters \
        stateAbbreviation=$abbr \
        resourceTags="$RESOURCE_TAGS" \
        location=$LOCATION

    # Even though the OS *should* be abstracted away at the Function level, Azure
    # portal has oddities/limitations when using Linux -- lets just get it
    # working with Windows as underlying OS
    az functionapp create \
      --resource-group $RESOURCE_GROUP \
      --consumption-plan-location $LOCATION \
      --tags Project=$PROJECT_TAG \
      --runtime dotnet \
      --functions-version 3 \
      --os-type Windows \
      --name $func_app \
      --storage-account $func_stor

    # XXX Assumes if any identity is set, it is the one we are specifying below
    exists=`az functionapp identity show \
      --resource-group $RESOURCE_GROUP \
      --name $func_app`

    if [ -z "$exists" ]; then
      # Conditionally execute otherwise we will get an error if it is already
      # assigned this managed identity
      az functionapp identity assign \
        --resource-group $RESOURCE_GROUP \
        --name $func_app \
        --identities ${DEFAULT_PROVIDERS}/Microsoft.ManagedIdentity/userAssignedIdentities/${identity}
    fi

    db_conn_str=`pg_connection_string $PG_SERVER_NAME $db_name $identity`
    blob_conn_str=`blob_connection_string $RESOURCE_GROUP $stor_name`
    az_serv_str=`az_connection_string $RESOURCE_GROUP $identity`
    az functionapp config appsettings set \
      --resource-group $RESOURCE_GROUP \
      --name $func_app \
      --settings \
        $DB_CONN_STR_KEY="$db_conn_str" \
        $AZ_SERV_STR_KEY="$az_serv_str" \
        $BLOB_CONN_STR_KEY="$blob_conn_str" \
        $CLOUD_NAME_STR_KEY="$CLOUD_NAME" \
      --output none

    az eventgrid system-topic create \
      --location $LOCATION \
      --name $topic_name \
      --topic-type Microsoft.Storage.storageAccounts \
      --resource-group $RESOURCE_GROUP \
      --source ${DEFAULT_PROVIDERS}/Microsoft.Storage/storageAccounts/${stor_name}

    # Create Function endpoint before setting up event subscription
    pushd ../etl/src/Piipan.Etl
    func azure functionapp publish $func_app --dotnet
    popd

    az eventgrid system-topic event-subscription create \
      --name $sub_name \
      --resource-group $RESOURCE_GROUP \
      --system-topic-name $topic_name \
      --endpoint ${DEFAULT_PROVIDERS}/Microsoft.Web/sites/${func_app}/functions/${func_name} \
      --endpoint-type azurefunction \
      --included-event-types Microsoft.Storage.BlobCreated \
      --subject-begins-with /blobServices/default/containers/upload/blobs/
  done < states.csv

  # Create per-state Function apps for state-level matching API using
  # ARM template and deploy project code to each function using functions
  # core tools. ARM template assigns a corresponding storage account,
  # managed identity, hosting plan, and application insights instance.
  #
  # Assumes existence of a managed identity with name `{abbr}admin`.

  # Relative path for per-state Query endpoint
  MATCH_API_QUERY_NAME='Query'

  match_api_uris=''
  match_func_names=()

  while IFS=, read -r abbr name ; do
    echo "Creating match API function app for $name ($abbr)"
    abbr=`echo "$abbr" | tr '[:upper:]' '[:lower:]'`

    identity=${abbr}admin
    db_name=${abbr}
    client_id=$(\
      az identity show \
        --resource-group $RESOURCE_GROUP \
        --name $identity \
        --query clientId \
        --output tsv)
    db_conn_str=`pg_connection_string $PG_SERVER_NAME $db_name $identity`
    az_serv_str=`az_connection_string $RESOURCE_GROUP $identity`

    echo "Deploying ${name} function resources"
    func_name=$(\
      az deployment group create \
        --name match-api \
        --resource-group $MATCH_RESOURCE_GROUP \
        --template-file  ./arm-templates/function-state-match.json \
        --query properties.outputs.functionAppName.value \
        --output tsv \
        --parameters \
          resourceTags="$RESOURCE_TAGS" \
          identityGroup=$RESOURCE_GROUP \
          location=$LOCATION \
          azAuthConnectionString=$az_serv_str \
          stateName="$name" \
          stateAbbr="$abbr" \
          dbConnectionString="$db_conn_str" \
          dbConnectionStringKey="$DB_CONN_STR_KEY")

    # Store function names for future auth configuration
    match_func_names+=("$func_name")

    echo "Waiting to publish function app"
    sleep 60

    echo "Publishing ${name} function app"
    pushd ../match/src/Piipan.Match.State
    func azure functionapp publish $func_name --dotnet
    popd

    # Store API query URIs as a JSON array to be bound to orchestrator API
    func_uri=$(\
      az functionapp function show \
        --resource-group $MATCH_RESOURCE_GROUP \
        --name $func_name \
        --function-name $MATCH_API_QUERY_NAME \
        --query invokeUrlTemplate \
        --output tsv)
    match_api_uris=${match_api_uris}",\"$func_uri\""
  done < states.csv

  # Create orchestrator-level Function app using ARM template and
  # deploy project code using functions core tools.
  match_api_uris="[${match_api_uris:1}]"
  orch_name=$(\
    az deployment group create \
      --name orch-api \
      --resource-group $MATCH_RESOURCE_GROUP \
      --template-file  ./arm-templates/function-orch-match.json \
      --query properties.outputs.functionAppName.value \
      --output tsv \
      --parameters \
        resourceTags="$RESOURCE_TAGS" \
        location=$LOCATION \
        StateApiUriStrings=$match_api_uris)
  orch_identity=$(\
    az webapp identity show \
      --name $orch_name \
      --resource-group $MATCH_RESOURCE_GROUP \
      --query principalId \
      --output tsv)


  echo "Waiting to publish function app"
  sleep 60

  echo "Publishing ${orch_name} function app"
  pushd ../match/src/Piipan.Match.Orchestrator
  func azure functionapp publish $orch_name --dotnet
  popd

  # Create App Service resources for query tool app.
  # This needs to happen after the orchestrator is created in order for
  # $orch_api to be set.
  echo "Creating App Service resources for query tool app"

  orch_api_uri=$(\
    az functionapp function show \
      -g $MATCH_RESOURCE_GROUP \
      -n $orch_name \
      --function-name Query \
      --query invokeUrlTemplate \
      -o tsv)

  query_tool_name=$(\
    az deployment group create \
      --name $QUERY_TOOL_APP_NAME \
      --resource-group $RESOURCE_GROUP \
      --template-file ./arm-templates/query-tool-app.json \
      --query properties.outputs.appName.value \
      --output tsv \
      --parameters \
        location=$LOCATION \
        resourceTags="$RESOURCE_TAGS" \
        appName=$QUERY_TOOL_APP_NAME \
        servicePlan=$APP_SERVICE_PLAN \
        OrchApiUri=$orch_api_uri)

  echo "Create Front Door and WAF policy for query tool app"
  suffix=$(web_app_host_suffix)
  query_tool_host=${query_tool_name}${suffix}
  ./add-front-door-to-app.bash \
    $azure_env \
    $RESOURCE_GROUP \
    $QUERY_TOOL_FRONTDOOR_NAME \
    $query_tool_host

  # Establish metrics sub-system
  ./create-metrics-resources.bash $azure_env

  script_completed
}

main "$@"
