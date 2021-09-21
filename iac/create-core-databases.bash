#!/bin/bash

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

set_constants () {
  DB_SERVER_NAME=$PREFIX-psql-core-$ENV
  DB_ADMIN_NAME=piipanadmin
  SUPERUSER=$DB_ADMIN_NAME

  METRICS_DB_NAME=metrics

  VAULT_NAME=$PREFIX-kv-core-$ENV
  PG_SECRET_NAME=core-pg-admin

  # Name of Azure Active Directory admin for PostgreSQL server
  PG_AAD_ADMIN=piipan-admins

  PRIVATE_DNS_ZONE=$(private_dns_zone)
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  # shellcheck source=./iac/db-common.bash
  source "$(dirname "$0")"/db-common.bash
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

  db_set_env "$RESOURCE_GROUP" "$DB_SERVER_NAME" "$DB_ADMIN_NAME" "$PG_SECRET"

  # Create and configure metrics DB
  db_init "$METRICS_DB_NAME" "$SUPERUSER"

  echo "Create $METRICS_DB_NAME table"
  db_apply_ddl "$METRICS_DB_NAME" ../metrics/ddl/metrics.sql

  db_config_aad "$RESOURCE_GROUP" "$DB_SERVER_NAME" "$PG_AAD_ADMIN"
  db_use_aad "$DB_SERVER_NAME" "$PG_AAD_ADMIN"

  echo "Configuring database access for $METRICS_API_APP_NAME"
  db_create_managed_role "$METRICS_DB_NAME" "$METRICS_API_APP_NAME"
  db_config_managed_role "$METRICS_DB_NAME" "$METRICS_API_APP_NAME"
  db_grant_read "$METRICS_DB_NAME" "$METRICS_API_APP_NAME"

  echo "Configuring database access for $METRICS_COLLECT_APP_NAME"
  db_create_managed_role "$METRICS_DB_NAME" "$METRICS_COLLECT_APP_NAME"
  db_config_managed_role "$METRICS_DB_NAME" "$METRICS_COLLECT_APP_NAME"
  db_grant_readwrite "$METRICS_DB_NAME" "$METRICS_COLLECT_APP_NAME"

  db_leave_aad $PG_AAD_ADMIN

  echo "Secure database connection"
  ./remove-external-network.bash \
    "$azure_env" \
    "$RESOURCE_GROUP" \
    "$DB_SERVER_NAME"

  script_completed
}

main "$@"
