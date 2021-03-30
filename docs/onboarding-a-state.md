# Onboarding a state
> ⚠️ This documentation is for Piipan developers only

1. Create a service principle for state storage account

## Create a service principle for state storage account

In order for a user to upload a csv, a service principle must be created for the specified storage account. This script will generate a service principle and output the credentials to be provided to the user.

From the top-level piipan directory:

```
$ cd iac
$ ./create-state-storage-service-principle.bash [env] [storage account name]
```

Usage example:
```
$ ./create-state-storage-service-principle.bash tts/dev my-storage-name
```

Note that running the script repeatedly will re-generate the credentials.

Once the credentials are generated, securely share them with the user.
