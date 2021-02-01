# Orchestrator matching API

## Prerequisites
- [Azure Command Line Interface (CLI)](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [.NET Core SDK 3.1](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

## Summary

An initial API for matching PII data across all participating states.
1. JSON `POST` request that conforms to the [OpenApi spec](openapi.md) is sent to the orchestrator API endpoint.
1. The `POST` event triggers a function named `Query` in the orchestrator Function App.
    - If the request is not valid (malformed, missing required data, etc), the function returns a 400 response. Currently no error messaging is included in the response.
    - If the request is valid, the function queries the per-state APIs for matches. A 200 response is returned containing any matching records.
    - If there is an issue connecting to or querying any of the per-state APIs, the orchestrator returns a 500 response.

## Environment variables

The following environment variables are required by `Query` and are set by the [IaC](../../docs/iac.md):

| Name | |
|---|---|
| `StateApiHostStrings` | [details](../../docs/iac.md#\:\~\:text=StateApiHostStrings) |
| `StateApiEndpointPath` | [details](../../docs/iac.md#\:\~\:text=StateApiEndpointPath) |

## Binding to state APIs

The orchestrator treats per-state APIs as backing services. When running the [IaC](../../docs/iac.md):
- Per-state base URIs are compiled into a JSON list and saved as an environment variable
- The relative endpoint for the state API query method is saved as an environment variable
- The orchestrator's system-assigned identity is given an authorized application role (which will be checked by the state API upon receiving requests)

At runtime, the app uses the base URI to request an authentication token from the state app's Active Directory app registration. This token, which includes the authorized application role, is included as an authorization header (`Authorization: Bearer {token}`) in the request sent to the state API.

## Local development

A true local development approach with locally run instances of the per-state APIs and participant records database does not yet exist.

Until then, local development is limited by the need to authenticate with Active Directory before accessing state APIs. The Instance Metadata Service used to retrieve authentication tokens is not available locally. There are [potential solutions](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/service-to-service-authentication#local-development-authentication) using the `Microsoft.Azure.Services.AppAuthentication` library. None have been implemented at this time.


### App deployment

Deploy the app using the Functions Core Tools, making sure to pass the `--dotnet` flag:

```
func azure functionapp publish <app_name> --dotnet
```

`<app_name>` is the name of the Azure Function App resource created by the IaC process.

## Remote testing

To test the orchestrator remotely:
1. Connect to a trusted network. Currently, only the GSA network block is trusted.
1. Submit valid POST requests using a tool like Postman.
