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
  CIS_POLICY_DISPLAY_NAME="[Preview]: CIS Microsoft Azure Foundations Benchmark v1.3.0"
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud

  set_constants

  cis_policy_set_definition_name=$(az policy set-definition list \
    --query "[?displayName=='$CIS_POLICY_DISPLAY_NAME'] | [0] | name" \
    --output tsv)

  echo "Assigning $cis_policy_set_definition_name to $RESOURCE_GROUP"
  az policy assignment create \
    --policy-set-definition "$cis_policy_set_definition_name" \
    --resource-group "$RESOURCE_GROUP" \
    --name "cis-1_3-$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --assign-identity

  echo "Assigning $cis_policy_set_definition_name to $MATCH_RESOURCE_GROUP"
  az policy assignment create \
    --policy-set-definition "$cis_policy_set_definition_name" \
    --resource-group "$MATCH_RESOURCE_GROUP" \
    --name "cis-1_3-$MATCH_RESOURCE_GROUP" \
    --location "$LOCATION" \
    --assign-identity
}

main "$@"
