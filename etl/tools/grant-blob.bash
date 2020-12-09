#!/bin/bash
#
# Grants the current user the ability to write a blob to the specified
# storage account. Used in conjunction with upload.bash for ad-hoc testing.
# Only intended for development environments.
#
# usage: grant-blob.bash account-name

set -e
set -u

main () {
  storage_account=$1

  # XXX Constants duplicated from iac/create-resources.bash

  # Default resource group for our system
  RESOURCE_GROUP=piipan-resources

  # The default Azure subscription
  SUBSCRIPTION_ID=`az account show --query id -o tsv`

  # Many CLI commands use a URI to identify nested resources; pre-compute the URI's prefix
  # for our default resource group
  DEFAULT_PROVIDERS=/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP}/providers

  assignee=`az ad signed-in-user show --query objectId -o tsv`

  az role assignment create \
    --role "Storage Blob Data Contributor" \
    --assignee $assignee \
    --scope "${DEFAULT_PROVIDERS}/Microsoft.Storage/storageAccounts/${storage_account}"
}

main "$@"
