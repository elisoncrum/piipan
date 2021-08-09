using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Piipan.Shared.Authorization
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureAuthorizationPolicy(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddAuthorizationCore(options => {
                var authzPolicyOptions = configuration
                    .GetSection(AuthorizationPolicyOptions.SectionName)
                    .Get<AuthorizationPolicyOptions>();

                var builder = new AuthorizationPolicyBuilder();

                // if no authorization policy is configured, forbid all requests
                if (authzPolicyOptions is null)
                {
                    builder.RequireAssertion(context => false);
                }
                else
                {
                    foreach (var rcv in authzPolicyOptions.RequiredClaims)
                    {
                        builder.RequireClaim(rcv.Type, rcv.Values);
                    }
                }

                options.DefaultPolicy = builder.Build();
            });
        }
    }
}