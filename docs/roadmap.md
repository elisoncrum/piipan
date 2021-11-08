# Status
The NAC is currently in pre-release status. Much of the core functionality is demonstrably working. The ATO process is underway but not complete. Four states have begun work on integration.

Piipan has a working continuous integration pipeline in TTS-managed environments that includes automated unit and accessibility tests.  Work is underway to create a similar CI pipeline in FNS-managed environments.

# Product Roadmap
The roadmap represents our latest thinking about:
- The order in which the NAC's features will be developed
- Which features are needed prior to initial use by state agencies
- Problems, opportunities, and refinements that are being deferred as enhancements after the NAC's initial rollout

Plans and features outlined in this roadmap are subject to change.

# Launch strategy and sequence 
Prior to being used to prevent duplicate participation, there will be two NAC deployments that will be used to reduce launch risks.  These will be followed by a minimum viable product (MVP) deployment.  The MVP is the first release that will be used in production by states to take action on potential cases of duplicate participation. 

The three deployments leading to production NAC usage are: 
- ATO (First production deployment)
- Pre-MVP Test Launch 
- MVP 

The goals and features of each phase are described in more detail below. 

## ATO
**Goal: Deliver working, secure software using FNS toolchains to the production environment that does not centrally collect PII.**

_Target date: January 2022_

**How this will be used in production**
- State agencies will be able to verify systems access and credentials to integrate with the production NAC.  States will only use the production NAC with test data that verifies successful systems access. 
- State agency users who will need access to the NAC website will submit access requests, as needed, and verify their ability to log in to the site  
- FNS users who will have access to administrative features will have their accounts set up and verify the ability to reach the metrics dashboard 

**Risks mitigated**
- Discover and address any deficiencies in the NAC’s implementation of required security controls 
- Ensure there are no unknown challenges to delivering software to the FNS environment 
- Discover and address systems access needs with ample time to resolve access limitations 

**External dependencies** 
- State agencies will need to take these actions: 
  - Test their production API keys 
  - Identify which users will need access to the NAC website 
  - Submit access requests for those users 
  - Verify that users have the access that has been granted 

**Acceptance criteria**
- ATO has been achieved 
- The production environment has been deployed through a FNS-hosted CI/CD toolchain 
- Uploads and query-initiated matches work for curated test data in production, using updated deidentification designs
- Each state agency in Group 1A has verified their ability to interact with the production API 
- Primary users at the state agencies and FNS have been identified, access has been granted, and each user has verified their ability to access the NAC website

A detailed list of the issues involved in achieving this goal can be found in the [1st release: Initial Production Deployment milestone](https://github.com/18F/piipan/milestone/21) 

## Pre-MVP Test Launch
**Goal: FNS can confirm that uploads and matching will work by allowing States to send production data (without triggering match actions).  FNS begins monitoring key performance indicators.  FNS can test usability of the process for match determinations to be made.**

_Target date: TBD_

**How this will be used in production:**
- State agencies will fully integrate the following activities into their benefits processing systems: 
  - Daily uploads of deidentified records of all active SNAP participants 
  - Automated NAC queries to accompany each case action (applications, recertifications, additions of a household member).   
    - All NAC searches will respond with “no match found” 
    - States may disregard search responses at this phase.  States do not need to have user interface updates to their benefits system in place to show the results of NAC searches yet. 
- FNS will use the metrics website and external tools to monitor system health and performance 

In this stage, states will not be provided with any information on matches discovered or be asked to take action to evaluate the validity of any matches.

In addition to the production environment usage, the test environment will be used for State agency users to perform usability tests for match resolution and disposition tracking workflows.

**Risks mitigated**
- Identify any problems with system performance under full volumes of traffic 
- Prove that state API integrations are working.  Surface integration challenges and unforeseen API needs with some time to adapt to any newly discovered scope 
- Learn about usability challenges by providing a prototype that can be used to test system interactions with users

**External dependencies**
- Verification of system performance depends on states completing API-based integrations 

**Acceptance criteria**
- The NAC supports the volume of bulk uploads and queries states will perform
  - States can test their normalization and validation
  - States can monitor the progress of their uploads
- At least 2 states are uploading data to the NAC daily and sending queries to the NAC with each relevant case action
- FNS can monitor and confirm key performance indicators
  - States are uploading data each day
  - States are performing as many searches as they should, given the volume of applications, recertifications, and additions of household members 
  - Some queries result in matches
  - Uptime and typical latency for APIs and websites is acceptable
- States have the ability to report determinations on matches, resolving them when complete
  - States can look up the record for a match that was previously found
  - States can report matches as invalid
  - Each state in a 2-state match can report the determination
  - Matches are closed when both states take action, or when either state reports the match as invalid

A full list of issues flagged for the Pre-MVP test launch is available in the [2nd release: Pre-MVP test launch milestone](https://github.com/18F/piipan/milestone/23) 


## MVP Launch: NAC in use
**Goal: The NAC is fully operational.  States are able to use the NAC to detect potential duplicate participation and take appropriate actions.  The NAC supports all requirements of the (not-yet-published) rule governing the NAC.**

_Target date: TBD_

**How this will be used in production**
- States will continue performing daily uploads of deidentified SNAP participant records and searching the NAC for every required case action 
- The NAC will provide match results to states 
- States will take action on every match per regulatory guidance from FNS 
- FNS will use the metrics website and external tools to monitor system health, performance, and state compliance with regulations 

**Risks mitigated**
- Validate successful operation of a minimal feature set before adding enhancements and improvements that might be based on untested assumptions 

**External dependencies**
- The final NAC rule needs to be published 
- State agencies will update their benefits systems to show the results of NAC searches 

**Acceptance criteria**
- At least 3 states are using the NAC for every required case action
- The NAC sends email notifications to each state involved in a match when a match is found
- FNS can use dispositions and invalid matches reported by states to measure accuracy
- The NAC implements all protections for vulnerable individuals required by the NAC rule
- States are provided with onboarding materials and training to make the launch as smooth as possible 
- When states are looking at a match, they are provided with the contact information for their counterparts at the other state

A full list of issues flagged for the MVP launch is available in the [3rd release: MVP milestone](https://github.com/18F/piipan/milestone/18) 
