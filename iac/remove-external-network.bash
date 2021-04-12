#!/bin/bash
# This script removes all firewall policies from database before
# denying public access to database.
# Once done, the only access to database would be through its private endpoint.
#
# Arguments:
#   env (eg: tts/dev)
#   resource_group (eg: rg-core-dev)
#   server_name (eg: fns-db-participant-records-dev)
#
# Usage:
# ./iac/remove-external-network.bash tts/dev rg-core-dev fns-db-participant-records-dev

source $(dirname "$0")/../tools/common.bash || exit
source $(dirname "$0")/iac-common.bash || exit

main () {
  azure_env=$1
  source $(dirname "$0")/env/${azure_env}.bash
  verify_cloud

  resource_group=$2
  server_name=$3

	echo "Collect all firewall rules on database"
	ids=`az postgres server firewall-rule list \
		--resource-group $resource_group \
		--server-name $server_name \
		--query "[].id" \
		--output tsv`

	len=${#ids}
	if [ $len -gt 1 ]; then
		echo "Remove all firewall rules from database"
		az postgres server firewall-rule delete \
			--yes \
			--ids $ids
	fi

	echo "Deny public access to database"
	az postgres server update \
		--name $server_name \
		--resource-group $resource_group \
		--public-network-access Disabled

  script_completed
}
main "$@"
