# Bulk import format for participant records

CSV files are used as a data exchange format for the bulk import of program participant records. A description of the required CSV fields can be found in [import-schema.json](csv/import-schema.json), using the [Table Schema specification](https://specs.frictionlessdata.io/table-schema).

UTF-8 encoding is required for all fields.

## Validating files

There are Table Schema libraries available for [multiple programming languages](https://frictionlessdata.io/tooling/table-schema-tools/). The following example will use macOS, homebrew, Python, and [tableschema-py](https://github.com/frictionlessdata/tableschema-py):

```
brew install python
pip3 install tableschema
cd piipan/etl/docs/csv
python3 validate.py example.csv
```
Any errors present in the supplied CSV file will be printed to `stdout`.

## Uploading files

There are a variety of ways to upload files to your provided Azure blob storage resource. We recommend using any of the Azure-supported tools in the programming language of your choice, which are listed in full [here](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction#about-blob-storage). Popular tools are:

- [.NET](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/storage)
- [Java](https://docs.microsoft.com/en-us/java/api/overview/azure/storage?view=azure-java-stable)
- [Node.js](https://github.com/Azure/azure-sdk-for-js/tree/master/sdk/storage)
- [Python](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-python)

You will need an Azure Access Key and a Connection string in order to use these tools. Contact a project representative to gain these credentials.

You can also upload files in a Put Blob request through Azure Blob Storage's [REST API](https://docs.microsoft.com/en-us/rest/api/storageservices/put-blob) which would require simililar key-based credentials.

For more information on managing Azure Blob Storage, see [Microsoft's documentation](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction).

## Notes
- Microsoft Excel is notorious for mangling CSV files. By default, it will reformat date fields, remove leading zeros in integer-like strings, etc. Round-tripping CSV files using Excel is only possible through careful import and export:
  1. Open a new workbook
  1. Use File → Import, not File → Open 
  1. When specifying column format, highlight all the columns in the preview and select Text
  1. Use File → Save As and specify the `CSV UTF-8 (Comma delimited) (.csv)` file format
- Microsoft Excel uses [`CR/LF` as a line ending (not just `LF`)](https://en.wikipedia.org/wiki/Newline#Representation), when exporting sheets as CSV. `CR/LF` is also specified in [RFC 4180](https://tools.ietf.org/html/rfc4180). This can confuse Unix-oriented text processing tools.
- Microsoft Excel will put a UTF-8 [Byte Order Mark](https://en.wikipedia.org/wiki/Byte_order_mark) (BOM) at the very start of the CSV file. UTF-8 has no byte ordering, but Excel uses this to automatically detect the encoding used in a CSV file. BOMs will frequently confuse Unix-oriented text processing tools.  
