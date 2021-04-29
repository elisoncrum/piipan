#!/bin/bash
# Configures Easy Auth for internal API communication within Piipan.
# Requires specific privileges on Azure Active Directory that the
# subscription Global Administrator role has, but a subscription
# Contributor does not.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: create-resources.bash <azure-env>

source $(dirname "$0")/../tools/common.bash || exit

# App Service Authentication is done at the Azure tenant level
TENANT_ID=$(az account show --query homeTenantId -o tsv)

# Generate the necessary JSON object for assigning an app role to
# a service principal or managed identity
app_role_assignment () {
  principalId=$1
  resourceId=$2
  appRoleId=$3

  echo "\
  {
    \"principalId\": \"${principalId}\",
    \"resourceId\": \"${resourceId}\",
    \"appRoleId\": \"${appRoleId}\"
  }"
}

# Generate the necessary JSON object for adding an app role
# to an Active Directory app registration
app_role_manifest () {
  role=$1

  json="\
  [{
    \"allowedMemberTypes\": [
      \"User\",
      \"Application\"
    ],
    \"description\": \"Grants application access\",
    \"displayName\": \"Authorized client\",
    \"isEnabled\": true,
    \"origin\": \"Application\",
    \"value\": \"${role}\"
  }]"
  echo $json
}

# Create an Active Directory app registration with an application
# role for a given application.
create_aad_app_reg () {
  app=$1
  role=$2
  resource_group=$3

  app_uri=$(\
    az functionapp show \
    --resource-group $resource_group \
    --name $app \
    --query defaultHostName \
    --output tsv)
  app_uri="https://${app_uri}"

  # Running `az ad app create` with the `--app-roles` parameter will throw
  # an error if the app already exists and the app role is enabled
  exists=$(\
    az ad app list \
    --display-name ${app} \
    --filter "displayName eq '${app}'" \
    --query "[0].appRoles[?value == '${role}'].value" \
    --output tsv)
  if [ -z "$exists" ]; then
    app_role=$(app_role_manifest $role)
    app_id=$(\
      az ad app create \
        --display-name $app \
        --app-roles "${app_role}" \
        --available-to-other-tenants false \
        --homepage $app_uri \
        --identifier-uris $app_uri \
        --reply-urls "${app_uri}/.auth/login/aad/callback" \
        --query objectId \
        --output tsv)
  else
    app_id=$(\
      az ad app list \
        --display-name ${app} \
        --filter "displayName eq '${app}'" \
        --query "[0].objectId" \
        --output tsv)
  fi

  echo $app_id
}

# Create a service principal associated with a given AAD
# application registration
create_aad_app_sp () {
  app=$1
  aad_app_id=$2
  filter="displayName eq '${app}' and servicePrincipalType eq 'Application'"

  # `az ad sp create` throws error if service principal exits
  sp=$(\
    az ad sp list \
    --display-name $app \
    --filter "${filter}" \
    --query "[0].objectId" \
    --output tsv)
  if [ -z "$sp" ]; then
    sp=$(\
      az ad sp create \
        --id $aad_app_id \
        --query objectId \
        --output tsv)
  fi

  echo $sp
}

# Assign an application role to a service principal (generally in
# the form of a managed identity)
assign_app_role () {
  echo "Assigning app role"
  resource_id=$1
  principal_id=$2
  role=$3
  role_id=$(\
    az ad sp show \
    --id $resource_id \
    --query "appRoles[?value == '${role}'].id" \
    --output tsv)

  domain=$(graph_host_suffix)

  # Similar to `az ad app create`, `az rest` will throw error when assigning
  # an app role to an identity that already has the role.
  exists=$(\
    az rest \
    --method GET \
    --uri "https://graph${domain}/v1.0/servicePrincipals/${resource_id}/appRoleAssignedTo" \
    --query "value[?principalId == '${principal_id}'].appRoleId" \
    --output tsv)

  if [ -z "$exists" ]; then
    role_json=`app_role_assignment $principal_id $resource_id $role_id`
    echo $role_json
    az rest \
    --method POST \
    --uri "https://graph${domain}/v1.0/servicePrincipals/${resource_id}/appRoleAssignedTo" \
    --headers 'Content-Type=application/json' \
    --body "$role_json"
  fi
}

