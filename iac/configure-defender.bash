#!/usr/bin/env bash
#
# Enables Azure Defender at the subscription level for storage 
# accounts. Assumes an Azure user with the Global Administrator
# role has signed in with the Azure CLI.
#
# usage: configure-defender.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud

  az security pricing create \
    --name StorageAccounts \
    --tier standard

  script_completed
}

main "$@"
