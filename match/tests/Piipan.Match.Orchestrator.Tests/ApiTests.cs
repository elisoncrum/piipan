using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;


namespace Piipan.Match.Orchestrator.Tests
{
    public class ApiTests
    {
        void SetEnvironment()
        {
            Environment.SetEnvironmentVariable("StateApiHostStrings", "[\"https://localhost/api/v1/query\"]");
        }

        static PiiRecord FullRecord()
        {
            return new PiiRecord
            {
                First = "First",
                Middle = "Middle",
                Last = "Last",
                Dob = new DateTime(1970, 1, 1),
                Ssn = "000-00-0000",
                Exception = "Exception"
            };
        }

        static MatchQueryRequest FullRequest()
        {
            return new MatchQueryRequest
            {
                Query = new MatchQuery
                {
                    First = "First",
                    Middle = "Middle",
                    Last = "Last",
                    Dob = new DateTime(1970, 1, 1),
                    Ssn = "000-00-0000"
                }
            };
        }

        static MatchQueryResponse FullResponse()
        {
            return new MatchQueryResponse
            {
                Matches = new List<PiiRecord> { FullRecord() }
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

        static Mock<HttpMessageHandler> MockMessageHandler(HttpStatusCode status, string response)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = status,
                    Content = new StringContent(response, Encoding.UTF8, "application/json")
                });

            return mockHttpMessageHandler;
        }

        // HttpStatusCode.BadRequest

        // Non-required and empty/whitespace properties parse to null
        [Theory]
        [InlineData(@"{last: 'Last', dob: '2020-01-01', ssn: '000-00-0000'}")] // Missing optionals
        [InlineData(@"{last: 'Last', middle: '', first: '', dob: '2020-01-01', ssn: '000-00-0000'}")] // Empty optionals
        [InlineData(@"{last: 'Last', middle: '     ', first: '\n', dob: '2020-01-01', ssn: '000-00-0000'}")] // Whitespace optionals
        public void ExpectEmptyOptionalPropertiesToBeNull(string query)
        {
            // Arrage
            var body = JsonBody(query);
            var logger = Mock.Of<ILogger>();

            // Act
            var request = Api.Parse(body, logger);
            var valid = Api.Validate(request, logger);

            // Assert
            Assert.Null(request.Query.Middle);
            Assert.Null(request.Query.First);
            Assert.True(valid);
        }

        // Malformed data result in null Query
        [Theory]
        [InlineData("{{")]
        [InlineData("<xml>")]
        public void ExpectMalformedDataResultsInNullQuery(string query)
        {
            // Arrange
            var logger = Mock.Of<ILogger>();

            // Act
            var request = Api.Parse(query, logger);

            // Assert
            Assert.Null(request.Query);
        }

        // Malformed data results in BadRequest
        [Theory]
        [InlineData("{{")]
        [InlineData("<xml>")]
        public async void ExpectMalformedDataResultsInBadRequest(string query)
        {
            // Arrange
            Mock<HttpRequest> mockRequest = MockRequest(query);
            var logger = Mock.Of<ILogger>();

            // Act
            var response = await Api.Query(mockRequest.Object, logger);

            // Assert
            Assert.IsType<BadRequestResult>(response);
            var result = response as BadRequestResult;
            Assert.Equal(400, result.StatusCode);
        }

        // Incomplete data results in null Query
        [Theory]
        [InlineData(@"{}")]
        [InlineData(@"{first: 'First'}")] // Missing Last, Dob, and Ssn
        [InlineData(@"{last: 'Last'}")] // Missing Dob and Ssn
        [InlineData(@"{last: 'Last', dob: '2020-01-01'}")] // Missing Ssn
        [InlineData(@"{last: 'Last', dob: '2020-01-1', ssn: '000-00-000'}")] // Invalid Dob DateTime
        [InlineData(@"{last: 'Last', dob: '', ssn: '000-00-000'}")] // Invalid (empty) Dob DateTime
        public void ExpectQueryToBeNull(string query)
        {
            // Arrange
            var body = JsonBody(query);
            var logger = Mock.Of<ILogger>();

            // Act
            var request = Api.Parse(body, logger);

            // Assert
            Assert.Null(request.Query);
        }

        // Incomplete data returns badrequest
        // Note: date validation happens in `Api.Parse` not `Api.Validate`
        [Theory]
        [InlineData("")]
        [InlineData(@"{first: 'First'}")] // Missing Last, Dob, and Ssn
        [InlineData(@"{last: 'Last'}")] // Missing Dob and Ssn
        [InlineData(@"{last: 'Last', dob: '2020-01-01'}")] // Missing Ssn
        [InlineData(@"{last: 'Last', dob: '2020-01-1', ssn: '000-00-000'}")] // Invalid Dob DateTime
        [InlineData(@"{last: 'Last', dob: '', ssn: '000-00-000'}")] // Empty Dob DateTime
        public async void ExpectBadResultFromIncompleteData(string query)
        {
            // Arrange
            Mock<HttpRequest> mockRequest = MockRequest(JsonBody(query));
            var logger = Mock.Of<ILogger>();

            // Act
            var response = await Api.Query(mockRequest.Object, logger);

            // Assert
            Assert.IsType<BadRequestResult>(response);
            var result = response as BadRequestResult;
            Assert.Equal(400, result.StatusCode);
        }

