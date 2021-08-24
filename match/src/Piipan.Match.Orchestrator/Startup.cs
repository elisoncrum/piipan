using System.Data.Common;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Piipan.Shared.Authentication;

[assembly: FunctionsStartup(typeof(Piipan.Match.Orchestrator.Startup))]
namespace Piipan.Match.Orchestrator
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            builder.Services.AddSingleton<ITokenProvider>((s) =>
            {
                if (configuration?["DEVELOPMENT"] == "true")
                {
                    return new CliTokenProvider();
                }
                return new EasyAuthTokenProvider();
            });
            builder.Services.AddSingleton<DbProviderFactory>(NpgsqlFactory.Instance);
        }
    }
}
