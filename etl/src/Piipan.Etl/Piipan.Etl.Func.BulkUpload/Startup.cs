using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Piipan.Etl.Func.BulkUpload.Startup))]

namespace Piipan.Etl.Func.BulkUpload
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

        }
    }
}