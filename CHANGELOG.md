# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/). This project **does not** adhere to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.97] - 2022-01-13

### Changed
- Documentation to reference specific Node.js version requirement
### Added
- ADRs for async/await, subsystem testing strategy, Windows app service plan decision
- Log streaming for blob storage
- Automated script for creating APIM subscriptions
- Various documentation clean up
- Custom widdershins templates for documentation generation
### Fixed
- CSV schema to show `lds_hash` as a required field

## [0.96] - 2021-12-28

### Changed
- Storage accounts to use zone redundancy
- Target region for `tts` deployments to support zone redundancy change
- `timestamp` fields to `timestamptz`
- DB connection strings to use `VerifyFull` SSL mode
- Duplicate Participation API documentation to clarify `Content-Length` header
### Added
- Updates to engineering practices doc
- Rate limiting on Query Tool search form
- ADR for per-state databases decision
- Logging for Event Hub
### Fixed
- `DbConnection` handling to allow for connection pooling
- Orchestrator IaC configuration to properly sequence adding network protections

## [0.95] - 2021-12-14

### Added
- Add APIM rate limiting policy Duplicate Participation API
- Stream diagnostic settings for key vault and event grid topics to event hub
### Fixed
- Add Microsoft.Storage service endpoint to function apps subnet to prevent deployment failures
- Fix resource deployment sequencing bug causing deployment failures

## [0.94] - 2021-11-29

### Changed
- Update approach to App ID URI usage for App Service Authentication based on recent Microsoft changes
- Denied default access to Orchestrator function storage account and storage containers in IAC script
- Use cloud-specific AAD endpoint when configuring App Service Auth

## [0.93] - 2021-11-16

### Added
- Network egress protections for Function apps and App Service apps
### Changed
- Updated PPRL guidance to include explicit list of suffixes for removal
- Refactored Dashboard subsystem to align with the [standard subsystem architecture](https://github.com/18F/piipan/blob/dev/docs/adr/0018-standardize-subsystem-software-architecture.md)
- Configuration options for containerized IaC environment
- Modified web app dependencies to drop requirement for PhantomJS
### Fixed
- Dashboard configuration to use correct metrics API URI
- Dashboard to display error message when API calls fail

## [0.92] - 2021-11-02

### Added
- `match_id` field in Duplicate Participation match responses
- Explicit session timeout for dashboard and query tool apps
- Front-end dependencies build process for query tool and dashboard apps
### Changed
- Refactored `query-tool` subsystem to align with the [standard subsystem architecture](https://github.com/18F/piipan/blob/dev/docs/adr/0018-standardize-subsystem-software-architecture.md)
- Made `uswds` the only production dependency for dashboard and query tool apps
### Fixed
- `test-apim-match-api.bash` to use a secure hash from `example.csv`

## [0.91] - 2021-10-14

### Added
- Match record persistence implementation
### Changed
- Enabled geo-redundancy for `core` and `participants` PostgreSQL databases
- Updated Query Tool to only accept printable characters as input
### Fixed
- Match API participant serialization
- IaC scripts to use updated path for Orchestrator app

## [0.9] - 2021-10-06

### Added
- Foundational components (e.g., database, class structures) for match events
### Changed
- `match` and `etl` subsystems were refactored to align with the [standard subsystem architecture](https://github.com/18F/piipan/blob/dev/docs/adr/0018-standardize-subsystem-software-architecture.md).
- Enhanced normalization library and applied to inputs in the query tool
- Minor enhancements to `create-apim.bash`
### Removed
- References to the previously planned plain text PII matching endpoint

## [0.8] - 2021-09-21

### Added
- `InitiatingState` header to internal request from APIM to Orchestrator API
- Participants library/subsystem to generalize code across from ETL and Match subsystems
- Normalization code to generate the secure hash digest (de-identified PII)
### Changed
- Metrics subsystem was refactored to reflect [ADR on internal software architecture](https://github.com/18F/piipan/blob/dev/docs/adr/0018-standardize-subsystem-software-architecture.md)
### Fixed
- Query tool match functionality using the new normalization code and PPRL API
- `authorize-cli.bash` and `test-metricsapi.bash` to work in `AzureCloud` and `AzureUSGovernment`

## [0.7] - 2021-09-08

### Added
- New Privacy-Preserving Record Linkage (PPRL) documentation
- Custom authorization error display and sign-out pages for web apps
### Changed
- Numerous style/layout changes for the dashboard
- Duplicate participation API for PPRL approach
  - base URL is now `/match/v2`
  - `query` renamed to `find_matches` which takes de-identified PII
  - `participant_id` and `case_id` is now required in match responses
- Bulk upload API for PPRL approach
  - base URL is now `/bulk/v2`
  - `first`, `middle`, `last`, `dob`, and `ssn` columns in CSV replaced with `lds_hash`
  - `participant_id` and `case_id` is now required in CSV
### Removed
- `state_abbr` property in duplicate participation API
- Internal per-state Function Apps for duplicate participation API
### Fixed
- Log categories used by App Service resources
### Broke
- Query tool match functionality (temporarily have no support for plain text match queries)

## [0.6] - 2021-08-23

### Added
- OIDC claim-based policy enforcement to query tool and dashboard
### Changed
- Numerous style/layout changes for the query tool
- Azure B2C IDP docs to include notes on updating user claims
### Removed
- `exceptions` field from bulk upload format and APIs
### Fixed
- Front Door and Easy Auth now work together in the query tool and dashboard

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
- query tool to remove lookup API feature and accommodate query API changes
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

[0.92]: https://github.com/18F/piipan/releases/tag/v0.92
[0.91]: https://github.com/18F/piipan/releases/tag/v0.91
[0.9]: https://github.com/18F/piipan/releases/tag/v0.9
[0.8]: https://github.com/18F/piipan/releases/tag/v0.8
[0.7]: https://github.com/18F/piipan/releases/tag/v0.7
[0.6]: https://github.com/18F/piipan/releases/tag/v0.6
[0.5]: https://github.com/18F/piipan/releases/tag/v0.5
[0.4]: https://github.com/18F/piipan/releases/tag/v0.4
[0.3]: https://github.com/18F/piipan/releases/tag/v0.3
[0.2]: https://github.com/18F/piipan/releases/tag/v0.2
[0.1]: https://github.com/18F/piipan/releases/tag/v0.1
