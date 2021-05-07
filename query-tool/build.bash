#!/bin/bash
#
# Builds project with optional testing and app deployment
# Relies on a solutions file (sln) in the subsystem root directory
#
# Arguments:
# [none]                Build project binaries
# test                  Run tests
# deploy -e <azure_env> Deploy to specified Azure Environment (e.g. tts/dev)
#
# Description:
# When passed no arguments, script runs in build mode.
# When deploying, an environment flag [-e] must be passed.
#
# Usage:
# ./build.bash
# ./build.bash test
# ./build.bash deploy
# ./build.bash deploy -e tts/test

source $(dirname "$0")/../tools/common.bash || exit
source $(dirname "$0")/../tools/build-common.bash || exit

set_constants () {
  QUERY_TOOL_APP_NAME=$PREFIX-app-querytool-$ENV # TODO: make this DRY
}

run_deploy () {
  azure_env=$1
  source $(dirname "$0")/../iac/env/${azure_env}.bash
  source $(dirname "$0")/../iac/iac-common.bash
  verify_cloud
  set_constants
  echo "Publishing project"
  dotnet publish -o ./artifacts
  echo "Deploying to Azure Environment ${azure_env}"
  pushd ./artifacts
    zip -r dashboard.zip .
  popd
  az webapp deployment \
    source config-zip \
    -g $RESOURCE_GROUP \
    -n $QUERY_TOOL_APP_NAME \
    --src ./artifacts/dashboard.zip
}

main $@
