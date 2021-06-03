#!/bin/bash
#
# Provisions and configures the infrastructure components for all Piipan Metrics subsystems.
# Assumes an Azure user with the Global Administrator role has signed in with the Azure CLI.
# Assumes Piipan base resource groups, resources have been created in the same environment
# (for example, state-specific blob topics).
# Must be run from a trusted network.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: create-metrics-resources.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

set_constants () {
  DB_SERVER_NAME=$PREFIX-psql-core-$ENV
  DB_ADMIN_NAME=piipanadmin
  DB_NAME=metrics
  DB_TABLE_NAME=participant_uploads
  # Needed for both function apps
  DB_CONN_STR=$(pg_connection_string "$DB_SERVER_NAME" "$DB_NAME" "$DB_ADMIN_NAME")
  # Name of Key Vault
  VAULT_NAME_KEY=KeyVaultName
  VAULT_NAME=$PREFIX-kv-metrics-$ENV
  # Name of secret used to store the PostgreSQL metrics server admin password
  PG_SECRET_NAME=metrics-pg-admin
  # Dashboard App Info
  DASHBOARD_APP_NAME=$PREFIX-app-dashboard-$ENV
  DASHBOARD_FRONTDOOR_NAME=$PREFIX-fd-dashboard-$ENV
  DASHBOARD_WAF_NAME=wafdashboard${ENV}
  # Metrics Collection Info
  COLLECT_APP_FILEPATH=Piipan.Metrics.Collect
  COLLECT_STORAGE_NAME=${PREFIX}st${METRICS_COLLECT_APP_ID}${ENV}
  COLLECT_FUNC=BulkUploadMetrics
  # Metrics API Info
  API_APP_FILEPATH=Piipan.Metrics.Api
  API_APP_STORAGE_NAME=${PREFIX}st${METRICS_API_APP_ID}${ENV}

  PRIVATE_DNS_ZONE=$(private_dns_zone)
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud

  set_constants

  # Create new Key Vault for this resource group
  echo "Creating Key Vault"
  az deployment group create \
    --name "$VAULT_NAME" \
    --resource-group "$METRICS_RESOURCE_GROUP" \
    --template-file ./arm-templates/key-vault.json \
    --parameters \
      name="$VAULT_NAME" \
      location="$LOCATION" \
      objectId="$CURRENT_USER_OBJID" \
      resourceTags="$RESOURCE_TAGS"

  # Set PG Secret in key vault
  # By default, Azure CLI will print the password set in Key Vault; instead
  # just extract and print the secret id from the JSON response.
  echo "Setting key in vault"
  PG_SECRET=$(random_password)
  export PG_SECRET
  printenv PG_SECRET | tr -d '\n' | az keyvault secret set \
    --vault-name "$VAULT_NAME" \
    --name "$PG_SECRET_NAME" \
    --file /dev/stdin \
    --query id

  echo "Creating Metrics database server"
  az deployment group create \
    --name metrics \
    --resource-group "$METRICS_RESOURCE_GROUP" \
    --template-file ./arm-templates/database-metrics.json \
    --parameters \
      administratorLogin=$DB_ADMIN_NAME \
      serverName="$DB_SERVER_NAME" \
      secretName="$PG_SECRET_NAME" \
      vaultName="$VAULT_NAME" \
      vnetName="$VNET_NAME" \
      subnetName="$DB_2_SUBNET_NAME" \
      privateEndpointName="$CORE_DB_PRIVATE_ENDPOINT_NAME" \
      privateDnsZoneName="$PRIVATE_DNS_ZONE" \
      resourceTags="$RESOURCE_TAGS" \
      eventHubName="$EVENT_HUB_NAME"

  ### Database stuff
  # Create database within db server (command is idempotent)
  az postgres db create --name $DB_NAME --resource-group "$METRICS_RESOURCE_GROUP" --server-name "$DB_SERVER_NAME"

  ## Connect to the db server
  # For some reason PG threw an Invalid Username error when trying to set PGUSER here, so it's specified in the prompt instead.
  # export PGUSER=${DB_ADMIN_NAME}@${DB_SERVER_NAME}
  PGHOST=$(az resource show \
    --resource-group "$METRICS_RESOURCE_GROUP" \
    --name "$DB_SERVER_NAME" \
    --resource-type "Microsoft.DbForPostgreSQL/servers" \
    --query properties.fullyQualifiedDomainName -o tsv)
  export PGHOST
  export PGPASSWORD=$PG_SECRET

  echo "Insert db table"
  psql -U "$DB_ADMIN_NAME@$DB_SERVER_NAME" -p 5432 -d "$DB_NAME" -w -v ON_ERROR_STOP=1 -X -q - <<EOF
      CREATE TABLE IF NOT EXISTS $DB_TABLE_NAME (
          id serial PRIMARY KEY,
          state VARCHAR(50) NOT NULL,
          uploaded_at timestamp NOT NULL
      );
EOF

  # Create Metrics Collect Function App in Azure

  # Will need to revisit how to successfully deploy this app through an arm template
  # Need a storage account to publish function app to:
  echo "Creating storage account for $METRICS_COLLECT_APP_NAME"
  az storage account create \
    --name "$COLLECT_STORAGE_NAME" \
    --location "$LOCATION" \
    --resource-group "$METRICS_RESOURCE_GROUP" \
    --sku Standard_LRS \
    --tags Project=$PROJECT_TAG

  # Create the function app in Azure
  echo "Creating function app $METRICS_COLLECT_APP_NAME in Azure"
  az functionapp create \
    --resource-group "$METRICS_RESOURCE_GROUP" \
    --plan "$APP_SERVICE_PLAN_FUNC_NAME" \
    --runtime dotnet \
    --functions-version 3 \
    --name "$METRICS_COLLECT_APP_NAME" \
    --storage-account "$COLLECT_STORAGE_NAME" \
    --tags Project=$PROJECT_TAG

  # Integrate function app into Virtual Network
  echo "Integrating $METRICS_COLLECT_APP_NAME into virtual network"
  az functionapp vnet-integration add \
    --name "$METRICS_COLLECT_APP_NAME" \
    --resource-group "$METRICS_RESOURCE_GROUP" \
    --subnet "$FUNC_SUBNET_NAME" \
    --vnet "$VNET_NAME"

  # Configure log streaming for function app
  metrics_collect_function_id=$(\
    az functionapp show \
      -n "$METRICS_COLLECT_APP_NAME" \
      -g "$METRICS_RESOURCE_GROUP" \
      -o tsv \
      --query id)
  hub_rule_id=$(\
    az eventhubs namespace authorization-rule list \
      --resource-group "$RESOURCE_GROUP" \
      --namespace-name "$EVENT_HUB_NAME" \
      --query "[?name == 'RootManageSharedAccessKey'].id" \
      -o tsv)

  az monitor diagnostic-settings create \
    --name "stream-logs-to-event-hub" \
    --resource "$metrics_collect_function_id" \
    --event-hub "logs" \
    --event-hub-rule "$hub_rule_id" \
    --logs '[
      {
        "category": "FunctionAppLogs",
        "enabled": true
      }
    ]'

  # Waiting before publishing the app, since publishing immediately after creation returns an   App Not Found error
  # Waiting was the best solution I could find. More info in these GH issues:
  # https://github.com/Azure/azure-functions-core-tools/issues/1616
  # https://github.com/Azure/azure-functions-core-tools/issues/1766
  echo "Waiting to publish function app"
  sleep 60

  echo "configure settings"
  az functionapp config appsettings set \
    --resource-group "$METRICS_RESOURCE_GROUP" \
    --name "$METRICS_COLLECT_APP_NAME" \
    --settings \
      $DB_CONN_STR_KEY="$DB_CONN_STR" \
      $VAULT_NAME_KEY="$VAULT_NAME" \
      $CLOUD_NAME_STR_KEY="$CLOUD_NAME" \
    --output none

  # Connect creds from function app to key vault so app can connect to db
  principalId=$(az functionapp identity assign \
    --resource-group "$METRICS_RESOURCE_GROUP" \
    --name "$METRICS_COLLECT_APP_NAME" \
    --query principalId \
    --output tsv)

  az keyvault set-policy \
    --name "$VAULT_NAME" \
    --object-id "$principalId" \
    --secret-permissions get list

  # publish the function app
  echo "Publishing function app $METRICS_COLLECT_APP_NAME"
  pushd ../metrics/src/Piipan.Metrics/$COLLECT_APP_FILEPATH
    func azure functionapp publish "$METRICS_COLLECT_APP_NAME" --dotnet
  popd

  # Subscribe each dynamically created event blob topic to this function
  METRICS_PROVIDERS=/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${METRICS_RESOURCE_GROUP}/providers
  SUBS_RESOURCE_GROUP=$RESOURCE_GROUP

  while IFS=, read -r abbr name ; do
      echo "Subscribing to ${name} blob events"
      abbr=$(echo "$abbr" | tr '[:upper:]' '[:lower:]')
      sub_name=evgs-${abbr}metricsupload-${ENV}
      topic_name=$(state_event_grid_topic_name "$abbr" "$ENV")

      az eventgrid system-topic event-subscription create \
          --name "$sub_name" \
          --resource-group "$SUBS_RESOURCE_GROUP" \
          --system-topic-name "$topic_name" \
          --endpoint "${METRICS_PROVIDERS}/Microsoft.Web/sites/${METRICS_COLLECT_APP_NAME}/functions/${COLLECT_FUNC}" \
          --endpoint-type azurefunction \
          --included-event-types Microsoft.Storage.BlobCreated \
          --subject-begins-with /blobServices/default/containers/upload/blobs/
  done < states.csv

  # Create Metrics API Function App in Azure

  # Will need to revisit how to successfully deploy this app through an arm template
  # Need a storage account to publish function app to:
  echo "Creating storage account for metrics api"
  az storage account create \
    --name "$API_APP_STORAGE_NAME" \
    --location "$LOCATION" \
    --resource-group "$METRICS_RESOURCE_GROUP" \
    --sku Standard_LRS \
    --tags Project=$PROJECT_TAG

  # Create the function app in Azure
  echo "Creating function app $METRICS_API_APP_NAME"
  az functionapp create \
    --resource-group "$METRICS_RESOURCE_GROUP" \
    --plan "$APP_SERVICE_PLAN_FUNC_NAME" \
    --runtime dotnet \
    --functions-version 3 \
    --name "$METRICS_API_APP_NAME" \
    --storage-account "$API_APP_STORAGE_NAME" \
    --tags Project=$PROJECT_TAG

  # Integrate function app into Virtual Network
  echo "Integrating $METRICS_API_APP_NAME into virtual network"
  az functionapp vnet-integration add \
    --name "$METRICS_API_APP_NAME" \
    --resource-group "$METRICS_RESOURCE_GROUP" \
    --subnet "$FUNC_SUBNET_NAME" \
    --vnet "$VNET_NAME"

  az functionapp config appsettings set \
      --resource-group "$METRICS_RESOURCE_GROUP" \
      --name "$METRICS_API_APP_NAME" \
      --settings \
        $DB_CONN_STR_KEY="$DB_CONN_STR" \
        $VAULT_NAME_KEY="$VAULT_NAME" \
        $CLOUD_NAME_STR_KEY="$CLOUD_NAME" \
      --output none

  # Configure log streaming for function app
  metrics_api_function_id=$(\
    az functionapp show \
      -n "$METRICS_API_APP_NAME" \
      -g "$METRICS_RESOURCE_GROUP" \
      -o tsv \
      --query id)
  hub_rule_id=$(\
    az eventhubs namespace authorization-rule list \
      --resource-group "$RESOURCE_GROUP" \
      --namespace-name "$EVENT_HUB_NAME" \
      --query "[?name == 'RootManageSharedAccessKey'].id" \
      -o tsv)

  az monitor diagnostic-settings create \
    --name "stream-logs-to-event-hub" \
    --resource "$metrics_api_function_id" \
    --event-hub "logs" \
    --event-hub-rule "$hub_rule_id" \
    --logs '[
      {
        "category": "FunctionAppLogs",
        "enabled": true
      }
    ]'

  # Connect creds from function app to key vault so app can connect to db
  principalId=$(az functionapp identity assign \
    --resource-group "$METRICS_RESOURCE_GROUP" \
    --name "$METRICS_API_APP_NAME" \
    --query principalId \
    --output tsv)

  az keyvault set-policy \
    --name "$VAULT_NAME" \
    --object-id "$principalId" \
    --secret-permissions get list

  echo "Waiting to publish function app"
  sleep 60

  # publish metrics function app
  echo "Publishing function app $METRICS_API_APP_NAME"
  pushd ../metrics/src/Piipan.Metrics/$API_APP_FILEPATH
    func azure functionapp publish "$METRICS_API_APP_NAME" --dotnet
  popd

  ## Dashboard stuff

  echo "Create Front Door and WAF policy for dashboard app"
  suffix=$(web_app_host_suffix)
  dashboard_host=${DASHBOARD_APP_NAME}${suffix}
  ./add-front-door-to-app.bash \
    "$azure_env" \
    "$RESOURCE_GROUP" \
    "$DASHBOARD_FRONTDOOR_NAME" \
    "$DASHBOARD_WAF_NAME" \
    "$dashboard_host"

  front_door_id=$(\
  az network front-door show \
    --name "$DASHBOARD_FRONTDOOR_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query frontdoorId \
    --output tsv)
  echo "Front Door iD: ${front_door_id}"

  metrics_api_uri=$(\
    az functionapp function show \
      -g "$METRICS_RESOURCE_GROUP" \
      -n "$METRICS_API_APP_NAME" \
      --function-name $METRICS_API_FUNCTION_NAME \
      --query invokeUrlTemplate \
      --output tsv)

  # Create App Service resources for dashboard app
  echo "Creating App Service resources for dashboard app"
  az deployment group create \
    --name "$DASHBOARD_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --template-file ./arm-templates/dashboard-app.json \
    --parameters \
      location="$LOCATION" \
      resourceTags="$RESOURCE_TAGS" \
      appName="$DASHBOARD_APP_NAME" \
      servicePlan="$APP_SERVICE_PLAN" \
      frontDoorId="$front_door_id" \
      metricsApiUri="$metrics_api_uri" \
      eventHubName="$EVENT_HUB_NAME"

  echo "Secure database connection"
  ./remove-external-network.bash \
    "$azure_env" \
    "$METRICS_RESOURCE_GROUP" \
    "$DB_SERVER_NAME"

  script_completed
}
main "$@"
