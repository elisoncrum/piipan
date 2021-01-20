# Extract-Transform-Load process for PII bulk import

## Prerequisites
- [Azure Command Line Interface (CLI)](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [.NET Core SDK 3.1](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

## Summary

An initial approach for the bulk import of PII records into piipan has been implemented:
1. Begin with a state-specific CSV of PII records, in our [bulk import format](bulk-import.md).
1. The CSV is uploaded to a per-state storage account, in a container named `upload`.
1. An Event Grid blob creation event triggers a function named `BulkUpload` in a per-state Function App.
1. The function extracts the CSV records, performs basic validation, and inserts into a table in a per-state database. Any error in the CSV file will abort the entire upload.

While all states have separate storage accounts, function apps, and databases, the function code is identical across each state.

## Local development

To Be Determined

## Manual deployment

These instructions assume that the [piipan infrastructure](../../docs/iac.md) has been established in the Azure subscription and an administrator has signed in with the Azure CLI. A future iteration will incorporate managed identities and automate these steps, either in the Infrastructure-as-Code and/or in our CI/CD pipeline.

### Database setup

First, establish the tables in the per-state database, in the `participants-records` cluster, using the `postgres` account. The Azure portal can be used to reset the cluster password as necessary. Use the connection string provided by the portal to extract values for `PGHOST` and `PGUSER`.
```
cd ../ddl
export PGUSER=…
export PGHOST=…
export PGPASSWORD=…
./apply-ddl.bash
```

### Environment variables

Set the blob storage connection string in the `BlobStorageConnectionString` environment variable, on a per-state basis, for the Function App. Use an access key indicated by the portal for the state storage account (e.g., specific-storage-account → Settings → Access keys → key1)
```
az functionapp config appsettings set --name function-app-name --resource-group piipan-functions --settings BlobStorageConnectionString="…"
```

Set the database connection string in the `DatabaseConnectionString` environment variable, on a per-state basis, for the Function App. Use the ADO.NET value indicated by the portal for the `participants-records` cluster, modifying the string to reflect the specific state database (e.g, `ea`, `eb`, `ec`, etc.).
```
az functionapp config appsettings set --name function-app-name --resource-group piipan-functions --settings DatabaseConnectionString="…"
```

### App deployment

To republish the Azure Function to a specific state:
```
func azure functionapp publish function-app-name
```

## Ad-hoc testing

In a development environment, the `upload.bash` tool can be used to upload test CSV files to a storage account.
```
./etl/tools/upload.bash docs/csv/example.csv account-name
```
