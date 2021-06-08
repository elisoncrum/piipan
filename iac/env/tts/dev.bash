# shellcheck disable=SC2034

# Deployment environment for resource identifiers
ENV=$(basename "${BASH_SOURCE%.*}")

# Default location
LOCATION=westus

# Default resource group
RESOURCE_GROUP=rg-core-$ENV

# Resource group for matching API
MATCH_RESOURCE_GROUP=rg-match-$ENV

# Resource group for metrics
METRICS_RESOURCE_GROUP=$RESOURCE_GROUP

# Prefix for resource identifiers
PREFIX=tts

# Either AzureCloud or AzureUSGovernment
CLOUD_NAME=AzureCloud

# used to create API Management resources
APIM_EMAIL=noreply@tts.test

OIDC_ISSUER_URI=https://ttsb2c$ENV.b2clogin.com/ttsb2c$ENV.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_si

DASHBOARD_APP_CLIENT_ID=e7e769ad-e9bc-4c5f-8c3e-ebaf6cf9cacb