#!/bin/bash
#
# Creates the API Management instance for managing the NAC API. Assumes
# an Azure user with the Global Administrator role has signed in with
# the Azure CLI. See install-extensions.bash for prerequisite Azure CLI
# extensions. Deployment can take ~45 minutes for new instances.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# admin-email is the email address to use for the required "publisher
# email" property. A notification will be sent to the email when the
# instance has been created.
#
# usage: create-apim.bash <azure-env> <admin-email>

source $(dirname "$0")/../tools/common.bash || exit
source $(dirname "$0")/iac-common.bash || exit

clean_defaults () {
  group=$1
  apim=$2
  
  # Delete "echo API" example API
  az apim api delete \
    --api-id echo-api \
    -g ${group} \
    -n ${apim_name} \
    -y
  
  # Delete default "Starter" and "Unlimited" products and their associated
  # product subscriptions
  az apim product delete \
    --product-id starter \
    --delete-subscriptions true \
    -g ${group} \
    -n ${apim_name} \
    -y
  
  az apim product delete \
    --product-id unlimited \
    --delete-subscriptions true \
    -g ${group} \
    -n ${apim_name} \
    -y
}

main () {

  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source $(dirname "$0")/env/${azure_env}.bash
  verify_cloud

  APIM_NAME='nac-api'
  PUBLISHER_NAME='NAC Administrator'
  publisher_email=$2
  orch_name=$(get_resources $ORCHESTRATOR_API_TAG $MATCH_RESOURCE_GROUP)

  apim_name=$(\
    az deployment group create \
      --name apim-dev \
      --resource-group $MATCH_RESOURCE_GROUP \
      --template-file ./arm-templates/apim.json \
      --query properties.outputs.apimName.value \
      --output tsv \
      --parameters \
        apiName=$APIM_NAME \
        publisherEmail=$publisher_email \
        publisherName="$PUBLISHER_NAME" \
        location=$LOCATION \
        functionAppName=$orch_name \
        resourceTags="$RESOURCE_TAGS")

  # Clear out default example resources
  # See: https://stackoverflow.com/a/64297708
  clean_defaults $MATCH_RESOURCE_GROUP $apim_name
}

main "$@"
