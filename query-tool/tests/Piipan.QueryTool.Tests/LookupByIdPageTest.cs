using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.QueryTool.Pages;
using Piipan.Shared.Authentication;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class LookupByIdPageTests
    {
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
    }
}
