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

These instructions assume that the [piipan infrastructure](../../docs/iac.md) has been established in the Azure subscription and an administrator has signed in with the Azure CLI. 

All of the underlying databases and service bindings to the Function App via environment variables (i.e., `DatabaseConnectionString` and `BlobStorageConnectionString`) are handled by the Intrastructure-as-Code.

### App deployment

To republish the `BulkUpload` Azure Function for a specific state:
```
func azure functionapp publish function-app-name
```

## Ad-hoc testing

In a development environment, the `upload.bash` tool can be used to upload test CSV files to a storage account.
```
./etl/tools/upload.bash docs/csv/example.csv storage-account-name
```

`upload.bash` uses the credentials of the signed in Azure administrator to access the storage accounts. Privileges to perform that operation have to be explicitly granted:
```
./etl/tools/grant-blob.bash storage-account-name
``` 

While `grant-blob.bash` may return within a few seconds, it take can up to several minutes for Azure to replicate the privileges across its internal infrastructure; e.g., if `upload.bash` fails right after `grant-blob.bash` has been run, try it again in a couple of minutes.