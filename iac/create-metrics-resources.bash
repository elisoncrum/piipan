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
DB_PASSWORD="<secure password>" # This is a dummy; TODO: rework how we access the DB
DB_PRICE_TIER=GP_Gen5_2 # TODO: finalize pricing tier for DB
DB_NAME=metrics
DB_TABLE_NAME=user_uploads
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
# if [ "$server_exists" = "" ]; then
#     # create database server
#     az postgres server create --resource-group $RESOURCE_GROUP --name $DB_SERVER_NAME  --location $LOCATION -u $DB_ADMIN_NAME -p $DB_PASSWORD --sku-name $DB_PRICE_TIER
#     # set firewall rule so local ip can have access to db server
#     az postgres server firewall-rule create --resource-group $RESOURCE_GROUP --server $DB_SERVER_NAME --name AllowMyIP --start-ip-address $LOCAL_IPV4 --end-ip-address $LOCAL_IPV4
# fi

## Connect to the db server
# USERNAME="${DB_ADMIN_NAME}@${DB_SERVER_NAME}"
# HOST="${DB_SERVER_NAME}.postgres.database.azure.com"
# CREATE_TABLE_SQL=<<EOF
#     CREATE TABLE IF NOT EXISTS user_uploads (
#         id serial PRIMARY KEY,
#         state_abbrev VARCHAR(50) NOT NULL,
#         uploaded_at timestamp NOT NULL
#     );
# EOF
## psql --host=$HOST --port=5432 --username=$USERNAME --dbname=postgres --password
# PGPASSWORD="$DB_PASSWORD" psql -t -A \
# -h "$HOST" \
# -p "5432" \
# -d "postgres" \
# -U "$USERNAME" \
# -c "$CREATE_TABLE_SQL"

# Commenting out Metrics DB creation for now so I can move on to function app and subscriptions
# LEFT OFF: need to verify I'm actually getting into psql prompt, sql commands seem to be silently failing

# Function App stuff
FUNC_APP_NAME=PiipanMetricsFunctions
FUNC_NAME=BulkUploadMetrics

# Need a storage account to publish function app to:
FUNC_STORAGE_NAME=piipanmetricsstorage
echo "Creating storage account $FUNC_STORAGE_NAME"
az storage account create --name $FUNC_STORAGE_NAME --location $LOCATION --resource-group $RESOURCE_GROUP --sku Standard_LRS

# Create the function app in Azure
echo "Creating function app $FUNC_APP_NAME in Azure"
az functionapp create --resource-group $RESOURCE_GROUP --consumption-plan-location $LOCATION --runtime dotnet --functions-version 3 --name $FUNC_APP_NAME --storage-account $FUNC_STORAGE_NAME

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
