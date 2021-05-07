#!/bin/bash
#
# Supporting functionality for subsystem build scripts.
# Purpose of build scripts is to have platform-agnostic scripts
# that can be used for local development, CI, and IAC.
# Each subsystem has a top-level build.bash file that may
# use and/or override any of these functions.
# Build scripts rely on a solutions file (sln) in subsystem root.

# runs the build process
run_build () {
  echo "Running build"
  dotnet build
}

# runs all tests
run_tests () {
  echo "Running tests"
  dotnet test
}

# The Main runner for build scripts
# switches between build modes (build, test, deploy)
main () {
  mode=${1:-build} # set default mode to "build"
  azure_env=""

  case "$mode" in
    deploy)
      shift # Remove `deploy` from the argument list

    while getopts ":e:" opt; do
      case ${opt} in
        e )
          azure_env=$OPTARG
          ;;
        \? )
          echo "Invalid Option: -$OPTARG" 1>&2
          exit 1
          ;;
        : )
          echo "Invalid Option: -$OPTARG requires an argument" 1>&2
          exit 1
          ;;
      esac
    done
    shift $((OPTIND -1))
    ;;
  esac

  if [ "$mode" = "build" ];   then run_build; fi
  if [ "$mode" = "test" ];    then run_tests; fi
  if [[ "$mode" = "deploy" ]]; then
    if [[ "$azure_env" = "" ]]; then
      echo "You must specify an azure environment using the -e flag"
      echo "Example: ./build.bash deploy -e tts/dev"
      exit 1
    else
      run_deploy $azure_env
    fi
  fi

  script_completed
}
