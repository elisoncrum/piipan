# Engineering Team Practices

This is a place to jot down our decisions around engineering team workflow. It's a living document subject to change as the codebase and team grows.

### Contents
- [Git Workflow](#git-workflow)
- [Pull Requests and Code Reviews](#pull-requests-and-code-reviews)
- [Definition of Done](#definition-of-done)
- [Dependabot PRs](#dependabot-prs)

## Git Workflow

- We'd like to use our git history as a way to easily view logical chunks of work. To that end, we support and encourage squashing commits and rebasing as necessary in a local dev environment.

- Rebasing should be done off of the dev branch.

- Avoid adjusting git history once a PR is submitted for review.

## Pull Requests and Code Reviews

- We strive to keep pull requests as small as possible, but realize this can be hard with greenfield projects. Small pull requests are easier to review and lead to more frequent merges.

- When reviewing code, be explicit about whether a comment is blocking or non-blocking, an issue or a suggestion, etc, in the vein of [conventional comments syntax](https://conventionalcomments.org/).

- The PR submitter closes the PR after getting 1 approval. This more directly assigns the responsibility of any failed CircleCI builds to the PR submitter, as others may not get the build failure notifications.

- If possible, judiciously select a single reviewer for a PR, based on their area of speciality, or if they had previously worked on the portion of the code base. For API specification work or changes concerning the entire development team, add the entire team as reviewers (but feel free to merge with 1 approval).

## Definition of Done

The usual suspects, plus some variants:
- Commented source code: required on public classes, methods; elsewhere as appropriate
- Supporting developer documentation for new build/test/API changes or for particularly complex portions of the system
- Architectual Decision Records (as appropriate) for research spikes
- Automated unit tests to maintain or increase level of code coverage
- If a PR changes the IaC, the IaC should be manually applied to tts/dev as our CI/CD pipeline does not run IaC automatically
- As warranted, add the `changelog` tag to the PR and leave text to add to CHANGELOG at the end of the sprint – this is particularly important for any external API changes (e.g.; [#1374](https://github.com/18F/piipan/pull/1374))
- For any deployment steps that require manual intervention, add them to the `Deployment notes` section of the draft release notes

Currently we our test coverage is falling short of our 90% target goal. We do not believe we will be able to meet that goal until we incorporate end-to-end browser integration tests for our web applications.

## Dependabot PRs

Dependabot does not support package lock files with .NET and so the automated PRs that it generates will fail in CI.

In addition, since we have avoided requiring Node to build the web applications, as a consequence Dependabot PRs that update Node packages do not correctly rebuild our web apps.

To address these Dependabot PRs, a developer can:
- assign the PRs to themselves (typically in bulk via the Asignee function)
- follow the [steps to update dependencies](./update-deps.md) using a a new PR

The Dependabot PRs should close automatically after the new PR is merged.