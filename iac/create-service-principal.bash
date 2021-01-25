#!/bin/bash

source $(dirname "$0")/../tools/common.bash || exit

main () {
    # Required name parameter
    NAME=$1

    # Optional output format parameter. Printing service principal credentials
    # to stdout may not be desired in all contexts. If no output parameter is
    # passed, use Azure CLI's default: json.
    #
    # Ouput formats: https://docs.microsoft.com/en-us/cli/azure/format-output-azure-cli
    OUTPUT=${2:-json}

    # Array of groups to which the service principal will be scoped
    RESOURCE_GROUPS=(piipan-functions piipan-resources piipan-match)

    # Create space-separated list of resource group IDs
    for g in "${RESOURCE_GROUPS[@]}"
    do
        scope=`az group show -n $g --query id --output tsv`
        SCOPES="${SCOPES:+$SCOPES }$scope"
    done

    # If the service principal does not exist, the `create-for-rbac` command will
    # create it. If the service principal does exist, `create-for-rbac` will "patch"
    # it, creating new credentials and attempting to add the appropriate scopes.
    echo "Creating/resetting service principal $NAME"
    az ad sp create-for-rbac \
        --name $NAME \
        --role Contributor \
        --scopes $SCOPES \
        --only-show-errors \
        --output $OUTPUT

    script_completed
}

main "$@"
