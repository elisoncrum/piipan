#!/bin/bash
#
# Assigns the CIS Microsoft Azure Foundations Benchmark policy set
# to the core and match resource groups. Assumes an Azure user with
# the Global Administrator role has signed in with the Azure CLI.
#
# usage: configure-cis-policy.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

set_constants () {
  # https://docs.microsoft.com/en-us/azure/governance/policy/samples/cis-azure-1-3-0
  # The policy "name" is the UUID of the set-definition
  CIS_POLICY_SET_DEFINITION_NAME="612b5213-9160-4969-8578-1518bd2a000c"
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud

  set_constants

  echo "Assigning $CIS_POLICY_SET_DEFINITION_NAME to $RESOURCE_GROUP"
  az policy assignment create \
    --policy-set-definition "$CIS_POLICY_SET_DEFINITION_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --name "cis-1_3-$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --assign-identity

  echo "Assigning $CIS_POLICY_SET_DEFINITION_NAME to $MATCH_RESOURCE_GROUP"
  az policy assignment create \
    --policy-set-definition "$CIS_POLICY_SET_DEFINITION_NAME" \
    --resource-group "$MATCH_RESOURCE_GROUP" \
    --name "cis-1_3-$MATCH_RESOURCE_GROUP" \
    --location "$LOCATION" \
    --assign-identity

  script_completed
}

main "$@"
