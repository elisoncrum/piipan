#!/bin/bash
#
# Validates and bundles Open Api spec for duplicate participation API
# into a single openapi markdown file to serve as API documentation.
#
# Requires swagger-cli (https://github.com/APIDevTools/swagger-cli)
# Requires widdershins cli (https://github.com/Mermade/widdershins/tree/master)
#
# usage (from project root):
# cd docs/openapi
# ./tools/generate-docs.bash

set -e
set -u

main () {
    ./tools/generate-specs.bash
    pushd ./generated/duplicate-participation-api/
        widdershins \
            --language_tabs 'shell:curl:request' \
            --omitBody \
            --omitHeader \
            --shallowSchemas \
            openapi.yaml \
            -o openapi.md
    popd
}

main
