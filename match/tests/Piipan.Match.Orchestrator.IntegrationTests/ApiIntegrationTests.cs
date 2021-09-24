using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Npgsql;
using Piipan.Shared.Authentication;
using Xunit;


namespace Piipan.Match.Func.Api.IntegrationTests
{
    public class ApiIntegrationTests : DbFixture
    {
        static ParticipantRecord FullRecord()
        {
            return new ParticipantRecord
            {
                // farrington,1931-10-13,000-12-3456
                LdsHash = "eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458",
                CaseId = "CaseIdExample",
                ParticipantId = "ParticipantIdExample",
                BenefitsEndMonth = new DateTime(1970, 1, 31),
                RecentBenefitMonths = new List<DateTime>() {
                  new DateTime(2021, 5, 31),
                  new DateTime(2021, 4, 30),
                  new DateTime(2021, 3, 31)
                },
                ProtectLocation = true
            };
        }

        static String JsonBody(object[] records)
        {
            var data = new
            {
                data = records
            };

            return JsonConvert.SerializeObject(data);
        }

        static Mock<HttpRequest> MockRequest(string jsonBody)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            sw.Write(jsonBody);
            sw.Flush();

            ms.Position = 0;

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Body).Returns(ms);

            return mockRequest;
        }

        static Api Construct()
        {
            var factory = NpgsqlFactory.Instance;
            var tokenProvider = new EasyAuthTokenProvider();
            var api = new Api(factory, tokenProvider);

            return api;
        }

        [Fact]
        public async void ApiReturnsMatches()
        {
            // Arrange
            var record = FullRecord();
            var logger = Mock.Of<ILogger>();
            var body = new object[] { record };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();
            var state = Environment.GetEnvironmentVariable("States").Split(",");

            ClearParticipants();
            Insert(record);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Single(resultObject.Data.Results);

            var person = resultObject.Data.Results[0];
            Assert.Equal(record.CaseId, person.Matches[0].CaseId);
            Assert.Equal(record.ParticipantId, person.Matches[0].ParticipantId);
            Assert.Equal(state[0], person.Matches[0].State);
            Assert.Equal(record.BenefitsEndMonth, person.Matches[0].BenefitsEndMonth);
            Assert.Equal(record.RecentBenefitMonths, person.Matches[0].RecentBenefitMonths);
            Assert.Equal(record.ProtectLocation, person.Matches[0].ProtectLocation);
        }

        [Fact]
        public async void ApiReturnsEmptyMatchesArray()
        {
            // Arrange
            var record = FullRecord();
            var logger = Mock.Of<ILogger>();
            var body = new object[] { record };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            ClearParticipants();

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Empty(resultObject.Data.Results[0].Matches);
        }

        [Fact]
        public async void ApiReturnsInlineErrors()
        {
            // Arrange
            var recordA = FullRecord();
            var recordB = FullRecord();
            recordB.LdsHash = "foo";
            var logger = Mock.Of<ILogger>();
            var body = new object[] { recordA, recordB };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            ClearParticipants();
            Insert(recordA);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Single(resultObject.Data.Results);
            Assert.Single(resultObject.Data.Errors);
        }

        [Fact]
        public async void ApiReturnsExpectedIndices()
        {
            // Arrange
            var recordA = FullRecord();
            var recordB = FullRecord();
            // lynn,1940-08-01,000-12-3457
            recordB.LdsHash = "97719c32bb3c6a5e08c1241a7435d6d7047e75f40d8b3880744c07fef9d586954f77dc93279044c662d5d379e9c8a447ce03d9619ce384a7467d322e647e5d95";
            recordB.ParticipantId = "ParticipantB";
            var logger = Mock.Of<ILogger>();
            var body = new object[] { recordA, recordB };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            ClearParticipants();
            Insert(recordA);
            Insert(recordB);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            var resultA = resultObject.Data.Results.Find(p => p.Index == 0);
            var resultB = resultObject.Data.Results.Find(p => p.Index == 1);

            Assert.Equal(resultA.Matches[0].ParticipantId, recordA.ParticipantId);
            Assert.Equal(resultB.Matches[0].ParticipantId, recordB.ParticipantId);
        }
    }
}
