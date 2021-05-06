#!/bin/bash
#
# Builds dashboard project
#
# Optional Flags:
# -t        Run tests only
# -d [env]  Build and deploy to specified Azure Environment (e.g. tts/dev)
#
# Usage:
# ./build.bash
# ./build.bash -t
# ./build.bash -d tts/dev

source $(dirname "$0")/../tools/common.bash || exit

# set modes
test_mode="false"
azure_env="none"

while getopts ":td:" arg; do
	case "${arg}" in
		t) test_mode="true" ;;
    d) azure_env=$OPTARG ;;
	esac
done
# end set modes

set_constants () {
  DASHBOARD_APP_NAME=$PREFIX-app-dashboard-$ENV # TODO: make this DRY
}

run_tests () {
  echo "Running tests"
  dotnet test
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
    -n $DASHBOARD_APP_NAME \
    --src ./artifacts/dashboard.zip
}

main () {
  test_mode=$1
  azure_env=$2

  dotnet build

  if [ "$test_mode" = "true" ]; then
    run_tests
  fi
  if [ "$azure_env" != "none" ]; then
    run_deploy $azure_env
  fi

  script_completed
}

main $test_mode $azure_env
