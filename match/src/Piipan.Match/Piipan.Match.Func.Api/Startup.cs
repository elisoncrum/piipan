using System.Data.Common;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Services;
using Piipan.Match.Core.Validators;
using Piipan.Participants.Core.Extensions;
using Piipan.Shared;
using FluentValidation;
using Npgsql;

[assembly: FunctionsStartup(typeof(Piipan.Match.Func.Api.Startup))]

namespace Piipan.Match.Func.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            builder.Services.AddLogging();

            builder.Services.AddTransient<IValidator<OrchMatchRequest>, OrchMatchRequestValidator>();
            builder.Services.AddTransient<IValidator<RequestPerson>, RequestPersonValidator>();

            builder.Services.AddTransient<IStreamParser<OrchMatchRequest>, OrchMatchRequestParser>();

            builder.Services.AddTransient<IMatchApi, MatchService>();

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
