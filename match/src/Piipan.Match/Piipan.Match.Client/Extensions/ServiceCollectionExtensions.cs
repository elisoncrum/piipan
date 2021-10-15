using System;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Match.Api;
using Piipan.Shared.Authentication;
using Piipan.Shared.Http;

namespace Piipan.Match.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterMatchClientServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<TokenCredential, AzureCliCredential>();
            serviceCollection.AddHttpClient<MatchClient>((c) =>
            {
                c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("OrchApiUrl"));
            });
            serviceCollection.AddTransient<ITokenProvider<MatchClient>, AzureTokenProvider<MatchClient>>();
            serviceCollection.AddTransient<IAuthorizedApiClient<MatchClient>, AuthorizedJsonApiClient<MatchClient>>();
            serviceCollection.AddTransient<IMatchApi, MatchClient>();
        }
    }
}