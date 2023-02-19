using Akka.ShoppingCart.Abstraction;
using FluentAssertions;

namespace Akka.ShoppingCart.Tests
{
    public class ApiTest : IntegrationTest
    {
        public ApiTest(ApiWebApplicationFactory fixture)
            : base(fixture) { }

        [Fact]
        public async Task TestPost()
        {
            var data = new ProductDetails();
            

            for(var i = 0; i < 100; i++)
            {
                data.Category = ProductCategory.Music;
                data.Description = $"{i}";
                data.Name = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                var o = await _client.PostAsync("/WriteApi/post", data.StringContent());
                o.Content.Should().NotBeNull();
            }
            
        }
        [Fact]
        public async Task Products()
        {            
                var o = await _client.GetAsync("/products");
                o.Content.Should().NotBeNull();            
        }
    }
}
