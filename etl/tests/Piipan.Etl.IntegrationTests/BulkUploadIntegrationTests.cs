using System;
using System.IO;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Xunit;

namespace Piipan.Etl.IntegrationTests
{
    /// <summary>
    /// Integration tests for saving csv records to the database on a bulk upload
    /// </summary>
    public class BulkUploadIntegrationTests : DbFixture
    {
        [Fact]
        public async void SavesCsvRecords()
        {
            // setup
            ClearParticipants();
            var eventGridEvent = Mock.Of<EventGridEvent>();
            eventGridEvent.Data = new Object();
            var input = new MemoryStream(File.ReadAllBytes("example.csv"));
            var logger = Mock.Of<ILogger>();
            // act
            await BulkUpload.Run(
                eventGridEvent,
                input,
                logger
            );
            var records = QueryParticipants("SELECT * from participants;");
            // assert
            Assert.Equal("eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458", records[0].LdsHash);
            Assert.Equal("caseid1", records[0].CaseId);
            Assert.Equal("participantid1", records[0].ParticipantId);
            Assert.Equal(new DateTime(2021, 05, 31), records[0].BenefitsEndDate);
            Assert.Equal(new DateTime(2021, 04, 30), records[0].RecentBenefitMonths[0]);
            Assert.Equal(new DateTime(2021, 03, 31), records[0].RecentBenefitMonths[1]);
            Assert.Equal(new DateTime(2021, 02, 28), records[0].RecentBenefitMonths[2]);
            Assert.True(records[0].ProtectLocation);
        }
    }
}