# Activate App Service authentication (Easy Auth) for an app
# service or function app, and require app role assignment.
# Assumes Active Directory application and associated service
# principal already exist for the app
enable_easy_auth () {
  app=$1
  resource_group=$2

  app_uri=$(\
    az functionapp show \
    --resource-group $resource_group \
    --name $app \
    --query defaultHostName \
    --output tsv)
  app_uri="https://${app_uri}"

  app_aad_client=$(\
    az ad app list \
      --display-name ${app} \
      --filter "displayName eq '${app}'" \
      --query "[0].objectId" \
      --output tsv)

  sp_filter="displayName eq '${app}' and servicePrincipalType eq 'Application'"
  app_aad_sp=$(\
    az ad sp list \
      --display-name $app \
      --filter "${sp_filter}" \
      --query "[0].objectId" \
      --output tsv)

  echo "Configuring Easy Auth settings for ${app}"
  az webapp auth update \
    --resource-group $resource_group \
    --name $app \
    --aad-allowed-token-audiences $app_uri \
    --aad-client-id $app_aad_client \
    --aad-token-issuer-url "https://sts.windows.net/${TENANT_ID}/" \
    --enabled true \
    --action LoginWithAzureActiveDirectory

  # Any client that attemps authentication must be assigned a role
  az ad sp update \
    --id $app_aad_sp \
    --set "appRoleAssignmentRequired=true"
}

# Configures App Service Authentication (aka Easy Auth) for an API provider
# (a Function App) and an API client (either a Function App or App Service):
#    - Registers an Azure Active Directory (AAD) app with an application role
#      for the API provider.
#    - Create a service principal (SP) for the app registation.
#    - Add the application role to the client identity.
#    - Configure and enable App Service Authentiction (i.e., Easy Auth)
#      for the API provider.
#    - Enable requirement that authentication tokens are only issued to client
#      applications that are assigned an app role.
#
# <func> is the name of the API provider Function App
# <group> is the resource group <func> belongs to
# <role> is the Piipan role name
# <client_identity> is the principal id of the client Function App/App service
configure_easy_auth_pair () {
  local func=$1
  local group=$2
  local role=$3
  local client_identity=$4

  local func_app_reg_id
  func_app_reg_id=$(create_aad_app_reg $func $role $group)

  # Wait a bit to prevent "service principal being created must in the local tenant" error
  sleep 60
  local func_app_sp
  func_app_sp=$(create_aad_app_sp $func $func_app_reg_id)

  # Activate App Service Authentication for the Function App API
  enable_easy_auth $func $group

  # Give the client component access to the Function App API
  # Wait a bit to prevent ResourceNotFoundError
  sleep 60
  assign_app_role $func_app_sp $client_identity $role
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source $(dirname "$0")/env/${azure_env}.bash
  source $(dirname "$0")/iac-common.bash
  verify_cloud

  # Name of application roles authorized to call match APIs
  STATE_API_APP_ROLE='StateApi.Query'
  ORCH_API_APP_ROLE='OrchestratorApi.Query'

  match_func_names=($(\
    get_resources $PER_STATE_MATCH_API_TAG $RESOURCE_GROUP))

  orch_name=$(get_resources $ORCHESTRATOR_API_TAG $MATCH_RESOURCE_GROUP)

  query_tool_name=$(get_resources $QUERY_APP_TAG $RESOURCE_GROUP)

  dp_api_name=$(get_resources $DUP_PART_API_TAG $MATCH_RESOURCE_GROUP)

  orch_identity=$(\
    az webapp identity show \
      --name $orch_name \
      --resource-group $MATCH_RESOURCE_GROUP \
      --query principalId \
      --output tsv)

  query_tool_identity=$(\
    az webapp identity show \
      --name $query_tool_name \
      --resource-group $RESOURCE_GROUP \
      --query principalId \
      --output tsv)

  dp_api_identity=$(\
    az apim show \
      --name $dp_api_name \
      --resource-group $MATCH_RESOURCE_GROUP \
      --query identity.principalId \
      --output tsv)

  for func in "${match_func_names[@]}"
  do
    echo "Configure Easy Auth for PerStateMatchApi:${func} and OrchestratorApi"
    configure_easy_auth_pair \
      $func $RESOURCE_GROUP \
      $STATE_API_APP_ROLE \
      $orch_identity
  done

  echo "Configure Easy Auth for OrchestratorApi and QueryApp"
  configure_easy_auth_pair \
    $orch_name $MATCH_RESOURCE_GROUP \
    $ORCH_API_APP_ROLE \
    $query_tool_identity

  echo "Configure Easy Auth for OrchestratorApi and DupPartApi"
  configure_easy_auth_pair \
    $orch_name $MATCH_RESOURCE_GROUP \
    $ORCH_API_APP_ROLE \
    $dp_api_identity

  script_completed
}

main "$@"
