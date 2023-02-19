using Akka.ShoppingCart.Abstraction;
using FluentAssertions;
using Xunit.Abstractions;

namespace Akka.ShoppingCart.Tests
{
    public class BlazorTest : IntegrationTest
    {
        private ITestOutputHelper _output;
        public BlazorTest(ITestOutputHelper output, ApiWebApplicationFactory fixture)
            : base(fixture) 
        {
            _output = output;
        }

        [Fact]
        public async Task Test_Products()
        {
            var o = await _client.GetAsync("api/product/id");
            var s = await o.Content.ReadAsStringAsync();
            _output.WriteLine(s);
            o.Content.Should().NotBeNull();
            Assert.True(true);
        }
        [Fact]
        public async Task Test_Shop()
        {
            var o = await _client.GetAsync("/shop");
            var s = await o.Content.ReadAsStringAsync();
            _output.WriteLine(s);
            o.Content.Should().NotBeNull();
            Assert.True(true);
        }
    }
}
