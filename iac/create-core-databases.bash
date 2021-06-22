#!/bin/bash

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

set_constants () {
  DB_SERVER_NAME=$PREFIX-psql-core-$ENV
  DB_ADMIN_NAME=piipanadmin
  DB_NAME=metrics
  DB_TABLE_NAME=participant_uploads
  VAULT_NAME=$PREFIX-kv-core-$ENV
  # Name of secret used to store the PostgreSQL metrics server admin password
  PG_SECRET_NAME=core-pg-admin
  PRIVATE_DNS_ZONE=$(private_dns_zone)
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud

  set_constants

  # Set PG Secret in key vault
  # By default, Azure CLI will print the password set in Key Vault; instead
  # just extract and print the secret id from the JSON response.
  echo "Setting key in vault"
  PG_SECRET=$(random_password)
  export PG_SECRET
  printenv PG_SECRET | tr -d '\n' | az keyvault secret set \
    --vault-name "$VAULT_NAME" \
    --name "$PG_SECRET_NAME" \
    --file /dev/stdin \
    --query id

  echo "Creating core database server"
  az deployment group create \
    --name core-db \
    --resource-group "$RESOURCE_GROUP" \
    --template-file ./arm-templates/database-core.json \
    --parameters \
      administratorLogin=$DB_ADMIN_NAME \
      serverName="$DB_SERVER_NAME" \
      secretName="$PG_SECRET_NAME" \
      vaultName="$VAULT_NAME" \
      vnetName="$VNET_NAME" \
      subnetName="$DB_2_SUBNET_NAME" \
      privateEndpointName="$CORE_DB_PRIVATE_ENDPOINT_NAME" \
      privateDnsZoneName="$PRIVATE_DNS_ZONE" \
      resourceTags="$RESOURCE_TAGS" \
      eventHubName="$EVENT_HUB_NAME"

  ### Database stuff
  # Create database within db server (command is idempotent)
  az postgres db create --name $DB_NAME --resource-group "$RESOURCE_GROUP" --server-name "$DB_SERVER_NAME"

  ## Connect to the db server
  # For some reason PG threw an Invalid Username error when trying to set PGUSER here, so it's specified in the prompt instead.
  # export PGUSER=${DB_ADMIN_NAME}@${DB_SERVER_NAME}
  PGHOST=$(az resource show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$DB_SERVER_NAME" \
    --resource-type "Microsoft.DbForPostgreSQL/servers" \
    --query properties.fullyQualifiedDomainName -o tsv)
  export PGHOST
  export PGPASSWORD=$PG_SECRET

  echo "Insert db table"
  psql -U "$DB_ADMIN_NAME@$DB_SERVER_NAME" -p 5432 -d "$DB_NAME" -w -v ON_ERROR_STOP=1 -X -q - <<EOF
      CREATE TABLE IF NOT EXISTS $DB_TABLE_NAME (
          id serial PRIMARY KEY,
          state VARCHAR(50) NOT NULL,
          uploaded_at timestamp NOT NULL
      );
EOF

  echo "Secure database connection"
  ./remove-external-network.bash \
    "$azure_env" \
    "$RESOURCE_GROUP" \
    "$DB_SERVER_NAME"

  script_completed
}

main "$@"
