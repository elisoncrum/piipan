#!/bin/bash
#
# Basic integration test tool for the lookup API. Sends a curl request to the
# API's lookup_ids endpoint and writes result to stdout. Requires that the user
# has been added to the OrchestratorApi.Query application role for the orchestrator
# function app (see match/docs/orchestrator-match.md#remote-testing).
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# lookup-id is the lookup ID string.
#
# usage: test-lookup-api.bash <azure-env> <lookup-id>

source $(dirname "$0")/../../tools/common.bash || exit
source $(dirname "$0")/../../iac/iac-common.bash || exit

LOOKUP_API_FUNC_NAME="lookup_ids"

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source $(dirname "$0")/../../iac/env/${azure_env}.bash
  verify_cloud

  lookup_id=$2

  name=$(get_resources $ORCHESTRATOR_API_TAG $MATCH_RESOURCE_GROUP)
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
      --function-name $LOOKUP_API_FUNC_NAME \
      --query invokeUrlTemplate \
      -o tsv)
  endpoint_uri=$(echo $endpoint_uri | sed "s/{lookupid}/$lookup_id/")

  echo "Submitting request to ${endpoint_uri}"
  curl \
    --request GET "${endpoint_uri}" \
    --header "Authorization: Bearer ${token}" \
    --include

  echo "\n"

  script_completed

}

main "$@"
