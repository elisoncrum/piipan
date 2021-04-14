# Quick-Start Guide for States

> ⚠️  Under construction

For a high-level view of system architecture, go [here](../README.md).

## System Status

All three API's are in Alpha stage and under active development. We plan on having API's ready for state testing in a sandbox environment by end of April 2021.

## API Overview

In order to participate, states will need to:

1. Upload participant data to the system
1. Conduct matches against the system
1. Take appropriate action on those matches

These three elements translate to three main areas of the API that states will integrate into their existing eligibility systems and workflows:

1. States will upload participant data through a scheduled [CSV upload](../etl/README.md)
2. Eligibility workers will conduct matches through [Active Matching](../match/README.md)
3. Eligibility workers will be able to take action by referencing previous matches through [a Lookup ID](../match/docs/openapi/orchestrator/index.yaml)

### Environments

Seperate endpoints and credentials will be provided for each environment.

| environment | purpose |
|---|---|
| sandbox | for initial testing of the integration; fake data only |
| pre-production | for testing with actual data at scale; data is not used in production |
| production | actual data used in the production system |

### Endpoints Overview

| Endpoint | Description | Type |
|---|---|---|---|---|---|---|
| `/BulkUpload` | uploads bulk participant data to the system | POST |
| [coming soon] | query for status on data processing from a bulk upload | POST |
| `/query` | query for active matches | POST |
| `lookup_ids/:id` | Returns PII for a Lookup ID | GET |

Detailed documentation for each endpoint can be found [here](./openapi/generated/duplicate-participation-api/openapi.md).

## Authentication

Users will obtain two API subscription keys that will be rotated on a rolling basis. Keys will be placed into request headers.

Example using cURL:

```
curl --request PUT '<uri>' --header 'Ocp-Apim-Subscription-Key: <api-key>'
```

Different endpoints may require different sets of keys. Contact us for access to these keys.

## Feedback

We track API issues through [Github Issues](https://github.com/18F/piipan/issues).

We also have a Slack channel for daily communication (forthcoming).


