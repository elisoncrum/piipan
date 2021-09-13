// using System;
// using System.Data.Common;
// using Xunit;
// using Moq;
// using Microsoft.Azure.EventGrid.Models;
// using Microsoft.Extensions.Logging;
// using Piipan.Metrics.Func.Collect;

// namespace Piipan.Metrics.Tests
// {
//     public class BulkUploadMetricsTests
//     {
//         public string badUrl = "eastate";

//         static EventGridEvent EventMock(Object dataObject)
//         {
//             var e = Mock.Of<EventGridEvent>();
//             // Can't override Data in Setup, just use a real one
//             e.Data = dataObject;
//             return e;
//         }

//         // Expect to throw exception when state is not found
//         [Fact]
//         public async void RunFailure()
//         {
//             var gridEvent = EventMock(new { url = badUrl });
//             var logger = new Mock<ILogger>();

//             await Assert.ThrowsAnyAsync<FormatException>(async () =>
//             {
//                 await BulkUploadMetrics.Run(gridEvent, logger.Object);
//             });
//         }

//         // Expect to add 1 record to metrics database
//         [Fact]
//         public async void WriteSuccess()
//         {
//             Environment.SetEnvironmentVariable("KeyVaultName", "foo");
//             string state = "eb";
//             var date = new DateTime();
//             var logger = Mock.Of<ILogger>();
//             var factory = new Mock<DbProviderFactory>() { DefaultValue = DefaultValue.Mock };
//             var cmd = new Mock<DbCommand>() { DefaultValue = DefaultValue.Mock };
//             factory.Setup(f => f.CreateCommand()).Returns(cmd.Object);

//             await BulkUploadMetrics.Write(state, date, factory.Object, logger);

//             cmd.Verify(f => f.ExecuteNonQuery(), Times.Exactly(1));
//             // teardown
//             Environment.SetEnvironmentVariable("KeyVaultName", null);
//         }
//     }
// }
