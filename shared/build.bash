#!/bin/bash
#
# Builds project with optional testing and app deployment
# Relies on a solutions file (sln) in the subsystem root directory
#
# Arguments:
# [none]                Build project binaries
# test [-c]             Run tests
# deploy -e <azure_env> Deploy to specified Azure Environment (e.g. tts/dev)
#
# Description:
# When passed no arguments, script runs in build mode.
# When deploying, an environment flag [-e] must be passed.
# When testing, an optional flag [-c] can be passed to run in Continuous Integration mode.
#
# Usage:
# ./build.bash
# ./build.bash test
# ./build.bash test -c
# ./build.bash deploy
# ./build.bash deploy -e tts/test

source $(dirname "$0")/../tools/common.bash || exit
source $(dirname "$0")/../tools/build-common.bash || exit

run_deploy () {
  echo "N/A"
}

main $@
