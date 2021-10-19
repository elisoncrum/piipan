using System;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Piipan.Match.Api;
using Piipan.Shared.Authentication;
using Piipan.Shared.Http;

namespace Piipan.Match.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterMatchClientServices(this IServiceCollection serviceCollection, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                serviceCollection.AddTransient<TokenCredential, AzureCliCredential>();
            }
            else
            {
                serviceCollection.AddTransient<TokenCredential, ManagedIdentityCredential>();
            }

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