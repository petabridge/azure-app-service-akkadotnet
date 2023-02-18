using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Akka.Hosting;
using Akka.ShoppingCart.Services;
using MudBlazor.Services;
using Akka.ShoppingCart.Actors;
using Akka.Management;
using Akka.Actor;
using Akka.Management.Cluster.Bootstrap;
using Akka.ShoppingCart.Extensions;
using Akka.Remote.Hosting;
using Akka.Cluster.Hosting;
using Akka.Persistence.Hosting;
using Akka.ShoppingCart.Abstraction;
using Akka.ShoppingCart.Messages;

namespace Akka.ShoppingCart.Tests.Fixtures
{
    public class WebApplicationFactoryFixture : WebApplicationFactory<Startup>
    {
        private IHost? _host;

        public string ServerAddress
        {
            get
            {
                EnsureServer();
                return ClientOptions.BaseAddress.ToString();
            }
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseUrls("https://localhost:7048");
        }
        protected override IHost CreateHost(IHostBuilder builder)
        {
            
            var testHost = builder.Build();

            
            builder.ConfigureWebHost(webHostBuilder =>
            {      
                webHostBuilder.ConfigureServices(services => 
                {
                    services.AddMudServices();
                    services.AddRazorPages();
                    services.AddServerSideBlazor();
                    services.AddHttpContextAccessor();
                    services.AddSingleton<ShoppingCartService>();
                    services.AddSingleton<InventoryService>();
                    services.AddSingleton<ProductService>();
                    services.AddScoped<ComponentStateChangedObserver>();
                    services.AddSingleton<ToastService>();
                    services.AddSingleton<ClusterStateBroadcastService>();
                    services.AddLocalStorageServices();
                    // Uncomment to enable ApplicationInsight logging
                    // services.AddApplicationInsights("Node");
                    services.AddAkka("ShoppingCart", builder =>
                    {
                        builder
                            // Uncomment to enable ApplicationInsight logging
                            /*
                            .ConfigureLoggers(logger =>
                            {
                                logger.LogLevel = LogLevel.WarningLevel;
                                logger.ClearLoggers();
                                logger.AddLoggerFactory();
                            })
                            */

                            // See HostingExtensions.AddShoppingCartRegions() to see how the shard regions are set-up
                            .AddShoppingCartRegions()
                            .WithActors((system, registry) =>
                            {
                                var listener = system.ActorOf(Props.Create(() => new ClusterListenerActor()));
                                registry.TryRegister<RegistryKey.ClusterStateListener>(listener);
                            })
                            .WithAkkaManagement(setup =>
                            {
                                setup.Http.Hostname = "localhost";
                                setup.Http.BindHostname = "localhost";
                                setup.Http.Port = 18558;
                                setup.Http.BindPort = 18558;
                            })
                            .WithClusterBootstrap(setup =>
                            {
                                setup.ContactPointDiscovery = new ContactPointDiscoverySetup
                                {
                                    ServiceName = nameof(ShoppingCartService),
                                    RequiredContactPointsNr = 1
                                };
                            })
                                .WithConfigDiscovery(new Dictionary<string, List<string>>
                                {
                                    [nameof(ShoppingCartService)] = new() { "localhost:18558" }
                                })
                                .WithRemoting("localhost", 12552)
                                .WithClustering()
                                .WithInMemoryJournal()
                                .WithInMemorySnapshotStore()
                                .WithActors(async (_, registry) =>
                                {
                                    var productRegion = registry.Get<RegistryKey.ProductRegion>();
                                    var faker = new ProductDetails().GetBogusFaker();

                                    foreach (var product in faker.GenerateLazy(50))
                                    {
                                        await productRegion.Ask<Done>(new Product.CreateOrUpdate(product));
                                    }
                                });

                    });
                });
                webHostBuilder.UseKestrel();
            });

            
            _host = builder.Build();
            _host.Start();

            // Extract the selected dynamic port out of the Kestrel server
            // and assign it onto the client options for convenience so it
            // "just works" as otherwise it'll be the default http://localhost
            // URL, which won't route to the Kestrel-hosted HTTP server.
            var server = _host.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>();

            ClientOptions.BaseAddress = addresses!.Addresses
                .Select(x => new Uri(x))
                .Last();

            testHost.Start();
            return testHost;
        }

        protected override void Dispose(bool disposing)
        {
            _host?.Dispose();
        }

        private void EnsureServer()
        {
            if (_host is null)
            {
                // This forces WebApplicationFactory to bootstrap the server
                using var _ = CreateDefaultClient();
            }
        }
    }
}

