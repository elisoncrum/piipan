#!/bin/bash
#
# Basic remote test tool for the orchestrator match API. Sends a curl request
# with de-identified data to the API's find_matches endpoint and writes result to
# stdout. Requires that the user has been added to the necessary application role
# for the function app (see Remote Testing in match/docs/orchestrator-match.md).
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: test-match-api.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

MATCH_API_FUNC_NAME="find_matches"
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

  name=$(get_resources "$ORCHESTRATOR_API_TAG" "$MATCH_RESOURCE_GROUP")

  resource_uri=$(\
    az functionapp show \
      -g "$MATCH_RESOURCE_GROUP" \
      -n "$name" \
      --query defaultHostName \
      -o tsv)
  resource_uri="https://${resource_uri}"

  echo "Retrieving access token from ${resource_uri}"
  token=$(\
    az account get-access-token \
      --resource "${resource_uri}" \
      --query accessToken \
      -o tsv
  )

  endpoint_uri=$(\
    az functionapp function show \
      -g "$MATCH_RESOURCE_GROUP" \
      -n "$name" \
      --function-name "$MATCH_API_FUNC_NAME" \
      --query invokeUrlTemplate \
      -o tsv)

  echo "Submitting request to ${endpoint_uri}"
  curl \
    --request POST "${endpoint_uri}" \
    --header "Authorization: Bearer ${token}" \
    --header 'Content-Type: application/json' \
    --data-raw "$JSON" \
    --include

  printf "\n"

  script_completed

}

main "$@"
