#!/bin/bash
#
# Assigns the current CLI user to the specified application role of the
# specified Azure Function. Used to enable the use of Azure CLI credentials
# (i.e., `Azure.Identity.AzureCliCredential`) when connecting to remote APIs
# which have been secured using App Service Authentication. Only intended
# for development environments.
#
# usage: assign-app-role.bash function-name role-name

source common.bash || exit

main () {
  function=$1
  role=$2

  # Get function's application object
  application=$(\
    az ad app list \
      --display-name ${function} \
      --query "[0].appId" \
      --output tsv)

  # Get application role ID from application object
  role_id=$(\
    az ad app show \
      --id ${application} \
      --query "appRoles[?value == '${role}'].id" \
      --output tsv)

  # Get application object's service principal
  service_principal=$(\
    az ad sp list \
      --display-name ${function} \
      --filter "appId eq '${application}'" \
      --query [0].objectId \
      --output tsv)

  # Get user's Azure AD object ID
  user=$(\
    az ad signed-in-user show \
      --query objectId \
      --output tsv)

  json="\
  {
    \"principalId\": \"${user}\",
    \"resourceId\": \"${service_principal}\",
    \"appRoleId\": \"${role_id}\"
  }"

  # Assign application role
  az rest \
    --method POST \
    --uri "https://graph.microsoft.com/v1.0/users/${user}/appRoleAssignments" \
    --headers 'Content-Type=application/json' \
    --body "$json"
}

main "$@"