        // Invalid data fails validation
        // Note: date validation happens in `Api.Parse` not `Api.Validate`
        [Theory]
        [InlineData(@"{last: 'Last', dob: '2020-01-01', ssn: '000-00-000'}")] // Invalid Ssn format
        [InlineData(@"{last: '', dob: '2020-01-01', ssn: '000-00-0000'}")] // Empty last
        [InlineData(@"{last: '        ', dob: '2020-01-01', ssn: '000-00-0000'}")] // Whitespace last
        [InlineData(@"{last: 'Last', dob: '2020-01-01', ssn: '000000000'}")] // Invalid Ssn format
        public void ExpectQueryToBeInvalid(string query)
        {
            // Arrange
            var body = JsonBody(query);
            var logger = Mock.Of<ILogger>();

            // Act
            var request = Api.Parse(body, logger);
            var valid = Api.Validate(request, logger);

            // Assert
            Assert.False(valid);
        }

        // Invalid data returns badrequest
        [Theory]
        [InlineData(@"{last: 'Last', dob: '2020-01-01', ssn: '000-00-000'}")] // Invalid Ssn format
        [InlineData(@"{last: '', dob: '2020-01-01', ssn: '000-00-0000'}")] // Empty last
        [InlineData(@"{last: '        ', dob: '2020-01-01', ssn: '000-00-0000'}")] // Whitespace last
        [InlineData(@"{last: 'Last', dob: '2020-01-01', ssn: '000000000'}")] // Invalid Ssn format
        public async void ExpectBadResultFromInvalidData(string query)
        {
            // Arrange
            Mock<HttpRequest> mockRequest = MockRequest(JsonBody(query));
            var logger = Mock.Of<ILogger>();

            // Act
            var response = await Api.Query(mockRequest.Object, logger);

            // Assert
            Assert.IsType<BadRequestResult>(response);
            var result = response as BadRequestResult;
            Assert.Equal(400, result.StatusCode);
        }

        // Parsed valid data returns object with matching properties
        [Fact]
        public void RequestObjectPropertiesMatchRequestJson()
        {
            // Arrange

            var body = JsonBody(@"{last:'Last', first: 'First', middle: 'Middle', dob: '1970-01-01', ssn: '000-00-0000'}");
            var bodyFormatted = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(body), Formatting.Indented);
            var logger = Mock.Of<ILogger>();

            // Act
            MatchQueryRequest request = Api.Parse(body, logger);

            // Assert
            Assert.Equal("Last", request.Query.Last);
            Assert.Equal("First", request.Query.First);
            Assert.Equal("Middle", request.Query.Middle);
            Assert.Equal("000-00-0000", request.Query.Ssn);
            Assert.Equal(new DateTime(1970, 1, 1), request.Query.Dob);
            Assert.Equal(bodyFormatted, request.ToJson());
        }

        [Fact]
        public async void OkResponseFromStateReturnsMatches()
        {
            // Arrange
            SetEnvironment();
            var logger = Mock.Of<ILogger>();
            var mockHttpMessageHandler = MockMessageHandler(HttpStatusCode.OK, FullResponse().ToJson());
            var client = new HttpClient(mockHttpMessageHandler.Object);
            var endpoints = Api.StateApiBaseUris();

            // Act
            var response = new MatchQueryResponse();
            response.Matches = await Api.Match(FullRequest(), client, logger);

            // Assert
            Assert.Equal(1, response.Matches.Count);
        }

        // [Fact]
        // public void ValidUris()
        // {
        //     Environment.SetEnvironmentVariable("StateApiHostStrings", "[\"https:;example.gov/\"]");
        //     IEnumerable<Uri> uris = Api.StateApiBaseUris();

        //     foreach (var uri in uris)
        //     {
        //         Console.WriteLine(uri.AbsoluteUri);
        //         Console.WriteLine(uri.Scheme);
        //         Console.WriteLine(uri.Host);
        //     }
        // }

        [Fact]
        public async void BadResponseFromStateThrowsException()
        {
            // Arrange
            SetEnvironment();
            var logger = Mock.Of<ILogger>();
            var mockHttpMessageHandler = MockMessageHandler(HttpStatusCode.BadRequest, "");
            var client = new HttpClient(mockHttpMessageHandler.Object);

            // Act/Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await Api.Match(FullRequest(), client, logger);
            });
        }

        // XXX Test exception handling in `Api.Query` via injected HttpClient
    }
}
