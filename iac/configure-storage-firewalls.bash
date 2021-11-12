#!/usr/bin/env bash
#
# Configures network rules for storage accounts so that
# default network access is denied, and only necessary
# networks and resources are allowed.
#
# usage: configure-storage-firewalls.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

disallow_default () {
  local st_name=$1
  local rg=$2

  echo "Disallowing default network access to $st_name"
  az storage account update \
    --name "$st_name" \
    --resource-group "$rg" \
    --default-action Deny
}

allow_snet () {
  local st_name=$1
  local rg_name=$2
  local vnet_name=$3
  local snet_name=$4

  echo "Allowing $snet_name to access $blob_st_name"
    az storage account network-rule add \
      --account-name "$st_name" \
      --resource-group "$rg_name" \
      --vnet-name "$vnet_name" \
      --subnet "$snet_name"
}

allow_apim () {
  local st_name=$1
  local rg_name=$2
  local apim_id=$3
  local tenant_id=$4

  echo "Allowing $APIM_NAME to access $blob_st_name"
  az storage account network-rule add \
    --account-name "$st_name" \
    --resource-group "$rg_name" \
    --resource-id "$apim_id" \
    --tenant-id "$tenant_id"
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud

  tenant_id=$(az account show --query homeTenantId -o tsv)

  APIM_NAME=${PREFIX}-apim-duppartapi-${ENV}
  APIM_ID=$(\
    az apim show \
      --name "$APIM_NAME" \
      --resource-group "$MATCH_RESOURCE_GROUP" \
      --query id \
      --output tsv)

  while IFS=, read -r abbr name ; do
    abbr=$(echo "$abbr" | tr '[:upper:]' '[:lower:]')

    echo "Configuring storage firewalls something for $name ($abbr)"

    blob_st_name="${PREFIX}st${abbr}upload${ENV}"

    disallow_default "$blob_st_name" "$RESOURCE_GROUP"
    allow_snet "$blob_st_name" "$RESOURCE_GROUP" "$VNET_NAME" "$FUNC_SUBNET_NAME"
    allow_apim "$blob_st_name" "$RESOURCE_GROUP" "$APIM_ID" "$tenant_id"

    func_st_name=${PREFIX}st${abbr}etl${ENV}

    disallow_default "$func_st_name" "$RESOURCE_GROUP"
    allow_snet "$func_st_name" "$RESOURCE_GROUP" "$VNET_NAME" "$FUNC_SUBNET_NAME"

  done < states.csv

  script_completed
}

main "$@"
