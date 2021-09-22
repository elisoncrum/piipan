using System;
using System.IO;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Piipan.Participants.Core.Extensions;
using Moq;
using Xunit;
using System.Data;
using Piipan.Participants.Api;
using Piipan.Etl.Func.BulkUpload.Parsers;
using System.Linq;

namespace Piipan.Etl.Func.BulkUpload.IntegrationTests
{
    /// <summary>
    /// Integration tests for saving csv records to the database on a bulk upload
    /// </summary>
    public class BulkUploadIntegrationTests : DbFixture
    {
        private ServiceProvider BuildServices()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddTransient<IDbConnection>(c =>
            {
                var connection = Factory.CreateConnection();
                connection.ConnectionString = ConnectionString;
                connection.Open();
                return connection;
            });

            services.AddTransient<IParticipantStreamParser, ParticipantCsvStreamParser>();
            services.RegisterParticipantsServices();

            return services.BuildServiceProvider();
        }

        private BulkUpload BuildFunction()
        {
            var services = BuildServices();
            return new BulkUpload(
                services.GetService<IParticipantApi>(),
                services.GetService<IParticipantStreamParser>()
            );
        }

        [Fact]
        public async void SavesCsvRecords()
        {
            // setup
            var services = BuildServices();
            ClearParticipants();
            var eventGridEvent = Mock.Of<EventGridEvent>();
            eventGridEvent.Data = new Object();
            var input = new MemoryStream(File.ReadAllBytes("example.csv"));
            var logger = Mock.Of<ILogger>();
            var function = BuildFunction();

            // act
            await function.Run(
                eventGridEvent,
                input,
                logger
            );

            var records = QueryParticipants("SELECT * from participants;").ToList();
            
            // assert
            for (int i = 0; i < records.Count(); i++)
            {
                Assert.Equal($"caseid{i+1}", records.ElementAt(i).CaseId);
                Assert.Equal($"participantid{i+1}", records.ElementAt(i).ParticipantId);
            }
            Assert.Equal("eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458", records.First().LdsHash);
            Assert.Equal(new DateTime(2021, 05, 31), records.First().BenefitsEndDate);
            Assert.Equal(new DateTime(2021, 04, 30), records.First().RecentBenefitMonths.First());
            Assert.Equal(new DateTime(2021, 03, 31), records.First().RecentBenefitMonths.ElementAt(1));
            Assert.Equal(new DateTime(2021, 02, 28), records.First().RecentBenefitMonths.ElementAt(2));
            Assert.True(records.First().ProtectLocation);
        }
    }
}
