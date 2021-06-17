# Per-state PII matching API

## Prerequisites
- [Azure Command Line Interface (CLI)](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [.NET Core SDK 3.1](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [Docker](https://docs.docker.com/get-docker/) (for running integration tests)

## Summary

An initial API for matching PII on a per-state basis:
1. JSON `POST` request that conforms to the [OpenApi spec](openapi.md) is sent to the state-specific API endpoint.
1. The `POST` event triggers a function named `Query` in a per-state Function App.
    - If the request is not valid (malformed, missing required data, etc), the function returns a 400 response. Currently no error messaging is included in the response.
    - If the request is valid, the function uses a per-state managed identity to connect to the state-specific database in the `participant-records` cluster and queries for matching records. A 200 response is returned containing any matching record(s).

While all states have separate function apps, managed identities, databases, and Azure Active Directory resources, the function code is identical across each state.

## Environment variables

The following environment variables are required by `Query` and are set by the [IaC](../../docs/iac.md):

| Name | |
|---|---|
| `DatabaseConnectionString` | [details](../../docs/iac.md#\:\~\:text=DatabaseConnectionString) |
| `StateName` | [details](../../docs/iac.md#\:\~\:text=StateName) |
| `StateAbbr` | [details](../../docs/iac.md#\:\~\:text=StateAbbr) |
| `CloudName` | [details](../../docs/iac.md#\:\~\:text=CloudName) |

## Local development

Local development is currently limited as a result of using a managed identity to connect to the state database. The Instance Metadata Service used by managed identities to retrieve authentication tokens is not available locally. There are [potential solutions](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/service-to-service-authentication#local-development-authentication) using the `Microsoft.Azure.Services.AppAuthentication` library. None have been implemented at this time.

The app will still build and run locally. However, any valid request sent to the local endpoint will result in an exception when the app attempts to retrieve an access token. Invalid requests (e.g., malformed or missing data in the request body) will return proper error responses.

To build and run the app with this limited functionality:

1. Fetch any app settings using `func azure functionapp fetch-app-settings {app-name}`. The app name can be retrieved from the Portal.
1. Run `func start` or, if hot reloading is desired, `dotnet watch msbuild /t:RunFunctions`.

## Unit / integration tests

Unit tests are contained within `Piipan.Match.State.Tests` and integration tests within `Piipan.Match.State.IntegrationTests`.

To run unit tests:

```
cd match/tests/Piipan.Match.State.Tests
dotnet test
```

Integration tests are run by connecting to a PostgreSQL Docker container. With Docker installed on your machine, run the integration tests using Docker Compose:

```
cd match/tests/
docker-compose run --rm app dotnet test /code/match/tests/Piipan.Match.State.IntegrationTests/Piipan.Match.State.IntegrationTests.csproj
```

## Manual deployment

These instructions assume that the [piipan infrastructure](../../docs/iac.md) has been established in the Azure subscription.

Deploy the app using the Functions Core Tools, making sure to pass the `--dotnet` flag:

```
func azure functionapp publish <app_name> --dotnet
```

`<app_name>` is the name of the Azure Function App resource created by the IaC process.

## Authentication and authorization

Each Function App is configured to use Azure App Service Authentication (aka "Easy Auth") to control access and authenticate incoming requests. This configuration is performed as part of the IaC process. See [Securing our internal APIs](../../docs/securing-internal-apis.md) for implementation details.

## Remote testing

To test a deployed state API from your local environment:
1. Follow the [instructions](../../docs/securing-internal-apis.md) to assign your Azure user account the `StateApi.Query` role for the remote state Function App and authorize the Azure CLI.
1. Retrieve a token for your user using the Azure CLI: `az account get-access-token --resource <application ID URI>`.
1. Send a request to the remote endpoint—perhaps using a tool like Postman or `curl`—and include the access token in the Authorization header: `Authorization: Bearer {token}`.
