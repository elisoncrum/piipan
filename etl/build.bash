#!/bin/bash
#
# Builds project with optional testing and app deployment
# Relies on a solutions file (sln) in the subsystem root directory
#
# Arguments:
# build                   Build project binaries
# test                    Run tests
# deploy [-e <azure_env>] Deploy to specified Azure Environment (e.g. tts/dev)
#
# Description
# When deploying, an optional environment flag [-e] can be passed. Defaults to tts/dev.
#
# Usage:
# ./build.bash build
# ./build.bash test
# ./build.bash deploy
# ./build.bash deploy -e tts/test

source $(dirname "$0")/../tools/common.bash || exit
source $(dirname "$0")/../tools/build-common.bash || exit

run_deploy () {
  azure_env=$1
  source $(dirname "$0")/../iac/env/${azure_env}.bash
  source $(dirname "$0")/../iac/iac-common.bash
  verify_cloud

  etl_function_apps=($(get_resources $PER_STATE_ETL_TAG $RESOURCE_GROUP))

  for app in "${etl_function_apps[@]}"
  do
    echo "\nPublish ${app} to Azure Environment ${azure_env}"
    pushd ./src/Piipan.Etl
      func azure functionapp publish $app --dotnet
    popd
  done
}

main $@
