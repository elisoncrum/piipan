using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.QueryTool.Pages;
using Piipan.Shared.Authentication;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class LookupByIdPageTests
    {
        public LookupByIdPageTests()
        {
            Environment.SetEnvironmentVariable("OrchApiUri", "https://localhost/");
        }

        public static IAuthorizedApiClient clientMock(HttpStatusCode statusCode, string returnValue)
        {
            var clientMock = new Mock<IAuthorizedApiClient>();
            clientMock
                .Setup(c => c.GetAsync(It.Is<Uri>(u => u.ToString().Contains("lookup_ids/"))))
                .Returns(Task.FromResult(new HttpResponseMessage()
                {
                    StatusCode = statusCode,
                    Content = new StringContent(returnValue)
                }));

            return clientMock.Object;
        }

        public static IClaimsProvider claimsProviderMock(string email)
        {
            var claimsProviderMock = new Mock<IClaimsProvider>();
            claimsProviderMock
                .Setup(c => c.GetEmail(It.IsAny<ClaimsPrincipal>()))
                .Returns(email);
            return claimsProviderMock.Object;
        }

        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange
            var mockApiClient = Mock.Of<IAuthorizedApiClient>();
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var pageModel = new LookupByIdModel(
                new NullLogger<LookupByIdModel>(),
                mockApiClient,
                mockClaimsProvider
                );
            // act
            // assert
            Assert.Equal("", pageModel.Title);
            Assert.Equal("", pageModel.Email);
        }

        [Fact]
        public void TestAfterOnGet()
        {
            // arrange
            var mockApiClient = Mock.Of<IAuthorizedApiClient>();
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var pageModel = new LookupByIdModel(new NullLogger<LookupByIdModel>(), mockApiClient, mockClaimsProvider);

            // act
            pageModel.OnGet();

            // assert
            Assert.Equal("NAC Query Tool", pageModel.Title);
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public async void LookupSetsRecord()
        {
            // arrange
            var returnValue = @"{
                ""data"": {
                    ""first"": ""Theodore"",
                    ""middle"": ""Carri"",
                    ""last"": ""Farrington"",
                    ""ssn"": ""000-00-0000"",
                    ""dob"": ""2021-01-01""
                }
            }";
            var mockClient = clientMock(HttpStatusCode.OK, returnValue);
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var pageModel = new LookupByIdModel(new NullLogger<LookupByIdModel>(), mockClient, mockClaimsProvider);
            pageModel.Query = new Lookup { LookupId = "BCD2345" };

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.IsType<LookupResponse>(pageModel.Record);
            Assert.NotNull(pageModel.Record);
            Assert.NotNull(pageModel.Record.data);
            Assert.False(pageModel.NoResults);
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public async void LookupNoResults()
        {
            // arrange
            var returnValue = @"{
                ""data"": null
            }";
            var mockClient = clientMock(HttpStatusCode.OK, returnValue);
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var pageModel = new LookupByIdModel(new NullLogger<LookupByIdModel>(), mockClient, mockClaimsProvider);
            pageModel.Query = new Lookup { LookupId = "BCD2345" };

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.IsType<LookupResponse>(pageModel.Record);
            Assert.Null(pageModel.Record.data);
            Assert.True(pageModel.NoResults);
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public async void LookupCapturesApiError()
        {
            // arrange
            var mockClient = clientMock(HttpStatusCode.BadRequest, "");
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var pageModel = new LookupByIdModel(new NullLogger<LookupByIdModel>(), mockClient, mockClaimsProvider);
            pageModel.Query = new Lookup { LookupId = "BCD2345" };

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.NotNull(pageModel.RequestError);
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }
    }
}
