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


namespace Piipan.Match.Orchestrator.IntegrationTests
{
    public class ApiIntegrationTests : DbFixture
    {
        static PiiRecord FullRecord()
        {
            return new PiiRecord
            {
                First = "First",
                Middle = "Middle",
                Last = "Last",
                Dob = new DateTime(1970, 1, 1),
                Ssn = "000-00-0000",
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
            var response = await api.Query(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Single(resultObject.Data.Results);

            var person = resultObject.Data.Results[0];
            Assert.Equal(record.First, person.Matches[0].First);
            Assert.Equal(record.Middle, person.Matches[0].Middle);
            Assert.Equal(record.Last, person.Matches[0].Last);
            Assert.Equal(record.Dob, person.Matches[0].Dob);
            Assert.Equal(record.Ssn, person.Matches[0].Ssn);
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
            var response = await api.Query(mockRequest.Object, logger);
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
            recordB.Ssn = "00-00-00";
            var logger = Mock.Of<ILogger>();
            var body = new object[] { recordA, recordB };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            ClearParticipants();
            Insert(recordA);

            // Act
            var response = await api.Query(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Single(resultObject.Data.Results);
            Assert.Single(resultObject.Data.Errors);
        }

        [Fact]
        public async void ApiReturnsMultipleValidationErrors()
        {
            // Arrange
            var recordA = FullRecord();
            var logger = Mock.Of<ILogger>();

            ClearParticipants();
            Insert(recordA);

            recordA.Ssn = "00-00-0000";
            recordA.First = "";
            var body = new object[] { recordA };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            // Act
            var response = await api.Query(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Equal(2, resultObject.Data.Errors.Count);
        }

        [Fact]
        public async void ApiReturnsExpectedIndices()
        {
            // Arrange
            var recordA = FullRecord();
            var recordB = FullRecord();
            recordB.Last = "LastB";
            var logger = Mock.Of<ILogger>();
            var body = new object[] { recordA, recordB };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            ClearParticipants();
            Insert(recordA);
            Insert(recordB);

            // Act
            var response = await api.Query(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            var resultA = resultObject.Data.Results.Find(p => p.Index == 0);
            var resultB = resultObject.Data.Results.Find(p => p.Index == 1);

            Assert.Equal(resultA.Matches[0].Last, recordA.Last);
            Assert.Equal(resultB.Matches[0].Last, recordB.Last);
        }
    }
}
