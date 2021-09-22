using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Piipan.Etl.Func.BulkUpload.Tests
{
    public class BulkUploadTests
    {
        // static Stream BadBlob()
        // {
        //     var stream = new MemoryStream();
        //     var writer = new StreamWriter(stream);
        //     writer.WriteLine("foo");
        //     writer.Flush();
        //     stream.Position = 0;

        //     return stream;
        // }

        // static PiiRecord AllFields()
        // {
        //     return new PiiRecord
        //     {
        //         LdsHash = LDS_HASH,
        //         CaseId = "CaseId",
        //         ParticipantId = "ParticipantId",
        //         BenefitsEndDate = new DateTime(1970, 1, 1),
        //         RecentBenefitMonths = new List<DateTime>() {
        //           new DateTime(2021, 5, 31),
        //           new DateTime(2021, 4, 30),
        //           new DateTime(2021, 3, 31)
        //         },
        //         ProtectLocation = true
        //     };
        // }

        // static PiiRecord OnlyRequiredFields()
        // {
        //     return new PiiRecord
        //     {
        //         LdsHash = LDS_HASH,
        //         CaseId = "CaseId",
        //         ParticipantId = null,
        //         BenefitsEndDate = null,
        //         RecentBenefitMonths = new List<DateTime>()
        //     };
        // }

        // static EventGridEvent EventMock()
        // {
        //     var e = Mock.Of<EventGridEvent>();
        //     // Can't override Data in Setup, just use a real one
        //     e.Data = new Object();
        //     return e;
        // }

        // // Check that the expected message was logged as an error at least once
        // static void VerifyLogError(Mock<ILogger> logger, String expected)
        // {
        //     logger.Verify(x => x.Log(
        //         It.Is<LogLevel>(l => l == LogLevel.Error),
        //         It.IsAny<EventId>(),
        //         It.Is<It.IsAnyType>((v, t) => v.ToString() == expected),
        //         It.IsAny<Exception>(),
        //         It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
        //     ));
        // }

        

        

        

        

        // [Fact]
        // public async void CountInserts()
        // {
        //     var logger = Mock.Of<ILogger>();
        //     var factory = new Mock<DbProviderFactory>() { DefaultValue = DefaultValue.Mock };
        //     var cmd = new Mock<DbCommand>() { DefaultValue = DefaultValue.Mock };
        //     factory.Setup(f => f.CreateCommand()).Returns(cmd.Object);

        //     // Mocks foreign key used in participants table
        //     cmd.Setup(c => c.ExecuteScalar()).Returns((Int64)1);

        //     // Mock can't test unique constraint on SSN
        //     var records = new List<PiiRecord>() {
        //         AllFields(),
        //         OnlyRequiredFields(),
        //     };
        //     await BulkUpload.Load(records, factory.Object, logger);

        //     // Row in uploads table + rows in participants table
        //     cmd.Verify(f => f.ExecuteNonQuery(), Times.Exactly(1 + records.Count));
        // }

        // [Fact]
        // public async void NoInputStream()
        // {
        //     var gridEvent = EventMock();
        //     var logger = new Mock<ILogger>();

        //     Stream input = null;
        //     await BulkUpload.Run(gridEvent, input, logger.Object);
        //     VerifyLogError(logger, "No input stream was provided");
        // }

        // [Fact]
        // public async void BadInputStream()
        // {
        //     var gridEvent = EventMock();
        //     var logger = new Mock<ILogger>();

        //     await Assert.ThrowsAnyAsync<Exception>(async () =>
        //     {
        //         await BulkUpload.Run(gridEvent, BadBlob(), logger.Object);
        //     });
        // }
    }
}
