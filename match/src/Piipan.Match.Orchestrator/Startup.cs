using System.Net.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Shared.Authentication;

[assembly: FunctionsStartup(typeof(Piipan.Match.Orchestrator.Startup))]
namespace Piipan.Match.Orchestrator
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IAuthorizedApiClient>((s) =>
            {
                var client = new HttpClient();
                var tokenProvider = new EasyAuthTokenProvider();

                return new AuthorizedJsonApiClient(client, tokenProvider);
            });
        }
    }
}
