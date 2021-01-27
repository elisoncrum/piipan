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

## Binding to state APIs

The orchestrator treats per-state APIs as backing services. When running the [IaC](../../docs/iac.md), per-state endpoints are compiled into a JSON list and saved as an environment variable for the orchestrator Function App.

Currently there is no authorization/authentication performed between the orchestrator and per-state APIs. This functionality is forthcoming.

## Local development

A true local development approach with locally run instances of the per-state APIs and participant records database does not yet exist.

During the development phase, a hybrid approach of running the orchestrator app locally and connecting to the remote (production) per-state APIs can be achieved with the following steps:
1. Connect to a trusted network. Currently, only the GSA network block is trusted.
1. If this is the first time running the app locally, fetch settings (including per-state API endpoints) for the orchestrator app from Azure with `func azure functionapp fetch-app-settings {app-name}`.
1. Run the app using `func start` or, if hot reloading is desired, `dotnet watch msbuild /t:RunFunctions`.
1. Submit `POST` requests against the local URL specified in your terminal.

### App deployment

Deploy the app using the Functions Core Tools, making sure to pass the `--dotnet` flag:

```
func azure functionapp publish <app_name> --dotnet
```

`<app_name>` is the name of the Azure Function App resource created by the IaC process.

## Remote testing

With the per-state APIs restricted to a trusted network, remote testing is not yet possible until the method for authorizing the orchestrator API with the per-state APIs has been implemented.
