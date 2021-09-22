using System;
using System.Data;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Participants.Core.Extensions;
using Piipan.Shared;

[assembly: FunctionsStartup(typeof(Piipan.Etl.Func.BulkUpload.Startup))]

namespace Piipan.Etl.Func.BulkUpload
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();
            
            builder.Services.AddTransient<IDbConnectionFactory, AzurePgConnectionFactory>();
            builder.Services.AddTransient<IParticipantStreamParser, ParticipantCsvStreamParser>();

            builder.Services.RegisterParticipantsServices();
        }
    }
}