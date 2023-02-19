using Akka.ShoppingCart.Abstraction;
using FluentAssertions;
using Xunit.Abstractions;

namespace Akka.ShoppingCart.Api.Tests
{
    public class ApiTest : IntegrationTest
    {
        private ITestOutputHelper _output;
        public ApiTest(ITestOutputHelper output, ApiWebApplicationFactory fixture)
            : base(fixture) 
        {
            _output = output;
        }

        
        [Fact]
        public async Task Test_Inventory()
        {
            var o = await _client.GetAsync("api/Inventory");
            var s = await o.Content.ReadAsStringAsync();
            _output.WriteLine(s);
            o.Content.Should().NotBeNull();
            Assert.True(true);
        }
        
    }
}
