# Our Naming Conventions for Azure

Our naming convention is similar to [Azure's guidelines](https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-naming#example-names-storage) with a few tweaks to fit our system's needs:

```
[prefix]-[resource_type]-[app_name]-[environment]-[azure_location]
```

Example for an Azure Function App in production, deployed to the Azure location "westus":

```
fns-func-metricsapi-prod-westus
```

## Name Properties

| name | description | required? | dev value | prod value |
| ---- | ----------- | --------- | --- | ---- |
| prefix | denotes agency (required for agency environment) | no for dev; yes for prod | none | "fns" |
| resource_type | abbreviation of Azure resource type | yes | [see list of Azure examples](https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-naming#example-names-general) | same as dev name |
| app_name | unique name that denotes subsystem; should be short but descriptive | yes | one of: ["match", "orch", "query", "metrics"] | same as dev |
| environment | denotes environment in which resource is deployed| yes | "dev" | "prod" |
| azure_location | denotes Azure location where resource is deployed; typically the same for all resources within a resource group | yes | TBD (example: "westus") | TBD |

## Example names for metrics resources:

| resource | dev name | prod name |
| ------- | ------------ | ---------- |
| Resource Group | rg-metrics-dev-westus | [to be named by partner] |
| Key Vault | kvmetricsdevwestus | fnskvmetricsprod[tbd]* |
| Database | db-metrics-dev-westus | fns-db-metrics-prod-[tbd] |
| API function app | func-metricsapi-dev-westus | fns-func-metricsapi-prod-[tbd] |
| Collection function app | func-metricscollect-dev-westus | fns-func-metricscollect-prod-[tbd] |
| Application Insights for API app | ins-metricsapi-dev-westus | fns-ins-metricsapi-prod-[tbd] |
| Application Insights for Collection app | ins-metricscollect-dev-westus | fns-ins-metricscollect-prod-[tbd] |
| Storage for API app | stmetricsapidevwestus | fnsstmetricsapiprod[tbd] |
| Storage for Collection app | stmetricscoldevwestus | fnsstmetricscolprod[tbd] |

_* A vault's name must be between 3-24 alphanumeric characters, even though Azure docs say hyphens are allowed_

### Hyphenation and Name Truncation

A few Azure resource groups disallow hyphens, namely Storage Accounts and Key Vaults. In these cases, hyphens are removed but the naming convention otherwise stays the same.

If in the event a name is too long, try to shorten the app_name until it fits within the required character length and document it.

For more information, see [naming rules for Azure resources](https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules), although note that these rules may conflict with the rules in actual error messages when deploying resources.

## Notes
- Top-level resource groups are named by partner agency and provided to us.
