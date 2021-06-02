using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;


namespace Piipan.Match.State.IntegrationTests
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
                Exception = "Exception",
                CaseId = "CaseIdExample",
                ParticipantId = "ParticipantIdExample",
                BenefitsEndMonth = new DateTime(1970, 2, 1)
            };
        }

        static String JsonBody(string json)
        {
            var data = new
            {
                query = JsonConvert.DeserializeObject(json)
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

        [Fact]
        public async void ApiReturnsMatches()
        {
            // Arrange
            var record = FullRecord();
            var logger = Mock.Of<ILogger>();
            var mockRequest = MockRequest(JsonBody(record.ToJson()));

            ClearParticipants();
            Insert(record);

            // Act
            var response = await Api.Query(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultRecord = result.Value as MatchQueryResponse;

            // Assert
            Assert.Single(resultRecord.Matches);
            Assert.Equal(record.First, resultRecord.Matches[0].First);
            Assert.Equal(record.Middle, resultRecord.Matches[0].Middle);
            Assert.Equal(record.Last, resultRecord.Matches[0].Last);
            Assert.Equal(record.Dob, resultRecord.Matches[0].Dob);
            Assert.Equal(record.Ssn, resultRecord.Matches[0].Ssn);
            Assert.Equal(record.Exception, resultRecord.Matches[0].Exception);
            Assert.Equal(record.CaseId, resultRecord.Matches[0].CaseId);
            Assert.Equal(record.ParticipantId, resultRecord.Matches[0].ParticipantId);
            Assert.Equal("ea", resultRecord.Matches[0].State);
            Assert.Equal(record.BenefitsEndMonth, resultRecord.Matches[0].BenefitsEndMonth);
        }

        [Fact]
        public async void ApiReturnsEmptyArray()
        {
            // Arrange
            var record = FullRecord();
            var logger = Mock.Of<ILogger>();
            var mockRequest = MockRequest(JsonBody(record.ToJson()));

            ClearParticipants();

            // Act
            var response = await Api.Query(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultRecord = result.Value as MatchQueryResponse;

            // Assert
            Assert.Empty(resultRecord.Matches);
        }

        [Fact]
        public async void ApiReturnsBadRequestWhenValidationFails()
        {
            // Arrange
            var query = @"{last: 'Last', first: 'First', dob: '2020-01-01', ssn: '000-00-000'}";
            var mockRequest = MockRequest(JsonBody(query));
            var logger = Mock.Of<ILogger>();

            // Act
            var response = await Api.Query(mockRequest.Object, logger);

            // Assert
            Assert.IsType<BadRequestResult>(response);
        }

        [Theory]
        [InlineData("{last: 'farrington', first: 't', dob: '1931-10-13', ssn: '000-12-3456'}")]
        [InlineData("{last: 'FARRINGTON', first: 'T', dob: '1931-10-13', ssn: '000-12-3456'}")]
        public async void ApiMatchesCaseInsensitive(string query)
        {
            var record = new PiiRecord
            {
                First = "Theodore",
                Middle = "Carri",
                Last = "Farrington",
                Dob = new DateTime(1931, 10, 13),
                Ssn = "000-12-3456",
                CaseId = "CaseIdExample",
                BenefitsEndMonth = new DateTime(2021, 05, 31)
            };
            var logger = Mock.Of<ILogger>();
            var mockRequest = MockRequest(JsonBody(query));

            ClearParticipants();
            Insert(record);

            // Act
            var response = await Api.Query(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultRecord = result.Value as MatchQueryResponse;

            // Assert
            Assert.Single(resultRecord.Matches);
            Assert.Equal(new DateTime(2021, 05, 01), resultRecord.Matches[0].BenefitsEndMonth);
        }

        [Fact]
        public async void ApiMatchesIngoreFirstAndMiddle()
        {
            var query = "{last: 'Farrington', first: 'Foo', middle: 'Bar', dob: '1931-10-13', ssn: '000-12-3456'}";
            var record = new PiiRecord
            {
                First = "Theodore",
                Middle = "Carri",
                Last = "Farrington",
                Dob = new DateTime(1931, 10, 13),
                Ssn = "000-12-3456",
                CaseId = "CaseIdExample"
            };
            var logger = Mock.Of<ILogger>();
            var mockRequest = MockRequest(JsonBody(query));

            ClearParticipants();
            Insert(record);

            // Act
            var response = await Api.Query(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultRecord = result.Value as MatchQueryResponse;

            // Assert
            Assert.Single(resultRecord.Matches);
            Assert.Null(resultRecord.Matches[0].BenefitsEndMonth);
        }
    }
}
