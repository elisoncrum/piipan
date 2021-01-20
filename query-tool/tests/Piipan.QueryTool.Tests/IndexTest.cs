using Xunit;
using query_tool.Pages;
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
  }
}
