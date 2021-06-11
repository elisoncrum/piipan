# Engineering Team Practices

This is a place to jot down our decisions around engineering team workflow. It's a living document subject to change as the codebase and team grows.

### Contents
- [Git Workflow](#git-workflow)
- [Pull Requests and Code Reviews](#pull-requests-and-code-reviews)

## Git Workflow

We'd like to use our git history as a way to easily view logical chunks of work. To that end, we support and encourage squashing commits and rebasing as necessary in a local dev environment.

Rebasing should be done off of the dev branch.

Avoid adjusting git history once a PR is submitted for review.

## Pull Requests and Code Reviews

We strive to keep pull requests as small as possible, but realize this can be hard with greenfield projects. Small pull requests are easier to review and lead to more frequent merges.

When reviewing code, be explicit about whether a comment is blocking or non-blocking, an issue or a suggestion, etc, in the vein of [conventional comments syntax](https://conventionalcomments.org/).

