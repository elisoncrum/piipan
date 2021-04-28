# Deployment environment for resource identifiers
ENV=$(basename "${BASH_SOURCE%.*}")

# Default location
LOCATION=westus

# Default resource group
RESOURCE_GROUP=rg-core-$ENV

# Resource group for matching API
MATCH_RESOURCE_GROUP=rg-match-$ENV

# Resource group for metrics
METRICS_RESOURCE_GROUP=rg-metrics-$ENV

# Prefix for resource identifiers
PREFIX=tts

# Either AzureCloud or AzureUSGovernment
CLOUD_NAME=AzureCloud

# used to create API Management resources
APIM_EMAIL=noreply@tts.test
