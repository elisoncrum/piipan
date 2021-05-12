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
  specs=(bulk-api duplicate-participation-api)

  ./tools/generate-specs.bash

  for s in "${specs[@]}"
  do
    pushd ./generated/"${s}"/
    widdershins \
        --language_tabs 'shell:curl:request' \
        --omitBody \
        --omitHeader \
        --shallowSchemas \
        openapi.yaml \
        -o openapi.md
    popd
  done
}

main
