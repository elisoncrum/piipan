#!/bin/bash
#
# Basic remote test tool for the APIM match API. Sends a curl request
# with de-identified data to the API's find_matches endpoint and writes result to
# stdout.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: test-apim-match-api.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

SUBSCRIPTION_NAME="EA-DupPart"
MATCH_API_PATH="/match/v2/find_matches"

# Hash digest for farrington,10/13/31,000-12-3456
JSON='{
    "data": [{
      "lds_hash": "eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458"
    }]
}'

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../../iac/iac-common.bash
  verify_cloud

  serviceName=$(get_resources "$DUP_PART_API_TAG" "$MATCH_RESOURCE_GROUP")
  domain=$(apim_host_suffix)
  endpoint_uri="https://${serviceName}${domain}${MATCH_API_PATH}"

  api_key=$(\
    az rest \
    --method POST \
    --uri "https://management.azure.com/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${MATCH_RESOURCE_GROUP}/providers/Microsoft.ApiManagement/service/${serviceName}/subscriptions/${SUBSCRIPTION_NAME}/listSecrets?api-version=2020-12-01" \
    --query primaryKey \
    --output tsv)

  echo "Submitting request to ${endpoint_uri}"
  curl \
    --request POST "${endpoint_uri}" \
    --header 'Content-Type: application/json' \
    --header 'Accept: application/json' \
    --header "Ocp-Apim-Subscription-Key: ${api_key}" \
    --data-raw "$JSON" \
    --include
}
main "$@"
