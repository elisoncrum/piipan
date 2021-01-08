# Infrastructure-as-Code

## Prerequisites
- [Azure Command Line Interface (CLI)](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download)
- `bash` shell, `/dev/urandom`, etc. via macOS, Linux, or the Windows Subsystem for Linux (WSL) 
- `psql` client for PostgreSQL

## Steps
To (re)create the Azure resources that `piipan` uses:
1. Connect to a trusted network. Currently, only the GSA network block is trusted.
2. Sign in with the Azure CLI `login` command :
```
    az login
```
3. Run `create-resources`, which deploys Azure Resource Manager (ARM) templates and runs associated scripts:
```
    cd iac
    ./create-resources.bash
```
## Notes
- `iac/states.csv` contains the comma-delimited records of participating states/territories. The first field is the [two-leter postal abbreviation](https://pe.usps.com/text/pub28/28apb.htm); the second field is the name of the state/territory.
- For development, dummy state/territories are used (e.g., the state of `Echo Alpha`, with an abbreviation of `EA`).
- If you forget to connect to a trusted network and `create-resources` fails, connect to the network, then re-run the script.
- Some Azure CLI provisioning commands will return before all of their behind-the-scenes operations complete in the Azure environment. Very occasionally, subsequent provisioning commands in `create-resources` will fail as it won't be able to locate services it expects to be present; e.g., `Can't find app with name` when publishing a Function to a Function App. As a workaround, re-run the script.
- .NET 5 with Azure Functions v3 is [not (yet) supported by Microsoft](https://github.com/Azure/azure-functions-host/issues/6674).
