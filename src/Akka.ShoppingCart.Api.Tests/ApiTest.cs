using Akka.ShoppingCart.Abstraction;
using FluentAssertions;
using System.Text.Json;
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
            var p = JsonSerializer.Deserialize<object>(s);  
            _output.WriteLine(JsonSerializer.Serialize(p, new JsonSerializerOptions { WriteIndented = true }));
            o.Content.Should().NotBeNull();
            Assert.True(true);
        }
        
    }
}
