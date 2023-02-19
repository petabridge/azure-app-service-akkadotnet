using Akka.ShoppingCart.Abstraction;
using Akka.ShoppingCart.Model;
using Akka.ShoppingCart.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Akka.ShoppingCart.Tests
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
                services.AddSingleton<BlazorService>();
                services.AddHostedService<BlazorService>(sp => sp.GetRequiredService<BlazorService>());
            });
        }
    }
}
