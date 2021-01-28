#!/bin/bash
#
# Provisions and configures the infrastructure components for all Piipan Metrics subsystems.
# Assumes an Azure user with the Global Administrator role has signed in with the Azure CLI.
# Assumes Piipan base resources have been created in the same environment
# (for example, state-sepcific blob topics).
# Must be run from a trusted network.
#
# usage: create-metrics-resources.bash

source $(dirname "$0")/../tools/common.bash || exit

### VARIABLES
# Default resource group for metrics system
RESOURCE_GROUP=piipan-metrics
LOCATION=westus
PROJECT_TAG=piipan
RESOURCE_TAGS="{ \"Project\": \"${PROJECT_TAG}\" }"
DB_SERVER_NAME=piipan-metrics-db
DB_ADMIN_NAME=piipanadmin
DB_PASSWORD=<dummy> # This is a dummy; TODO: rework how we access the DB
DB_PRICE_TIER=GP_Gen5_2 # TODO: finalize pricing tier for DB
DB_NAME=metrics
# Identity object ID for the Azure environment account
CURRENT_USER_OBJID=`az ad signed-in-user show --query objectId --output tsv`
# The default Azure subscription
SUBSCRIPTION_ID=`az account show --query id -o tsv`
# Get to local IP address
LOCAL_IPV4=`dig @resolver4.opendns.com myip.opendns.com +short -4`
### END VARIABLES

# Create Metrics resource group
echo "Creating $RESOURCE_GROUP group"
az group create --name $RESOURCE_GROUP -l $LOCATION --tags Project=$PROJECT_TAG

# Create Metrics database server if we don't have one yet
# server_exists=`az postgres server list --resource-group $RESOURCE_GROUP | grep $DB_SERVER_NAME`
server_exists=`az postgres server list --resource-group $RESOURCE_GROUP`
if [ "$server_exists" = "[]" ]; then
    echo "Creating Metrics database server"
    az postgres server create --resource-group $RESOURCE_GROUP --name $DB_SERVER_NAME  --location $LOCATION -u $DB_ADMIN_NAME -p $DB_PASSWORD --sku-name $DB_PRICE_TIER
    # set firewall rule so local ip can have access to db server
    echo "Set firewall rules for db server"
    # for local IP for in order to complete this script
    az postgres server firewall-rule create --resource-group $RESOURCE_GROUP --server $DB_SERVER_NAME --name AllowLocalIP --start-ip-address $LOCAL_IPV4 --end-ip-address $LOCAL_IPV4
    # for all Azure connections
    # https://docs.microsoft.com/en-us/azure/postgresql/howto-manage-firewall-using-cli#create-firewall-rule
    # TODO: lock this down to only our Azure resources
    az postgres server firewall-rule create --resource-group $RESOURCE_GROUP --server-name $DB_SERVER_NAME --name AllowAllAzureIps --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
fi

### Database stuff
# Create database within db server (command is idempotent)
az postgres db create --name $DB_NAME --resource-group $RESOURCE_GROUP --server-name $DB_SERVER_NAME

## Connect to the db server
# For some reason PG threw an Invalid Username error when trying to set PGUSER here, so it's specified in the prompt instead.
# export PGUSER=${DB_ADMIN_NAME}@${DB_SERVER_NAME}
export PGHOST=`az resource show \
  --resource-group $RESOURCE_GROUP \
  --name $DB_SERVER_NAME \
  --resource-type "Microsoft.DbForPostgreSQL/servers" \
  --query properties.fullyQualifiedDomainName -o tsv`
export PGPASSWORD=$DB_PASSWORD

# echo "PGUSER: ${PGUSER}"
echo "PGHOST: ${PGHOST}"

echo "Insert db table"
psql -U $DB_ADMIN_NAME@$DB_SERVER_NAME -p 5432 -d $DB_NAME -w -v ON_ERROR_STOP=1 -X -q - <<EOF
    CREATE TABLE IF NOT EXISTS user_uploads (
        id serial PRIMARY KEY,
        actor VARCHAR(50) NOT NULL,
        uploaded_at timestamp NOT NULL
    );
EOF

### Function App stuff
FUNC_APP_NAME=PiipanMetricsFunctions
FUNC_NAME=BulkUploadMetrics

# Need a storage account to publish function app to:
FUNC_STORAGE_NAME=piipanmetricsstorage
echo "Creating storage account $FUNC_STORAGE_NAME"
az storage account create --name $FUNC_STORAGE_NAME --location $LOCATION --resource-group $RESOURCE_GROUP --sku Standard_LRS

# Create the function app in Azure
echo "Creating function app $FUNC_APP_NAME in Azure"
az functionapp create --resource-group $RESOURCE_GROUP --consumption-plan-location $LOCATION --runtime dotnet --functions-version 3 --name $FUNC_APP_NAME --storage-account $FUNC_STORAGE_NAME

# Waiting before publishing the app, since publishing immediately after creation returns an App Not Found error
# Waiting was the best solution I could find. More info in these GH issues:
# https://github.com/Azure/azure-functions-core-tools/issues/1616
# https://github.com/Azure/azure-functions-core-tools/issues/1766
echo "Waiting to publish function app"
sleep 60s

# publish the function app
echo "Publishing function app $FUNC_APP_NAME"
pushd ../metrics/src/Piipan.Metrics/$FUNC_APP_NAME
  func azure functionapp publish $FUNC_APP_NAME --dotnet
popd

# Subscribe each dynamically created event blob topic to this function
FUNCTIONS_PROVIDERS=/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP}/providers
SUBS_RESOURCE_GROUP=piipan-resources

while IFS=, read -r abbr name ; do
    echo "Subscribing to ${name} blob events"
    abbr=`echo "$abbr" | tr '[:upper:]' '[:lower:]'`
    sub_name=${abbr}-blob-metrics-subscription
    topic_name=${abbr}-blob-topic

    az eventgrid system-topic event-subscription create \
        --name $sub_name \
        --resource-group $SUBS_RESOURCE_GROUP \
        --system-topic-name $topic_name \
        --endpoint ${FUNCTIONS_PROVIDERS}/Microsoft.Web/sites/${FUNC_APP_NAME}/functions/${FUNC_NAME} \
        --endpoint-type azurefunction \
        --included-event-types Microsoft.Storage.BlobCreated \
        --subject-begins-with /blobServices/default/containers/upload/blobs/
done < states.csv
