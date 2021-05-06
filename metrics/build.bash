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

set_constants () {
   # TODO: make more DRY
  COLLECT_APP_ID=metricscol
  COLLECT_APP_NAME=${PREFIX}-func-${COLLECT_APP_ID}-${ENV}
  METRICS_API_APP_ID=metricsapi
  API_APP_NAME=$PREFIX-func-$METRICS_API_APP_ID-$ENV
}

run_deploy () {
  azure_env=$1
  source $(dirname "$0")/../iac/env/${azure_env}.bash
  source $(dirname "$0")/../iac/iac-common.bash
  verify_cloud

  set_constants

  echo "\nPublish ${COLLECT_APP_NAME} to Azure Environment ${azure_env}"
  pushd ./src/Piipan.Metrics/Piipan.Metrics.Collect
    func azure functionapp publish $COLLECT_APP_NAME --dotnet
  popd

  echo "\nPublish ${API_APP_NAME} to Azure Environment ${azure_env}"
  pushd ./src/Piipan.Metrics/Piipan.Metrics.Api
    func azure functionapp publish $API_APP_NAME --dotnet
  popd
}

main $@
