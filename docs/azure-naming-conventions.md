# Our Naming Conventions for Azure

Our naming convention is similar to [Azure's guidelines](https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-naming#example-names-storage) with a few tweaks to fit our system's needs:

```
[prefix]-[resource_type]-[app_name]-[environment]
```

Example for an Azure Function App in production:

```
fns-func-metricsapi-prod
```

## Name Properties

| name | description | required? | dev value | prod value |
| ---- | ----------- | --------- | --- | ---- |
| prefix | denotes agency (required for agency environment) | no for dev; yes for prod | none | "fns" |
| resource_type | abbreviation of Azure resource type | yes | [see list of Azure examples](https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-naming#example-names-general) | same as dev name |
| app_name | unique name that denotes subsystem; should be short but descriptive | yes | one of: ["match", "orch", "query", "metrics"] | same as dev |
| environment | denotes environment in which resource is deployed| yes | "dev" | "prod" |

## Example names for metrics resources:

| resource | dev name | prod name |
| ------- | ------------ | ---------- |
| Resource Group | rg-metrics-dev | [to be named by partner] |
| Key Vault | kvmetricsdev | fnskvmetricsprod |
| Database | db-metrics-dev | fns-db-metrics-prod |
| API function app | func-metricsapi-dev | fns-func-metricsapi-prod |
| Collection function app | func-metricscollect-dev | fns-func-metricscollect-prod |
| Application Insights for API app | ins-metricsapi-dev | fns-ins-metricsapi-prod |
| Application Insights for Collection app | ins-metricscollect-dev | fns-ins-metricscollect-prod |
| Storage for API app | stmetricsapidev | fnsstmetricsapiprod |
| Storage for Collection app | stmetricscoldev | fnsstmetricscolprod |

### Hyphenation and Name Truncation

A few Azure resource groups disallow hyphens, namely Storage Accounts. In these cases, hyphens are removed but the naming convention otherwise stays the same.

If in the event a name is too long, try to shorten the app_name until it fits within the required character length and document it.

For more information, see [naming rules for Azure resources](https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules).

## Notes
- Top-level resource groups are named by partner agency and provided to us.
- Although Azure docs recommend appending an Azure location to the end of names (eg: -westus), we found this made names too long in Azure Government environments.
