#!/bin/bash
#
# Builds project with optional testing and app deployment
# Relies on a solutions file (sln) in the subsystem root directory
# See build-common.bash for usage details

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit
# shellcheck source=./tools/build-common.bash
source "$(dirname "$0")"/../tools/build-common.bash || exit

set_constants () {
   # TODO: make more DRY
  COLLECT_APP_ID=metricscol
  COLLECT_APP_NAME=${PREFIX}-func-${COLLECT_APP_ID}-${ENV}
  METRICS_API_APP_ID=metricsapi
  API_APP_NAME=$PREFIX-func-$METRICS_API_APP_ID-$ENV
}

run_deploy () {
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../iac/iac-common.bash

  verify_cloud
  set_constants

  echo "Publish ${COLLECT_APP_NAME} to Azure Environment ${azure_env}"
  pushd ./src/Piipan.Metrics/Piipan.Metrics.Collect
    func azure functionapp publish "$COLLECT_APP_NAME" --dotnet
  popd

  echo "Publish ${API_APP_NAME} to Azure Environment ${azure_env}"
  pushd ./src/Piipan.Metrics/Piipan.Metrics.Api
    func azure functionapp publish "$API_APP_NAME" --dotnet
  popd
}

main "$@"
