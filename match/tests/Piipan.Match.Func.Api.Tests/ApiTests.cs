using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Npgsql;
using Piipan.Match.Func.Api.Models;
using Piipan.Match.Shared;
using Piipan.Participants.Api;
using Piipan.Shared.Authentication;
using Xunit;

namespace Piipan.Match.Func.Api.Tests
{
    public class ApiTests
    {
        static Participant FullRecord()
        {
            return new Participant
            {
                CaseId = "CaseIdExample",
                BenefitsEndDate = new DateTime(1970, 1, 31),
                RecentBenefitMonths = new List<DateTime>() {
                  new DateTime(2021, 5, 31),
                  new DateTime(2021, 4, 30),
                  new DateTime(2021, 3, 31)
                },
                ProtectLocation = true
            };
        }

        static OrchMatchRequest FullRequest()
        {
            return new OrchMatchRequest
            {
                Data = new List<RequestPerson>() {
                    new RequestPerson
                    {
                        // farrington,1931-10-13,000-12-3456
                        LdsHash = "eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458"
                    }
                }
            };
        }

        static OrchMatchRequest FullRequestMultiple()
        {
            return new OrchMatchRequest
            {
                Data = new List<RequestPerson>() {
                    new RequestPerson
                    {
                        // farrington,1931-10-13,000-12-3456
                        LdsHash = "eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458"
                    },
                    new RequestPerson
                    {
                        // lynn,1940-08-01,000-12-3457
                        LdsHash = "97719c32bb3c6a5e08c1241a7435d6d7047e75f40d8b3880744c07fef9d586954f77dc93279044c662d5d379e9c8a447ce03d9619ce384a7467d322e647e5d95"
                    }
                }
            };
        }

        static OrchMatchRequest OverMaxRequest()
        {
            var list = new List<RequestPerson>();
            for (int i = 0; i < 51; i++)
            {
                list.Add(new RequestPerson
                {
                    // farrington,1931-10-13,000-12-3456
                    LdsHash = "eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458"
                });
            }
            return new OrchMatchRequest { Data = list };
        }

        static OrchMatchResult StateResponse()
        {
            var stateResponse = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<Participant> { FullRecord() }
            };
            return stateResponse;
        }

        static String JsonBody(string json)
        {
            var data = new
            {
                data = JsonConvert.DeserializeObject(json)
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

            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { "From", "foobar"}
            }) as IHeaderDictionary;
            mockRequest.Setup(x => x.Headers).Returns(headers);

