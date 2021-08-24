# Releases

## Versioning nomenclature

Versions of this product have used zero-based versioning (e.g., `0.1`, `0.2`, etc) to as a subtle indicator that we do not yet have a release in production.

We currently **do not** adhere to [Semantic Versioning](https://semver.org/spec/v2.0.0.html) for the system version number.

The convention for each release tag is `v<version-number>`; e.g., `v0.1`, `v0.2`.

## Tagging the release

Note: before tagging the release, it is easiest to update the CHANGELOG first.

Tagging process:

```
git checkout develop
git checkout -b release/<version-number>    # e.g., release/0.5
git tag v<version-number>                   # e.g., v0.5
git push origin v<version-number>
git push origin release/<version-number>
```

Once tagged, go to GitHub:

- Click `New pull request`
- Change base to `main`, set compare to `release/<version-number>`
- Create pull request
- Review commits you may not have had an opportunity to review during the sprint.
- Wait for tests to finish running
- Merge, delete branch

## Creating the release

- Go to Releases tab from main page.
- Create new release, type in the tag name; e.g., `v0.5`. It will appear in drop down. (i.e., you won't be creating it in this UI, you wont specify any particular branch)
- Type in release notes (see other releases for template). You can get a template by going to previous release and click Edit, then copy the text there. Keep the highlights high-level, consolidating similar tickets into a basic description. While the audience for the CHANGELOG is primarily the development team (and integration teams), the release notes is tailored for other stakeholders.
- A draft release (prior to tagging) may already be present to capture deployment notes
