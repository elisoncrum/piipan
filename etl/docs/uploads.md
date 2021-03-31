## Uploading Participant Data

> ⚠️ The service API for the bulk upload of PII data is not stable enough for client development. This documentation describes a _temporary_ approach that should allow states to quickly provide test data to our system with out expending development effort unnecessarily.

Once you have [validated the format](./bulk-import.md) of your participant data CSV, you can upload the file to the system through AzCopy.
### AzCopy

AzCopy is a Microsoft-supported command line utility that you can use to upload files to Azure blob storage resources.

[Download AzCopy for your environment](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-v10#run-azcopy)

Upon receiving credentials, you'll first authorize AzCopy through a [service principal approach](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-authorize-azure-active-directory?toc=/azure/storage/blobs/toc.json#authorize-a-service-principal).

Example (Unix/Linux):

```
$ export AZCOPY_SPA_CLIENT_SECRET="$(Read-Host -prompt "Enter key")"
$ azcopy login --service-principal  --application-id application-id --tenant-id=tenant-id
```

Command will return a success or failure message.

For more more details on authorizing AzCopy, visit the [Microsoft Documentation](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-authorize-azure-active-directory?toc=/azure/storage/blobs/toc.json#authorize-a-service-principal-by-using-a-client-secret).

After authorization succeeds, you can upload a file:

```
$ azcopy copy 'name-of-file.csv' 'https://[storage-account-name].blob.core.windows.net/upload'
```

For more details on uploading files through AzCopy, visit the [Microsoft documentation](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-blobs-upload).

## Access

Contact a project representative to gain the necessary credentials for uploading through AzCopy.
