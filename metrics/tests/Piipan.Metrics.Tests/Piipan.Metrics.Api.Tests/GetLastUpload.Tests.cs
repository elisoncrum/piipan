// using System;
// using System.Collections.Generic;
// using System.Data.Common;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Primitives;
// using Moq;
// using Piipan.Metrics.Api;
// using Piipan.Metrics.Models;
// using Xunit;

// #nullable enable

// namespace Piipan.Metrics.Tests
// {
//     namespace Piipan.Metrics.Api.Tests
//     {
//         public class GetLastUploadTests
//         {
//             static HttpRequest EventMock()
//             {
//                 var context = new DefaultHttpContext();
//                 return context.Request;
//             }

//             static Mock<ILogger> Logger()
//             {
//                 return new Mock<ILogger>();
//             }

//             public class ResultsQueryTests
//             {
//                 [Fact]
//                 public async void ResultsQuerySuccess() {
//                     Environment.SetEnvironmentVariable("KeyVaultName", "foo");
//                     var logger = Logger();
//                     var factory = new Mock<DbProviderFactory>() { DefaultValue = DefaultValue.Mock };
//                     var cmd = new Mock<DbCommand>() { DefaultValue = DefaultValue.Mock };
//                     var requestQuery = new Mock<IQueryCollection>();
//                     requestQuery.SetupGet(q => q["state"]).Returns(new StringValues("ea"));
//                     factory.Setup(f => f.CreateCommand()).Returns(cmd.Object);

//                     var results = await GetParticipantUploads.ResultsQuery(
//                         factory.Object,
//                         requestQuery.Object,
//                         logger.Object
//                     );

//                     Assert.Equal(new List<ParticipantUpload>(), results);

//                     //teardown
//                     Environment.SetEnvironmentVariable("KeyVaultName", null);
//                 }

//             }
//         }
//     }
// }
