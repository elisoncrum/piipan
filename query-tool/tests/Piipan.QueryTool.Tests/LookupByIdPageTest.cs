using System;
using System.Net;
using System.Net.Http;
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

        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange
            var mockApiClient = Mock.Of<IAuthorizedApiClient>();
            var pageModel = new LookupByIdModel(
                new NullLogger<LookupByIdModel>(),
                mockApiClient
                );
            // act
            // assert
            Assert.Equal("", pageModel.Title);
        }

        [Fact]
        public void TestAfterOnGet()
        {
            // arrange
            var mockApiClient = Mock.Of<IAuthorizedApiClient>();
            var pageModel = new LookupByIdModel(new NullLogger<LookupByIdModel>(), mockApiClient);

            // act
            pageModel.OnGet();

            // assert
            Assert.Equal("NAC Query Tool", pageModel.Title);
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
            var pageModel = new LookupByIdModel(new NullLogger<LookupByIdModel>(), mockClient);
            pageModel.Query = new Lookup { LookupId = "BCD2345" };

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.IsType<LookupResponse>(pageModel.Record);
            Assert.NotNull(pageModel.Record);
            Assert.NotNull(pageModel.Record.data);
            Assert.False(pageModel.NoResults);
        }

        [Fact]
        public async void LookupNoResults()
        {
            // arrange
            var returnValue = @"{
                ""data"": null
            }";
            var mockClient = clientMock(HttpStatusCode.OK, returnValue);
            var pageModel = new LookupByIdModel(new NullLogger<LookupByIdModel>(), mockClient);
            pageModel.Query = new Lookup { LookupId = "BCD2345" };

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.IsType<LookupResponse>(pageModel.Record);
            Assert.Null(pageModel.Record.data);
            Assert.True(pageModel.NoResults);
        }

        [Fact]
        public async void LookupCapturesApiError()
        {
            // arrange
            var mockClient = clientMock(HttpStatusCode.BadRequest, "");
            var pageModel = new LookupByIdModel(new NullLogger<LookupByIdModel>(), mockClient);
            pageModel.Query = new Lookup { LookupId = "BCD2345" };

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.NotNull(pageModel.RequestError);
        }
    }
}
