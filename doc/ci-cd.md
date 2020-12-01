# CI/CD Pipeline

## CircleCI

We use CircleCI to automate the build and deployment of our subsystems.

### Environment variables

CircleCI is configured with several environment variables that provide information and credentials necessary to automate deployment to Azure.

| Environment variable | Value |
|---|---|---|
| `AZURE_RESOURCE_GROUP` | Name of resource group where services will be deployed (e.g., "piipan-resources") |
| `APP_NAME` | Name of [dashboard app](dashboard.md) |
| `AZURE_SP` | Service principal `appId` (aka `clientId`) |
| `AZURE_SP_PASSWORD` | Service principal `password` (aka `clientSecret`)|
| `AZURE_SP_TENANT` | Service principal `appOwnerTenantId` |

The three environment variables beginning with `AZURE_SP` provide CircleCI with credentials for an Azure service principal. The credentials are used to log in to the Azure CLI via a user ID and password that are provided to the [circleci/azure-cli orb](https://circleci.com/developer/orbs/orb/circleci/azure-cli).

A service principal named `piipan-cicd` (note, this is the *display name* and not the *`appId`*) is created as part of the IaC process and intended to be used by CircleCI.

### Retrieving credentials

Service principal credentials are not stored outside of CircleCI and can not be retrieved after they are created. To get credentials, follow the process for refreshing credentials in [`service-principals.md`](service-principals.md):

```
    cd iac
    ./create-service-principal.bash piipan-cicd
```

*Note:* Refreshing credentials will invalidate existing credentials and the `AZURE_SP` and `AZURE_SP_PASSWORD` environment variables will need to be updated.
