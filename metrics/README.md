# Piipan Metrics

*Piipan subsystem for monitoring other subsystems*

## Sources

The `metrics/src` directory contains:

* [Metrics Functions Function App](./src/PiipanMetricsFunctions) - monitors the other subsystems and stores the metrics data
* [Metrics API Function App](./src/PiipanMetricsApi) - serves metrics data to external systems
* [Metrics Shared Models](./src/Piipan.Metrics.Models) - maps data from metrics db into application objects

## Summary

### Metrics Functions

Metrics Functions is an Azure Function App made up of Azure Event Grid event functions.

For now, there's a 1-1 relationship between a specific metric needing to be captured and a function within this app.

### Metrics API

The Metrics API is a Function App made up of Azure HTTPTrigger event functions.

For now, there's a 1-1 relationship between an API endpoint and a function within this app.

## Environment variables

The following environment variables are required by both the Metrics Functions and Metrics API apps and are set by the [IaC](../docs/iac.md):

| Name | |
|---|---|
| `DatabaseConnectionString` | [details](../../docs/iac.md#\:\~\:text=DatabaseConnectionString) |
| `CloudName` | [details](../../docs/iac.md#\:\~\:text=CloudName) |
| `KeyVaultName` | [details](../../docs/iac.md#\:\~\:text=KeyVaultName) |

## Local development

Local development is currently limited as a result of using a managed identity to connect to the metrics database. The Instance Metadata Service used by managed identities to retrieve authentication tokens is not available locally. There are [potential solutions](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/service-to-service-authentication#local-development-authentication) using the `Microsoft.Azure.Services.AppAuthentication` library. None have been implemented at this time.

The app will still build and run locally. However, any valid request sent to the local endpoint will result in an exception when the app attempts to retrieve an access token. Invalid requests (e.g., malformed or missing data in the request body) will return proper error responses.

To build and run the app with this limited functionality:

1. Fetch any app settings using `func azure functionapp fetch-app-settings {app-name}`. The app name can be retrieved from the Portal.
1. Run `func start` or, if hot reloading is desired, `dotnet watch msbuild /t:RunFunctions`.

## Testing

`Piipan.Metrics.Test` holds all tests for the Piipan Metrics Subsystem

``` bash
$ cd metrics/tests/Piipan.Metrics.Tests
```

If running for the first time, do `dotnet build`, then:

``` bash
$ dotnet test
```

