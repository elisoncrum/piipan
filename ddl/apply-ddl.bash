#!/bin/bash
#
# Creates participant records tables and their access controls for
# each configured state. PGHOST, PGUSER, PGPASSWORD must be set.
#
# usage: apply-ddl.bash

# shellcheck source=./iac/iac-common.bash
source "$(dirname "$0")"/../iac/iac-common.bash || exit

set -e

# PGUSER and PGPASSWORD should correspond to the out-of-the-box,
# non-AD "superuser" administrtor login
set -u
: "$PGHOST"
: "$PGUSER"
: "$PGPASSWORD"
: "$ENV"

# Azure user connection string will be of the form:
# administatorLogin@serverName
SUPERUSER=${PGUSER%@*}

export PGOPTIONS='--client-min-messages=warning'
PSQL_OPTS=(-v ON_ERROR_STOP=1 -X -q)

apply_ddl () {
  db=$1
  owner=$2
  admin=$3

  psql "${PSQL_OPTS[@]}" -d "$db" \
    -v owner="$owner" \
    -v admin="$admin" \
    -v superuser="$SUPERUSER" \
    -f ./per-state.sql
}

main () {
  while IFS=, read -r abbr _; do
    db=$(echo "$abbr" | tr '[:upper:]' '[:lower:]')
    owner=$db
    admin=$(state_managed_id_name "$db" "$ENV")
    admin=${admin//-/_}

    echo "Applying DDL to database $db..."
    apply_ddl "$db" "$owner" "$admin"

  done < ../iac/states.csv
}

main "$@"
