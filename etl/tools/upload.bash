#!/bin/bash
#
# Uploads a file to a storage account, in the `upload` container. Used for
# ad-hoc testing of the bulk import of participant records into a per-state
# database. Requires that the user has write privileges on the account; use
# grant-blob.bash to establish those privs.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: upload.bash <azure-env> <path-to-file> <storage-account>

source $(dirname "$0")/../../tools/common.bash || exit
source $(dirname "$0")/../../iac/iac-common.bash || exit

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source $(dirname "$0")/../../iac/env/${azure_env}.bash
  verify_cloud

  file_path=$2
  storage_account=$3

  az storage blob upload \
    --account-name $storage_account \
    --container-name upload \
    --name `basename $file_path` \
    --file $file_path \
    --auth-mode login
}

main "$@"
