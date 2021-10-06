using System;
using System.Data.Common;
using FluentValidation;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Services;
using Piipan.Match.Core.Validators;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Extensions;
using Piipan.Shared.Database;

[assembly: FunctionsStartup(typeof(Piipan.Match.Func.Api.Startup))]

namespace Piipan.Match.Func.Api
{
    public class Startup : FunctionsStartup
    {
        public const string DatabaseConnectionString = "DatabaseConnectionString";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            builder.Services.AddLogging();

            builder.Services.AddTransient<IValidator<OrchMatchRequest>, OrchMatchRequestValidator>();
            builder.Services.AddTransient<IValidator<RequestPerson>, RequestPersonValidator>();

            builder.Services.AddTransient<IStreamParser<OrchMatchRequest>, OrchMatchRequestParser>();

            builder.Services.AddTransient<IMatchApi, MatchService>();

            builder.Services.AddSingleton<DbProviderFactory>(NpgsqlFactory.Instance);
            builder.Services.AddTransient<IDbConnectionFactory<ParticipantsDb>>(s =>
            {
                return new AzurePgConnectionFactory<ParticipantsDb>(
                    new AzureServiceTokenProvider(),
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(DatabaseConnectionString)
                );
            });

            builder.Services.RegisterParticipantsServices();
        }
    }
}
