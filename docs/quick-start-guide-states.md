# Quick-Start Guide for States

For a high-level view of system architecure, go [here](../README.md).

## System Status

All three API's are in Alpha stage and under active development. We plan on having API's ready for states testing in a sandbox environment by the end of April 2021.

## API Overview

In order to participate, states will need to:

1. Upload participant data to the system
1. Conduct matches against the system
1. Take appropriate action on those matches

These three elements translate to three main areas of the API that states will integrate into their existing eligibility systems and workflows:

1. States will upload participant data through a scheduled [CSV upload](../etl/README.md)
2. Eligibility workers will conduct matches through [Active Matching](../match/README.md)
3. Eligibility workers will be able to take action by referncing PII through [a Lookup ID](../match/docs/openapi/orchestrator/index.yaml)

### Urls

| environment | description | url |
|---|---|---|
| sandbox | for initial testing of the integration; fake data only | [coming soon] |
| pre-production | for testing with actual data at scale; data is not used in production | [coming soon] |
| production | actual data used in the production system | [coming soon] |

### Endpoints Overview

| endpoint | description | Type | Parameters | Response | Authentication |
|---|---|---|---|---|---|
| `/BulkUpload` | uploads bulk participant data to the system | POST | a CSV file [in this format](../etl/docs/bulk-import.md) | [coming soon] | Key-based, contact a developer for access |
| [coming soon] | query for status on data processing from a bulk upload | POST | [coming soon] | [coming soon] | Key-based, contact a developer for access |
| `/query` | query for active matches | POST | refer to the [OpenApi Schema](../match/docs/openapi/orchestrator/index.yaml) | refer to the [OpenApi Schema](../match/docs/openapi/orchestrator/index.yaml) | Key-based, contact a developer for access |
| `lookup_ids/:id` | Returns PII for a Lookup ID | GET | Lookup ID | [coming soon] | Key-based, contact a developer for access |

## Authentication

To use all endpoints, states will need two API keys: one for bulk uploading and another for the rest of the endpoints. Contact us for access to these keys.

## Feedback

We track API issues through [Github Issues](https://github.com/18F/piipan/issues).

We also have a Slack channel for daily communication (forthcoming).


