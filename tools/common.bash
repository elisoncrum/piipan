#!/bin/bash
#
# Common preamble for bash scripts.

# Exit immediately if a command fails
set -e

# Unset variables are an error
set -u

# Conform more closely to POSIX standard, specifically so command substitutions
# inherit the value of the -e option. The more targeted inherit_errexit option
# would be preferred, but it is not available in bash 3.2, which ships with macOS.
set -o posix

# Exit immediately if any command in a pipeline errors
set -o pipefail

# Inherit trap on ERR in shell functions, command substitutions, etc.
set -o errtrace

_script=$(basename "$0")

_err_report () {
  # If the error occurred in a while/done loop or in a function, the trap can
  # only report the line number of the loop or the function, not the offending
  # command itself.
  echo "$_script: error on (or around) line $1"
}

trap '_err_report $LINENO' ERR

script_completed () {
  echo "$_script: completed successfully"
}
