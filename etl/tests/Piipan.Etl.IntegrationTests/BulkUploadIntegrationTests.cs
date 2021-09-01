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
        static string LDS_HASH = "04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef";

        static Stream CsvFixture(string[] records)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.WriteLine("lds_hash,case_id,participant_id,benefits_end_month,recent_benefit_months,protect_location");
            foreach (var record in records) writer.WriteLine(record);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

        [Fact]
        public async void SavesCsvRecords()
        {
            // setup
            ClearParticipants();
            var eventGridEvent = Mock.Of<EventGridEvent>();
            eventGridEvent.Data = new Object();
            Stream input = CsvFixture(new string[] {
                $"{LDS_HASH},CaseId,ParticipantId,1970-01,2021-05 2021-04 2021-03,true"
            });
            var logger = Mock.Of<ILogger>();
            // act
            await BulkUpload.Run(
                eventGridEvent,
                input,
                logger
            );
            var records = QueryParticipants("SELECT * from participants;");
            // assert
            Assert.Equal(LDS_HASH, records[0].LdsHash);
            Assert.Equal("CaseId", records[0].CaseId);
            Assert.Equal("ParticipantId", records[0].ParticipantId);
            Assert.Equal(new DateTime(1970, 01, 31), records[0].BenefitsEndDate);
            Assert.Equal(new DateTime(2021, 05, 31), records[0].RecentBenefitMonths[0]);
            Assert.Equal(new DateTime(2021, 04, 30), records[0].RecentBenefitMonths[1]);
            Assert.Equal(new DateTime(2021, 03, 31), records[0].RecentBenefitMonths[2]);
            Assert.True(records[0].ProtectLocation);
        }
    }
}
