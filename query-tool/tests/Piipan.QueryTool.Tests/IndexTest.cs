using Xunit;
using Moq;
using Piipan.QueryTool.Pages;
using Microsoft.Extensions.Logging.Abstractions;

namespace Piipan.QueryTool.Tests
{
    public class IndexPageTests
    {
        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange
            var pageModel = new IndexModel(new NullLogger<IndexModel>());
            // act
            // assert
            Assert.Equal("", pageModel.Title);
        }
        [Fact]
        public void TestAfterOnGet()
        {
            // arrange
            var pageModel = new IndexModel(new NullLogger<IndexModel>());

            // act
            pageModel.OnGet();

            // assert
            Assert.Equal("NAC Query Tool", pageModel.Title);
        }
        [Fact]
        public async void TestAfterOnPostAsync()
        {
            // arrange
            var pageModel = new IndexModel(new NullLogger<IndexModel>());
            var resp = new Mock<OrchestratorApiResponse>();
            resp.SetupGet(x => x.text).Returns("You did a request");
            var query = new PiiRecord();
            query.FirstName = "Fred";
            query.LastName = "Bloggs";

            // act
            await pageModel.OnPostAsync(query);

            // assert
            Assert.Equal("NAC Query Results", pageModel.Title);
            Assert.Equal("You did a request", pageModel.QueryResult);
        }
    }
}
