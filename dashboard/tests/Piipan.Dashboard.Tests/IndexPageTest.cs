using Xunit;
using Piipan.Dashboard.Pages;
using Microsoft.Extensions.Logging.Abstractions;

namespace Piipan.Dashboard.Tests
{
    public class IndexPageTests
    {
        [Fact]
        public void BeforeOnGet_MessageIsCorrect()
        {
            // arrange
            var pageModel = new IndexModel(new NullLogger<IndexModel>());

            // act

            // assert
            Assert.Equal("Hello", pageModel.Message);
        }

        [Fact]
        public void AfterOnGet_MessageIsCorrect()
        {
            // arrange
            var pageModel = new IndexModel(new NullLogger<IndexModel>());

            // act
            pageModel.OnGet();

            // assert
            Assert.Equal("Hello, world.", pageModel.Message);
        }
    }
}
