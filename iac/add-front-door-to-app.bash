source $(dirname "$0")/../tools/common.bash || exit
source $(dirname "$0")/iac-common.bash || exit

### Constants
set_constants () {
  # Set name of an existing resource group here:
  # example: rg-metrics-dev
  RESOURCE_GROUP=<add_here>

  # Override any resource tags not already set in iac-common:
  PROJECT_TAG=<add_here>
  RESOURCE_TAGS="{ \"Project\": \"${PROJECT_TAG}\" }"

  # Set WAF policy name to be created that Front Door will use:
  WAF_POLICY_NAME=<add_here>

  # Set name of Front Door resource to be created
  # example: my-dashboard
  # This name must correspond to the app name:
  FD_NAME=<add_here>

  # Set host name of app you want Front Door to manage
  # example: my-dashboard.azurefd.net
  # The first part of this host name must correspond to FD_NAME
  APP_HOST_NAME=<add_here>

  # Set web address of same app
  # example: my-dashboard.azurewebsites.net
  APP_ADDRESS=<add_here>
}
### END Constants

### Main
main () {
  set_constants

  az deployment group create \
    --name $FD_NAME \
    --resource-group $RESOURCE_GROUP \
    --template-file ./arm-templates/front-door-app-service.json \
    --parameters \
      appAddress=$APP_ADDRESS \
      appHostName=$APP_HOST_NAME \
      frontDoorName=$FD_NAME \
      resourceGroupName=$RESOURCE_GROUP \
      resourceTags="$RESOURCE_TAGS" \
      wafPolicyName=$WAF_POLICY_NAME

  script_completed
}
main "$@"
### END Main
