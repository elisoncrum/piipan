# Quickstart Integration Guide for States

This guide is to help development teams working at state and territory governments integrate their SNAP eligibility systems with the NAC. 

## Overview

In order to onboard to the NAC, states will need to:

1. Upload de-identified participant data to the NAC on daily basis
1. Conduct searches against the NAC during (re)certification and resolve matches

These actions correspond to 2 web service APIs:

1. [Bulk Upload API](./openapi/generated/bulk-api/openapi.md)
1. [Duplicate Participation API](./openapi/generated/duplicate-participation-api/openapi.md)

Each API has one or more RPC or REST operations and uses JSON in the operation request and/or response bodies. All operations must be made over HTTPS and authenticated by an API key. Each state will be issued a key for the Bulk Upload API and a separate key for the Duplicate Participation API.

For details, please see our specifications:
- [Bulk Upload API](./openapi/generated/bulk-api/openapi.md)
- [Bulk Upload CSV format](../etl/docs/bulk-import.md)
- [Personal Identifiable Information (PII) de-identification](./pprl.md)
- [Duplicate Participation API](./openapi/generated/duplicate-participation-api/openapi.md)

## Notional, high-level integration steps
1. Understand the NAC material in the [system overview](https://github.com/18F/piipan#overview) and our [introduction to our Privacy-Preserving Record Linkage approach](https://github.com/18F/piipan/blob/dev/docs/pprl-plain.md).
1. Export participant data in plain text from your eligibility system to CSV.
1. Transform the plain text CSV to the Bulk Upload CSV format, in accordance with the PII de-identification specification.
1. Integrate with the Bulk Upload API to submit the CSV to the NAC using the `/upload` operation.
1. Integrate with the Duplicate Participation API to conduct searches, de-identifying applicant PII before making the `/find_matches` call.

## Environments

Several isolated NAC systems are being built for states to use:

| Environment    | Purpose                                 | Status    | API hostname                           |
|----------------|-----------------------------------------|-----------|----------------------------------------|
| Testing        | For initial testing with synthetic data | Available | tts-apim-duppartapi-test.azure-api.net |
| Pre-production | For testing with real data at scale     | Pending   | -                                      |
| Production     | For production use with real data       | Pending   | -                                      |

## Usage notes

### De-identification testing
- Correct de-identification in accordance with our defined process is critical for cross-state matching. We strongly recommend unit testing your de-identification code, [covering the specific normalization and validation scenarios we describe](./pprl.md). The NAC team is exploring strategies to verify state-performed de-identification in an automated, ongoing fashion.

### Record retention
- Save API responses received from the duplicate participation API for 3 years.
- API responses that are used for SNAP eligibility determinations are subject to the requirements of 7 CFR 272.1(f).

### Failed requests
- Bulk uploads can be resubmitted as required; the most recent upload will overwrite any pre-existing participant snapshot.

## Sample records

The test environment currently includes three sample states (i.e., `EA`, `EB`, `EC`) that reflect an upload of this [de-identified example CSV](../etl/docs/csv/example.csv). 

The [plain text CSV](../etl/docs/csv/plaintext-example.csv) that this file is derived from is also published. Searching for any of the individuals in the [plain text CSV](../etl/docs/csv/plaintext-example.csv) should return back a match result.

## Feedback

Need to report a defect? We track API issues through [GitHub](https://github.com/18F/piipan/issues).

Have a question, or want to work through a technical issue? Start a thread in our Microsoft Teams channel or reach out to us by email.
