# Status
The NAC is currently in prerelease status. Much of the core functionality is demonstrably working. The ATO process is underway but not complete. Four states have begun work on integration.

# Product Roadmap
The roadmap represents our latest thinking about:
- the order in which the NAC's features will be developed
- which features are needed prior to initial use by state agencies
- problems, opportunities, and refinements that are being deferred as enhancements after the NAC's first use.

## ATO
**Goal: Deliver working, secure software using FNS toolchains to the production environment that does not centrally collect PII.**

_Target: December 2021_

Acceptance criteria
- ATO has been achieved 
- The production environment has been deployed through a FNS-hosted CI/CD toolchain 
- Uploads and query-initiated matches work for curated test data in production, using updated deidentification designs

A detailed list of the issues involved in achieving this goal can be found in the [1st release: Initial Production Deployment](https://github.com/18F/piipan/milestone/21) milestone

## Pre-MVP Test Launch
**Goal: FNS can confirm that uploads and matching will work by allowing States to send production data (without triggering match actions).  FNS begins monitoring key performance indicators.**

Acceptance criteria
- The NAC supports the volume of bulk uploads and queries states will perform 
- States can test their normalization and validation 
- States can monitor the progress of their uploads 
- At least 2 states are uploading data to the NAC daily and sending queries to the NAC with each relevant case action 
- FNS can monitor and confirm key performance indicators
  - States are uploading data each day 
  - States are performing queries 
  - The NAC is online as often as needed 
  - The NAC responds to queries in a reasonable time frame 
  - Some queries result in matches

## MVP Launch: NAC in use
**Goal: Meet all needs to operationalize the NAC.**

Acceptance criteria
- Allows 3 states to begin using NAC for every required case action
- Email notifications
- Flag bad matches
- Monitor dispositions and measure accuracy
- _Other features with scope still under development_
