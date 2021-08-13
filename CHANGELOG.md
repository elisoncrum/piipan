# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/). This project **does not** adhere to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.5] - 2021-08-10
### Added
- OpenID Connect (OIDC) authentication to dashboard and query tool 
- managed identity to metrics Function Apps and database access
- IaC for streaming logs to an external SIEM via Event Hub
- system account and initiating user to audit logs for API calls
- Defender to all storage accounts in subscription
- CIS benchmark to Policy
- top-level build/test script
### Changed
- duplicate participation API to allow an entire household to be queried for
- App Service instances to use Windows under-the-hood
- query tool to remove lookup API feature and accomodate query API changes
- Front Door to use a designated public file in dashboard and query tool for health check
- duplicate participation Function Apps so they do not hibernate
- Orchestrator Function App so that network egress is through a VNet 
### Removed
- Lookup API call; it's been obsoleted by PPRL model
- `METRICS_RESOURCE_GROUP`; folded resources into `RESOURCE_GROUP`
### Fixed
- `update-packages.bash --highest-major`
- Key Vault-related IaC so as to be compatible in either `AzureCloud` or `AzureUSGovernment`

## [0.4] - 2021-06-15
### Added
- `benefits_end_month`, `protect_location`, and `recent_benefit_months` to query response.
- `protect_location` and `recent_benefit_months` to CSV.
- `case_id`, `participant_id` to query tool.
- logging to indicate identity of Function App callers.
- log streaming to an Event Hub for remaining Azure resources.
- documentation for creating an Azure AD B2C OIDC identity provider.
- OIDC support for dashboard and query tool via Easy Auth.
- updated high-level architecture diagram.
### Changed
- `dob` field in CSV to be ISO 8601 formatted.
- CSV backwards compatibility: columns, not just field values, are optional when fields are not required.
### Deprecated
- MM/DD/YYYY format for `dob` field in CSV. Will continue to be accepted along with ISO 8601 format.
### Fixed
- `build.bash deploy` for dashboard and query tool.

## [0.3] - 2021-06-01
### Added
- `case_id`, `participant_id`, and `benefits_end_month` fields to CSV.
- `case_id`, `participant_id`, and `state` properties to query response.
- initial log streaming to an Event Hub for Azure resources.
### Changed
- the query tool so as to display the state abbreviation as "State".
### Deprecated
- `state_abbr` property in query response. It has been replaced by `state`.
### Removed
- `state_name` property from the query response.

## [0.2] - 2021-05-18
### Added
- CUI banner to query tool.
- Improved tooling for automated builds, tests, and deploys.
- Shellcheck to the Continuous Integration (CI) process.
### Changed
- Date of Birth (DoB) display format in query tool, just show the month/day/year.

## [0.1] - 2021-05-04
### Added
- Initial APIs for use by group 1A state integrators.

[0.5]: https://github.com/18F/piipan/releases/tag/v0.5
[0.4]: https://github.com/18F/piipan/releases/tag/v0.4
[0.3]: https://github.com/18F/piipan/releases/tag/v0.3
[0.2]: https://github.com/18F/piipan/releases/tag/v0.2
[0.1]: https://github.com/18F/piipan/releases/tag/v0.1
