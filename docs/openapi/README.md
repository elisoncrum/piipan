# State-facing API Specification

## Prerequisites
- [Swagger/OpenAPI CLI](https://github.com/APIDevTools/swagger-cli)
- [Swagger Markdown tool](https://www.npmjs.com/package/swagger-markdown)

This is the OpenAPI specification for our state-facing API. This spec mirrors what's found in our Azure API Management (APIM) instance as the "Duplication Participation" API.

The goal of this spec is to generate documentation that states will use to integrate with our system.

## Directory Structure

Within the top-level `openapi` directory, there's an OpenAPI spec representing the Duplicate Participation API. Since this API is essentially a compilation of our various subsystem API's, this spec is composed of paths from various subsystem OpenAPI specs. The subsystem specs are managed in their own subdirectories as the single source of truth, and merely referenced here.

The top-level directory also has a tools directory for automatic documentation generation.

Generating documentation will alter the contents of the `generated` directory.

## Generating Documentation

from piipan project root, `cd docs/openapi` then run:
```
./tools/generate-docs.bash
```

This creates a markdown file within the `generated` directory that's used for external documentation.
