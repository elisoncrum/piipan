#!/bin/bash
#
# Unit test all subsystems in repo
#
# Optional Flags:
# -c Run tests in continuous integration mode
#
# usage: ./unit-test-all.bash

source $(dirname "$0")/common.bash || exit

ci='false'

while getopts ':c' arg; do
	case "${arg}" in
		c) ci='true' ;;
	esac
done

subsystems=(dashboard etl match metrics query-tool shared)

for s in "${subsystems[@]}"
do
	pushd ../$s/
		echo "\nTesting ${s}"
		if [ "$ci" = "true" ]; then
			dotnet test -p:ContinuousIntegrationBuild=true \
				--collect:"XPlat Code Coverage" \
				-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=lcov
		else
			dotnet test
		fi
	popd
done

script_completed
