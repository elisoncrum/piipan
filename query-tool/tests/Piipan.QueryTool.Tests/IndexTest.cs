using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.QueryTool.Pages;
using Piipan.Shared.Authentication;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class IndexPageTests
    {
        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange
            var mockApiClient = Mock.Of<IAuthorizedApiClient>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
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
            var pageModel = new IndexModel(new NullLogger<IndexModel>(), mockApiClient);

            // act
            pageModel.OnGet();

            // assert
            Assert.Equal("NAC Query Tool", pageModel.Title);
        }
    }
}
