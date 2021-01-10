#!/bin/bash
# 
# Creates participant records tables and their access controls for
# each configured state. PGHOST, PGUSER, PGPASSWORD must be set.
#
# usage: apply-ddl.bash

set -e

# PGUSER and PGPASSWORD should correspond to the out-of-the-box,
# non-AD "superuser" administrtor login
set -u
: "$PGHOST"
: "$PGUSER"
: "$PGPASSWORD"

# Azure user connection string will be of the form:
# administatorLogin@serverName
SUPERUSER=${PGUSER%@*}

export PGOPTIONS='--client-min-messages=warning'
PSQL_OPTS='-v ON_ERROR_STOP=1 -X -q'

apply_ddl () {
  db=$1
  owner=$2
  admin=$3

  psql $PSQL_OPTS -d $db \
    -v owner=$owner \
    -v admin=$admin \
    -v superuser=$SUPERUSER \
    -f ./per-state.sql
}

main () {
  while IFS=, read -r abbr name ; do
    db=`echo "$abbr" | tr '[:upper:]' '[:lower:]'`
    owner=$db
    admin=${db}admin
    
    echo "Applying DDL to database $db..."
    apply_ddl $db $owner $admin

  done < ../iac/states.csv
}

main "$@"
