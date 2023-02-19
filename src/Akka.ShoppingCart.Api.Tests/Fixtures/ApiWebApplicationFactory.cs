
using Akka.ShoppingCart.Api.Model;
using Akka.ShoppingCart.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Akka.ShoppingCart.Api.Tests
{
    public class ApiWebApplicationFactory : WebApplicationFactory<PostModel>
    {
        public IConfiguration Configuration { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                Configuration = new ConfigurationBuilder()
                  //.AddJsonFile("integrationsettings.json")
                  .Build();

                config.AddConfiguration(Configuration);
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<ApiService>();
                services.AddHostedService<ApiService>(sp => sp.GetRequiredService<ApiService>());
            });
        }
    }
}
