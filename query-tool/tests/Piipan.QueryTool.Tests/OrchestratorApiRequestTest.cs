using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.QueryTool.Pages;
using Piipan.Shared.Authentication;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class OrchestratorApiRequestTests
    {
        [Fact]
        public async void TestMatch()
        {
            // arrange
            var mockResponse = @"{
                ""lookup_id"": ""BBB2222"",
                ""matches"": [
                    {
                        ""first"": ""Theodore"",
                        ""middle"": ""Carri"",
                        ""last"": ""Farrington"",
                        ""ssn"": ""000-00-0000"",
                        ""dob"": ""2021-01-01"",
                        ""state_abbr"": ""ea"",
                        ""state_name"": ""Echo Alpha""
                    }
                ]
            }";
            var query = new PiiRecord
            {
                FirstName = "Theodore",
                MiddleName = "Carri",
                LastName = "Farrington"
            };
            var clientMock = new Mock<IAuthorizedApiClient>();
            clientMock
                .Setup(c => c.PostAsync(It.IsAny<Uri>(), It.IsAny<StringContent>()).Result)
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockResponse)
                });
            var api = new OrchestratorApiRequest(
                clientMock.Object,
                new Uri("https://localhost/"),
                new NullLogger<IndexModel>());

            // act
            var result = await api.Match(query);

            // assert
            Assert.IsType<MatchResponse>(result);
            Assert.Single(result.matches);
            Assert.Equal("BBB2222", result.lookupId);
            Assert.Equal("Theodore", result.matches[0].FirstName);
            Assert.Equal("Carri", result.matches[0].MiddleName);
            Assert.Equal("Farrington", result.matches[0].LastName);
            Assert.Equal("000-00-0000", result.matches[0].SocialSecurityNum);
            Assert.Equal(new DateTime(2021, 1, 1), result.matches[0].DateOfBirth);
            Assert.Equal("ea", result.matches[0].StateAbbr);
            Assert.Equal("Echo Alpha", result.matches[0].StateName);
        }

        [Fact]
        public async void TestLookup()
        {
            var mockResponse = @"{
                ""data"": {
                    ""first"": ""Theodore"",
                    ""middle"": ""Carri"",
                    ""last"": ""Farrington"",
                    ""ssn"": ""000-00-0000"",
                    ""dob"": ""2021-01-01""
                }
            }";
            var clientMock = new Mock<IAuthorizedApiClient>();
            clientMock
                .Setup(c => c.GetAsync(It.IsAny<Uri>()).Result)
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(mockResponse)
                });
            var api = new OrchestratorApiRequest(
                clientMock.Object,
                new Uri("https://localhost/"),
                new NullLogger<IndexModel>());

            // act
            var result = await api.Lookup("BCD2345");

            // assert
            Assert.IsType<LookupResponse>(result);
            Assert.Equal("Theodore", result.data.FirstName);
            Assert.Equal("Carri", result.data.MiddleName);
            Assert.Equal("Farrington", result.data.LastName);
            Assert.Equal("000-00-0000", result.data.SocialSecurityNum);
            Assert.Equal(new DateTime(2021, 1, 1), result.data.DateOfBirth);
        }
    }
}
