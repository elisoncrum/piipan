## Uploading Participant Data

> ⚠️ The service API for the bulk upload of PII data is not stable enough for client development. This documentation describes a _temporary_ approach that should allow states to quickly provide test data to our system with out expending development effort unnecessarily.

Once you have [validated the format](./bulk-import.md) of your participant data CSV, you can upload the file to the system through AzCopy.
### AzCopy

AzCopy is a Microsoft-supported command line utility that you can use to upload files to Azure blob storage resources.

[Download AzCopy for your environment](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-v10#run-azcopy)

#### Authorization

Contact a project representative to gain the necessary credentials for uploading through AzCopy. What you will need:

- application id
- tenant id
- password (this is used as the client secret)
- storage account url

All of these items will vary between test and production environments.

Upon receiving these credentials, you'll first authorize AzCopy through a [service principal approach](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-authorize-azure-active-directory?toc=/azure/storage/blobs/toc.json#authorize-a-service-principal).

First save the password:

PowerShell: `$env:AZCOPY_SPA_CLIENT_SECRET="$(Read-Host -prompt "Enter key")"`

Bash: `export AZCOPY_SPA_CLIENT_SECRET=<password>`

Then login using the application id and tenant id:

```
azcopy login --service-principal  --application-id application-id --tenant-id=tenant-id
```

Login command will return a success or failure message. For more more details on authorizing AzCopy, visit the [Microsoft Documentation](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-authorize-azure-active-directory?toc=/azure/storage/blobs/toc.json#authorize-a-service-principal-by-using-a-client-secret).

#### Uploading a File

After authorization succeeds, you can upload your file using the storage account url:

Example:
```
$ azcopy copy 'name-of-file.csv' 'https://my-storage-account-name.blob.core.windows.net/upload'
```

For more details on uploading files through AzCopy, visit the [Microsoft documentation](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-blobs-upload).
