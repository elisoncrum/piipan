using System.Data.Common;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Piipan.Participants.Core.Extensions;
using Piipan.Shared;
using Piipan.Shared.Authentication;

[assembly: FunctionsStartup(typeof(Piipan.Match.Func.Api.Startup))]
namespace Piipan.Match.Func.Api
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
            builder.Services.AddTransient<IDbConnectionFactory>(s =>
            {
                return new AzurePgConnectionFactory(
                    new AzureServiceTokenProvider(),
                    NpgsqlFactory.Instance
                );
            });
            
            builder.Services.RegisterParticipantsServices();
        }
    }
}
