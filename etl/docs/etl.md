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

## Environment variables

The following environment variables are required by `BulkUpload` and are set by the [IaC](../../docs/iac.md):

| Name | |
|---|---|
| `DatabaseConnectionString` | [details](../../docs/iac.md#\:\~\:text=DatabaseConnectionString) |
| `BlobStorageConnectionString` | [details](../../docs/iac.md#\:\~\:text=BlobStorageConnectionString) |
| `CloudName` | [details](../../docs/iac.md#\:\~\:text=CloudName) |

## Local development

To Be Determined

## Manual deployment

### Prerequisites
1. The [Piipan infrastructure](../../docs/iac.md) has been established in the Azure subscription.
1. An administrator has signed in with the Azure CLI.

### App deployment
To republish the `BulkUpload` Azure Function for one specific state:
```
func azure functionapp publish <function-app-name> --dotnet
```

## Ad-hoc testing

In a development environment, the `test-storage-upload.bash` tool can be used to upload test CSV files to a storage account.

For example, if you are targeting the `tts/dev` environment and the `eastate5or4l3nqzevf4` storage account:
```
./tools/test-storage-upload.bash tts/dev ./docs/csv/example.csv eastate5or4l3nqzevf4
```

`test-storage-upload.bash` uses the credentials of the signed in Azure administrator to access the storage accounts. Privileges to perform that operation have to be explicitly granted on each storage account:
```
./tools/grant-blob.bash tts/dev eastate5or4l3nqzevf4
``` 

While `grant-blob.bash` may return within a few seconds, it take can up to several minutes for Azure to replicate the privileges across its internal infrastructure; e.g., if `test-storage-upload.bash` fails right after `grant-blob.bash` has been run, try it again in a couple of minutes.
