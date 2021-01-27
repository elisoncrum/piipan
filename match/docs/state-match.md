# Per-state PII matching API

## Prerequisites
- [Azure Command Line Interface (CLI)](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [.NET Core SDK 3.1](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

## Summary

An initial API for matching PII on a per-state basis:
1. JSON `POST` request that conforms to the [OpenApi spec](openapi.md) is sent to the state-specific API endpoint.
1. The `POST` event triggers a function named `Query` in a per-state Function App.
    - If the request is not valid (malformed, missing required data, etc), the function returns a 400 response. Currently no error messaging is included in the response.
    - If the request is valid, the function uses a per-state managed identity to connect to the state-specific database in the `participant-records` cluster and queries for matching records. A 200 response is returned containing any matching record(s).

While all states have separate function apps, managed identities, and databases, the function code is identical across each state.

## Environment variables

The following environment variables are required by `Query` and are set by the [IaC](../../docs/iac.md):

| Name | |
|---|---|
| `DatabaseConnectionString` | [details](../../docs/iac.md#\:\~\:text=DatabaseConnectionString) |
| `StateName` | [details](../../docs/iac.md#\:\~\:text=StateName) |
| `StateAbbr` | [details](../../docs/iac.md#\:\~\:text=StateAbbr) |

## Local development

Local development is currently limited as a result of using a managed identity to connect to the state database. The Instance Metadata Service used by managed identities to retrieve authentication tokens is not available locally. There are [potential solutions](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/service-to-service-authentication#local-development-authentication) using the `Microsoft.Azure.Services.AppAuthentication` library. None have been implemented at this time.

The app will still build and run locally. However, any valid request sent to the local endpoint will result in an exception when the app attempts to retrieve an access token. Invalid requests (e.g., malformed or missing data in the request body) will return proper error responses.

To build and run the app with this limited functionality:

1. Fetch any app settings using `func azure functionapp fetch-app-settings {app-name}`. The app name can be retrieved from the Portal.
1. Run `func start` or, if hot reloading is desired, `dotnet watch msbuild /t:RunFunctions`.

## Manual deployment

These instructions assume that the [piipan infrastructure](../../docs/iac.md) has been established in the Azure subscription. Running the IaC will set up a Function app for each participating state. Each app is associated with a storage account, an application insights instance, a state-specific managed identity, and a state-specific database. All settings necessary for connecting the app to the state database are automatically stored in the app's configuration.

### Database setup

See the [ETL database setup instructions](../../etl/docs/etl.md#database-setup).

### App deployment

Deploy the app using the Functions Core Tools, making sure to pass the `--dotnet` flag:

```
func azure functionapp publish <app_name> --dotnet
```

`<app_name>` is the name of the Azure Function App resource created by the IaC process.

## Remote testing

The published app's `query` endpoint currently can be accessed over a trusted network. This functionality is only temporary, as the state-level API is intended to be restricted to internal use amongst Piipan's other subsystems.

To test the remote app:

1. Connect to a trusted network. Currently, only the GSA network block is trusted.
1. Send a valid `POST` request to the app's endpoint. The endpoint is in the format `https://{app-name}.azurewebsites.net/api/v1/query`.

Until the ETL process is updating to insert data into tables in the `piipan` schema, responses will not contain matches unless records have been manually imported into the database.
