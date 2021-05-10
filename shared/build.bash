#!/bin/bash
#
# Builds project with optional testing and app deployment
# Relies on a solutions file (sln) in the subsystem root directory
# See build-common.bash for usage details

source $(dirname "$0")/../tools/common.bash || exit
source $(dirname "$0")/../tools/build-common.bash || exit

run_deploy () {
  echo "N/A"
}

main $@
