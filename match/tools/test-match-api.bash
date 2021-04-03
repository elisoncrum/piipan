#!/bin/bash
#
# Basic integration test tool for the orchestrator and per-state APIs. Sends a curl
# request with a pre-set PII query to the API's query endpoint and writes result to
# stdout. Requires that the user has been added to the necessary application role
# for the function app (see Remote Testing in match/docs/orchestrator-match.md and
# match/docs/state-match.md).
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# function-name is the function app name in Azure.
#
# usage: test-lookup-api.bash <azure-env> <function-name>

source $(dirname "$0")/../../tools/common.bash || exit
source $(dirname "$0")/../../iac/iac-common.bash || exit

QUERY_API_FUNC_NAME="query"
JSON='{
    "query": {
        "last": "Lynn",
        "dob": "1940-08-01",
        "ssn": "000-12-3457",
        "first": "Wesley",
        "middle": "Eura"
    }
}'

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source $(dirname "$0")/../../iac/env/${azure_env}.bash
  verify_cloud

  name=$2

  resource_uri=$(\
    az functionapp show \
      -g $MATCH_RESOURCE_GROUP \
      -n $name \
      --query defaultHostName \
      -o tsv)
  resource_uri="https://${resource_uri}"

  echo "Retrieving access token from ${resource_uri}"
  token=$(\
    az account get-access-token \
      --resource ${resource_uri} \
      --query accessToken \
      -o tsv
  )

  endpoint_uri=$(\
    az functionapp function show \
      -g $MATCH_RESOURCE_GROUP \
      -n $name \
      --function-name $QUERY_API_FUNC_NAME \
      --query invokeUrlTemplate \
      -o tsv)

  echo "Submitting request to ${endpoint_uri}"
  curl \
    --request POST "${endpoint_uri}" \
    --header "Authorization: Bearer ${token}" \
    --header 'Content-Type: application/json' \
    --data-raw "$JSON" \
    --include

  echo "\n"

  script_completed

}

main "$@"
