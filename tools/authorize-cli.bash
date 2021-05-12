#!/bin/bash
#
# Add the Azure CLI as a "pre-authorized application" for the specified
# application object. Used to allow users to obtain an access token via
# the CLI (e.g., `az account get-access-token <app-uri>`). Access tokens
# can be used to call internal APIs which are protected by Easy Auth.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# app-uri is the application ID URI. See docs/securing-internal-apis.md.
#
# usage: assign-app-role.bash <azure-env> <app-uri>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/common.bash || exit

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../iac/iac-common.bash
  verify_cloud

  app_uri=$2

  # For references to hard-coded ID see:
  # - https://docs.microsoft.com/en-us/azure-stack/user/azure-stack-rest-api-use?view=azs-2008#example
  # - https://github.com/Azure/azure-cli/blob/24e0b9ef8716e16b9e38c9bb123a734a6cf550eb/src/azure-cli-core/azure/cli/core/_profile.py#L65
  CLI_ID="04b07795-8ddb-461a-bbee-02f9e1bf7b46"
  object_id=$(\
    az ad app show \
      --id "$app_uri" \
      --query objectId \
      -o tsv)
  # shellcheck disable=SC2016
  permission_id=$(\
    az rest \
      -m GET \
      -u "https://graph.microsoft.com/v1.0/applications/${object_id}" \
      --query 'api.oauth2PermissionScopes[?value == `user_impersonation`].id' \
      -o tsv)
  json="{
    \"api\": {
      \"preAuthorizedApplications\": [{
        \"appId\":\"${CLI_ID}\",
        \"delegatedPermissionIds\":[\"${permission_id}\"]
      }]
    }
  }"

  az rest \
    -m PATCH \
    -u "https://graph.microsoft.com/v1.0/applications/${object_id}" \
    --headers 'Content-Type=application/json' \
    --body "$json"
}

main "$@"
