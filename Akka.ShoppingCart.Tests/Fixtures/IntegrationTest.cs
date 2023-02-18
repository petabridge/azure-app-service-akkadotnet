

using Akka.ShoppingCart.Tests.Fixtures;

namespace Akka.ShoppingCart.Tests.Fixtures
{
    public abstract class IntegrationTest : IClassFixture<WebApplicationFactoryFixture>
    {
        protected readonly WebApplicationFactoryFixture _factory;
        //protected readonly HttpClient _client;

        public IntegrationTest(WebApplicationFactoryFixture fixture)
        {
            _factory = fixture;
            //_client = _factory.CreateClient();

            // if needed, reset the DB
            //_checkpoint.Reset(_factory.Configuration.GetConnectionString("SQL")).Wait();
        }
    }
}
