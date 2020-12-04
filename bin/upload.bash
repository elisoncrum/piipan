#!/bin/bash
#
# Uploads a file to a storage account, in the `upload` container. Used for
# ad-hoc testing of the bulk import of participant records into a per-state
# database. Requires that the user has write privileges on the account; use
# grant-blob.bash to establish those privs.
#
# usage: upload.bash path-to-file account-name

set -e
set -u

main () {
  file_path=$1
  storage_account=$2

  az storage blob upload \
    --account-name $storage_account \
    --container-name upload \
    --name `basename $file_path` \
    --file $file_path \
    --auth-mode login
}

main "$@"
