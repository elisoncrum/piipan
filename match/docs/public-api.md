# Public API

## Summary

The public API is intended as a collection of external-facing endpoints for consumption by state systems. It is managed as an Azure API Management (APIM) instance and deployed by the [IaC](../../docs/iac.md).

Currently the API includes a single endpoint which maps to the [orchestrator](orchestrator-match.md) API's query endpoint.

For a general overview of APIM, refer to [Microsoft's documentation](https://docs.microsoft.com/en-us/azure/api-management/). Piipan makes use of the following concepts:

- [APIs](https://docs.microsoft.com/en-us/azure/api-management/api-management-key-concepts#-apis-and-operations)
- [Operations](https://docs.microsoft.com/en-us/azure/api-management/api-management-key-concepts#-apis-and-operations)
- [Policies](https://docs.microsoft.com/en-us/azure/api-management/api-management-howto-policies)
- [Versions](https://docs.microsoft.com/en-us/azure/api-management/api-management-versions)
- [Subscriptions](https://docs.microsoft.com/en-us/azure/api-management/api-management-subscriptions)

## Calling the API

To call an endpoint:

1. [Procure an API key](#managing-api-keys)
1. Send a request, passing the API key in the `Ocp-Apim-Subscription-Key` header

## Managing endpoints

In APIM, endpoints take the form of operations. Operations are collected together within a parent resource called an API ([details](https://docs.microsoft.com/en-us/azure/api-management/api-management-key-concepts#-apis-and-operations)). Operations and their associated resources are managed in the [apim.json ARM template](../../iac/arm-templates/apim.json). New operations can be added to the Public API by including additional `Microsoft.ApiManagement/service/apis/operations` resources in the template.

An operation's endpoint is constructed using the following components, all provided via the ARM template:

| Component | Example values | Example URL |
|---|---|---|
| The APIM instance's base gateway URL | `https://<apim-name>.azure-api.net` | `https://<apim-name>.azure-api.net` |
| The API's optional `path` property | `path` | `https://<apim-name>.azure-api.net/path` |
| The API's optional version identifier | `v1` | `https://<apim-name>.azure-api.net/path/v1` |
| The operation's `urlTemplate` property | `/query` <br> `/lookup_ids/{id}` | `https://<apim-name>.azure-api.net/path/v1/query` <br> `https://<apim-name>.azure-api.net/path/v1/lookup_ids/1` |

Operations are frontend layers for backend services. A client's request is received by an operation and forwarded to a backend server for processing. The operation receives the resulting response from the backend server and completes the loop by forwarding it to the client.

The connection between an operation and a backend server is made by specifying the backend's base URL (e.g., `https://<function-app-name>.azurewebsites.net/api/v1`) as the `serviceUrl` property of the operation's parent API resource. 

## Managing API keys

API keys are managed as [subscriptions](https://docs.microsoft.com/en-us/azure/api-management/api-management-subscriptions) in APIM. Each subscription is granted a primary and secondary API key to support credential rotation without downtime. Subscriptions can be scoped to a single API, a group of APIs (called a [product](https://docs.microsoft.com/en-us/azure/api-management/api-management-key-concepts#--products)), or all APIs.

Piipan has not yet established a detailed policy for issuing and managing keys. For now, keys are created and managed ad-hoc by system developers via the Portal (Portal > {APIM instance} > Subscriptions).
