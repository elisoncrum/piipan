# Lookup ID API

## Prerequisites
- [Azure Command Line Interface (CLI)](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [.NET Core SDK 3.1](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

## Summary

An API for querying the PII associated with an opaque lookup ID. The lookup ID is provided in responses from the orchestrator matching API when a match has been detected.

The lookup ID API is implemented in the `Piipan.Match.Orchestrator` project and deployed to an Azure Function App.

## Deployment and testing

As the lookup ID and orchestrator match APIs share a codebase, see [orchestrator-match.md](orchestrator-match.md) for deployment and testing details.
