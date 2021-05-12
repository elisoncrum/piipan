#!/bin/bash
#
# Builds project with optional testing and app deployment
# Relies on a solutions file (sln) in the subsystem root directory
# See build-common.bash for usage details

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit
# shellcheck source=./iac/iac-common.bash
source "$(dirname "$0")"/../iac/iac-common.bash || exit

run_deploy () {
  echo "N/A"
}

main "$@"
