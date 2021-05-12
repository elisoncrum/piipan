#!/bin/bash
#
# Grants the current user the ability to write a blob to the specified
# storage account. Used in conjunction with upload.bash for ad-hoc testing.
# Only intended for development environments.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: grant-blob.bash <azure-env> <storage-account>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/../../iac/env/"${azure_env}".bash
  source "$(dirname "$0")"/../../iac/iac-common.bash
  verify_cloud

  storage_account=$2

  # The default Azure subscription
  SUBSCRIPTION_ID=$(az account show --query id -o tsv)

  # Many CLI commands use a URI to identify nested resources; pre-compute the URI's prefix
  # for our default resource group
  DEFAULT_PROVIDERS=/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP}/providers

  assignee=$(az ad signed-in-user show --query objectId -o tsv)

  az role assignment create \
    --role "Storage Blob Data Contributor" \
    --assignee "$assignee" \
    --scope "${DEFAULT_PROVIDERS}/Microsoft.Storage/storageAccounts/${storage_account}"
}

main "$@"