            return mockRequest;
        }

        static HttpResponseMessage MockResponse(System.Net.HttpStatusCode statusCode, string body)
        {
            return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        }

        static Mock<HttpMessageHandler> MockMessageHandler(List<HttpResponseMessage> responses)
        {
            var responseQueue = new Queue<HttpResponseMessage>();
            foreach (HttpResponseMessage response in responses)
            {
                responseQueue.Enqueue(response);
            }
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseQueue.Dequeue)
                .Verifiable();

            return mockHttpMessageHandler;
        }

        static MatchApi Construct()
        {
            var participantApi = new Mock<IParticipantApi>();
            var requestValidator = new OrchMatchRequestValidator();
            var requestPersonValidator = new PersonValidator();

            var api = new MatchApi(participantApi.Object, requestValidator, requestPersonValidator);

            return api;
        }

        static MatchApi ConstructMocked(Mock<HttpMessageHandler> handler)
        {
            var participantApi = Mock.Of<IParticipantApi>();
            var requestValidator = new OrchMatchRequestValidator();
            var requestPersonValidator = new PersonValidator();

            var api = new MatchApi(participantApi, requestValidator, requestPersonValidator);

            return api;
        }

        ////
        // Tests
        ////

        [Fact]
        public void ParticipantRecordJson()
        {
            var json = @"{participant_id: 'baz', case_id: 'foo', benefits_end_month: '2020-01', recent_benefit_months: ['2019-12', '2019-11', '2019-10'], protect_location: true}";
            var record = JsonConvert.DeserializeObject<Participant>(json);

            string jsonRecord = record.ToJson();

            Assert.Contains("\"state\": null", jsonRecord);
            Assert.Contains("\"participant_id\": \"baz\"", jsonRecord);
            Assert.Contains("\"case_id\": \"foo\"", jsonRecord);
            Assert.Contains("\"benefits_end_month\": \"2020-01\"", jsonRecord);
            Assert.Contains("\"recent_benefit_months\": [", jsonRecord);
            Assert.Contains("\"2019-12\",", jsonRecord);
            Assert.Contains("\"2019-11\",", jsonRecord);
            Assert.Contains("\"2019-10\"", jsonRecord);
            Assert.Contains("\"protect_location\": true", jsonRecord);
        }

        // Malformed request results in BadRequest
        [Theory]
        [InlineData("")]
        [InlineData("{{")]
        [InlineData("<xml>")]
        public async void ExpectMalformedRequestResultsInBadRequest(string query)
        {
            // Arrange
            var api = Construct();
            Mock<HttpRequest> mockRequest = MockRequest(query);
            var logger = Mock.Of<ILogger>();

            // Act
            var response = await api.Find(mockRequest.Object, logger);

            // Assert
            var result = response as BadRequestObjectResult;
            Assert.Equal(400, result.StatusCode);

            var errorResponse = result.Value as ApiErrorResponse;
            Assert.Equal(1, (int)errorResponse.Errors.Count);
            Assert.Equal("400", errorResponse.Errors[0].Status);
            Assert.NotEmpty(errorResponse.Errors[0].Title);
            Assert.NotEmpty(errorResponse.Errors[0].Detail);
        }

        [Theory]
        [InlineData("{ data: 'foobar' }")]
        [InlineData("{ data: [] }")]
        public async void ExpectMalformedDataResultsInBadRequest(string query)
        {
            // Arrange
            var api = Construct();
            Mock<HttpRequest> mockRequest = MockRequest(query);
            var logger = Mock.Of<ILogger>();

            // Act
            var response = await api.Find(mockRequest.Object, logger);

            // Assert
            var result = response as BadRequestObjectResult;
            Assert.Equal(400, result.StatusCode);

            var errorResponse = result.Value as ApiErrorResponse;
            Assert.Equal(1, (int)errorResponse.Errors.Count);
            Assert.Equal("400", errorResponse.Errors[0].Status);
            Assert.NotEmpty(errorResponse.Errors[0].Title);
            Assert.NotEmpty(errorResponse.Errors[0].Detail);
        }

        // Invalid person-level results in item-level validation errors
        [Theory]
        [InlineData(@"[{lds_hash: 'abc'}]")] // Invalid hash
        [InlineData(@"[{lds_hash: ''}]")] // Empty hash
        public async void ExpectBadResultFromInvalidPersonData(string query)
        {
            // Arrange
            var api = Construct();
            Mock<HttpRequest> mockRequest = MockRequest(JsonBody(query));
            var logger = Mock.Of<ILogger>();

            // Act
            var response = await api.Find(mockRequest.Object, logger);

            // Assert
            var result = response as JsonResult;
            Assert.Equal(200, result.StatusCode);

            var res = result.Value as OrchMatchResponse;
            var data = res.Data;
            Assert.Single(data.Errors);
            Assert.NotEmpty(data.Errors.First().Code);
            Assert.NotEmpty(data.Errors.First().Detail);
        }

        // Incomplete person-level results in top-level bad request response
        [Theory]
        [InlineData(@"[{ssn: '000-00-0000'}]")] // Missing hash
        [InlineData(@"[{}]")] // Empty record
        public async void ExpectBadResultFromIncompleteData(string query)
        {
            // Arrange
            var api = Construct();
            Mock<HttpRequest> mockRequest = MockRequest(JsonBody(query));
            var logger = Mock.Of<ILogger>();

            // Act
            var response = await api.Find(mockRequest.Object, logger);

            // Assert
            var result = response as BadRequestObjectResult;
            Assert.Equal(400, result.StatusCode);

            var errorResponse = result.Value as ApiErrorResponse;
            Assert.Equal(1, (int)errorResponse.Errors.Count);
            Assert.Equal("400", errorResponse.Errors[0].Status);
            Assert.NotEmpty(errorResponse.Errors[0].Title);
            Assert.NotEmpty(errorResponse.Errors[0].Detail);
        }

        // Required services are passed to Api on startup
        [Fact]
        public void DependencyInjection()
        {
            var startup = new Startup();
            var host = new HostBuilder()
                .ConfigureWebJobs(startup.Configure)
                .Build();

            Assert.NotNull(host);
            Assert.NotNull(host.Services.GetRequiredService<ITokenProvider>());
            Assert.NotNull(host.Services.GetRequiredService<DbProviderFactory>());
        }

        // Whole thing blows up and returns a top-level error
        [Fact]
        public async void ReturnsInternalServerError()
        {
            // Arrange
            var api = Construct();
            Mock<HttpRequest> mockRequest = MockRequest("foobar");
            var logger = new Mock<ILogger>();

            // Set up first log to throw an exception
            // How to mock LogInformation: https://stackoverflow.com/a/58413842
            logger.SetupSequence(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Throws(new Exception("example message"));

            // Act
            var response = await api.Find(mockRequest.Object, logger.Object);
            var result = response as JsonResult;
            var resBody = result.Value as ApiErrorResponse;
            var error = resBody.Errors[0];

            // Assert
            Assert.Equal(500, result.StatusCode);
            Assert.NotEmpty(resBody.Errors);
            Assert.Equal("500", error.Status);
            Assert.NotNull(error.Title);
            Assert.NotNull(error.Detail);
        }

        [Fact]
        public async void LogsApimSubscriptionIfPresent()
        {
            // Arrange
            var api = Construct();
            var mockRequest = MockRequest("foobar");
            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Ocp-Apim-Subscription-Name", "sub-name" }
                }));

            var logger = new Mock<ILogger>();

            // Act
            await api.Find(mockRequest.Object, logger.Object);

            // Assert
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using APIM subscription sub-name")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }
    }
}
