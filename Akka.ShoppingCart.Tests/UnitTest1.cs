using Akka.ShoppingCart.Tests.Fixtures;
using Microsoft.Playwright;

namespace Akka.ShoppingCart.Tests
{
    public class UnitTest1 : IntegrationTest
    {
        private readonly string _serverAddress;
        public UnitTest1(WebApplicationFactoryFixture fixture)
            : base(fixture) 
        {
            _serverAddress = fixture.ServerAddress;
            fixture.CreateDefaultClient();
        }

        [Fact]
        public async Task Test1()
        {
            //Arrange
            using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync();
            var page = await browser.NewPageAsync();
            //Act
            await page.GotoAsync(_serverAddress);
            await page.ClickAsync("[class='nav-link']");
            await page.ClickAsync("[class='btn btn-primary']");

            //Assert
            //await Assert.True(page.Locator("[role='status']")).ToHaveTextAsync("Current count: 1");
        }
    }
}