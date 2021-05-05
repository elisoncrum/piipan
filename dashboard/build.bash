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
#
# Note: -t and -d flags are exclusive. When used together, it will only run in test mode.

source $(dirname "$0")/../tools/common.bash || exit

# set modes
test_mode="false"
azure_env=""

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

main () {
  test_mode=$1
  azure_env=$2
  source $(dirname "$0")/../iac/env/${azure_env}.bash
  source $(dirname "$0")/../iac/iac-common.bash
  verify_cloud

  set_constants

  # run build
  dotnet build

  if [ "$test_mode" = "true" ]; then
    echo "Running tests"
    dotnet test
  else
    echo "Publishing project"
    dotnet publish -o ./artifacts
    if [ "$azure_env" != "" ]; then
      echo "Deploying to Azure Environment ${azure_env}"
      pushd ./artifacts
        zip -r dashboard.zip .
      popd
      az webapp deployment \
        source config-zip \
        -g $RESOURCE_GROUP \
        -n $DASHBOARD_APP_NAME \
        --src ./artifacts/dashboard.zip
    fi
  fi

  script_completed
}

main $test_mode $azure_env
