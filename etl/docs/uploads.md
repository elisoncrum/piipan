## Uploading Participant Data

Once you have [validated the format](./bulk-import.md) of your participant data CSV, there are various ways to upload CSV files to your provided Azure blob storage resource, allowing you to choose the right approach for your workflow.

### Command-Line Utilities

#### AzCopy

AzCopy is a Microsoft-supported command line utility that you can use to upload files to Azure blob storage resources.

[Download AzCopy for your environment](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-v10#run-azcopy)

Example command to upload a csv file to a storage account:

```
azcopy copy '<yourfile.csv>' '<provided-storage-url>'
```

For more details on using AzCopy, visit the [Microsoft documentation](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-blobs-upload).

### Programmatically

Use any of the Azure-supported tools in the programming language of your choice, which are listed in full [here](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction#about-blob-storage). Popular tools are:

- [.NET](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/storage)
- [Java](https://docs.microsoft.com/en-us/java/api/overview/azure/storage?view=azure-java-stable)
- [Node.js](https://github.com/Azure/azure-sdk-for-js/tree/master/sdk/storage)
- [Python](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-python)

For more information on uploading to Azure Blob Storage programmatically, see [Microsoft's documentation](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction).

### Web-based API

You can also upload files in a Put Blob request through Azure Blob Storage's [REST API](https://docs.microsoft.com/en-us/rest/api/storageservices/put-blob).

## Access

For all of these uploads methods, you'll need an Azure Access Key and a Connection string to the storage resource. Contact a project representative to gain these credentials, specifying your intended upload method.
