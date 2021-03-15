### This script creates an Azure Front Door with a WAF policy
### and applies it to a single web app.
###
### Run:
### ./iac/add-front-door-to-app.bash \
###   <env> \
###   <resource_group> \
###   <front_door_name> \
###   <app_host_name>
###
### Arguments must be ordered:
###   env (eg: tts/dev)
###   resource_group (eg: rg-core-dev)
###   front_door_name (eg: dashboard) Hyphens not allowed
###   app_host_name (eg: my-dashboard.azurewebsites.net)
###
### Example script:
### ./iac/add-front-door-to-app.bash tts/dev rg-core-dev piipandashboard piipan-dashboard-5or4l3nqzevf4.azurewebsites.net

source $(dirname "$0")/../tools/common.bash || exit
source $(dirname "$0")/iac-common.bash || exit

main () {
  azure_env=$1
  source $(dirname "$0")/env/${azure_env}.bash

  resource_group=$2
  front_door_name=$3 # no hyphens
  app_address=$4

  front_door_full_name=${PREFIX}-fd-${front_door_name}-${ENV}
  waf_full_name=${PREFIX}waf${front_door_name}${ENV} # Policy name must start with a letter and contain only numbers and letters

  echo "WAF name: ${waf_full_name}"

  az deployment group create \
    --name $front_door_full_name \
    --resource-group $resource_group \
    --template-file ./arm-templates/front-door-app-service.json \
    --parameters \
      appAddress=$app_address \
      frontDoorHostName="${front_door_full_name}.azurefd.net" \
      frontDoorName=$front_door_full_name \
      resourceGroupName=$resource_group \
      resourceTags="$RESOURCE_TAGS" \
      wafPolicyName=$waf_full_name

  script_completed
}
main "$@"
