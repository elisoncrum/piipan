using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class PlaywrightTests : IClassFixture<WebServerFixture>
    {
        private readonly WebServerFixture fixture;

        public PlaywrightTests(WebServerFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task Index_ShowsErrorsOnEmptySubmit()
        {
            // launch playwright
            // using var playwright = await Playwright.CreateAsync();
            // await using var browser = await playwright.Chromium.LaunchAsync();
            var page = await fixture.browser.NewPageAsync();
            await page.GotoAsync(fixture.BaseUrl);

            // action
            var element = page.Locator("input[type='submit']");
            await element.ClickAsync();

            // assert
            var content = await page.InnerTextAsync(".usa-alert--error");
            Assert.Contains("The First name field is required", content);
            Assert.Contains("The Last name field is required", content);
            Assert.Contains("The Date of birth field is required", content);
            Assert.Contains("The SSN field is required", content);
        }

        // [Fact]
        // public async Task Index_ShowsEmptyMessageWhenNoMatches()
        // {

        //     // launch playwright
        //     // using var playwright = await Playwright.CreateAsync();
        //     // await using var browser = await playwright.Chromium.LaunchAsync();
        //     var page = await fixture.browser.NewPageAsync();
        //     await page.GotoAsync(fixture.BaseUrl);

        //     //setup
        //     // abort routes
        //     await page.RouteAsync(new Regex("find_matches"), async route => {
        //         await route.AbortAsync();
        //         // await route.FulfillAsync(new RouteFulfillOptions { Body = "{ \"data\": { \"matches\": []}}" });
        //     });

        //     await page.FillAsync("#Query_FirstName", "joe");
        //     await page.FillAsync("#Query_LastName", "schmo");
        //     await page.FillAsync("#Query_DateOfBirth", "1997-01-01");
        //     await page.FillAsync("#Query_SocialSecurityNum", "550-01-6981");

        //     // action
        //     var element = page.Locator("input[type='submit']");
        //     await element.ClickAsync();

        //     // assert
        //     var content = await page.InnerTextAsync("#main-content");
        //     Assert.Contains("No matches found", content);
        // }
    }
}
