// -----------------------------------------------------------------------
//  <copyright file="Startup.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2023 Petabridge, LLC <https://petabridge.com>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

using LogLevel = Akka.Event.LogLevel;

namespace Akka.ShoppingCart;

public class Startup
{
    private readonly WebHostBuilderContext _context;
    
    public Startup(WebHostBuilderContext context)
    {
        _context = context;
    }
    
    public void ConfigureServices(IServiceCollection services)
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
                });

            if (_context.HostingEnvironment.IsDevelopment())
            {
                // For local development, we will be using Akka.Discovery.ConfigServiceDiscovery instead of Akka.Discovery.Azure
                // We will also use in memory persistence providers instead of using Akka.Persistence.Azure
                builder
                    .WithAkkaManagement(new AkkaManagementOptions
                    {
                        HostName = "localhost",
                        BindHostName = "localhost",
                        Port = 18558,
                        BindPort = 18558
                    })
                    .WithClusterBootstrap(setup =>
                    {
                        setup.ContactPointDiscovery.ServiceName = nameof(ShoppingCartService);
                        setup.ContactPointDiscovery.RequiredContactPointsNr = 1;
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
            }
            else
            {
                var endpointAddress = _context.Configuration["WEBSITE_PRIVATE_IP"];
                if (string.IsNullOrWhiteSpace(endpointAddress))
                    throw new Exception("Missing WEBSITE_PRIVATE_IP environment variable");

                var strPort = _context.Configuration["WEBSITE_PRIVATE_PORTS"];
                if (string.IsNullOrWhiteSpace(strPort))
                    throw new Exception("Missing WEBSITE_PRIVATE_PORTS environment variable");
                
                var strPorts = strPort.Split(',');
                if (strPorts.Length < 2)
                    throw new Exception("Insufficient private ports configured.");
                var (remotePort, managementPort) = (int.Parse(strPorts[0]), int.Parse(strPorts[1]));
                
                var connectionString = _context.Configuration["AZURE_STORAGE_CONNECTION_STRING"];
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new Exception("Missing AZURE_STORAGE_CONNECTION_STRING environment variable");
                
                builder
                    .WithAkkaManagement(new AkkaManagementOptions
                    {
                        HostName = endpointAddress,
                        BindHostName = endpointAddress,
                        Port = managementPort,
                        BindPort = managementPort
                    })
                    .WithClusterBootstrap(setup =>
                    {
                        setup.ContactPointDiscovery.ServiceName = nameof(ShoppingCartService);
                        setup.ContactPointDiscovery.RequiredContactPointsNr = 1;
                    })
                    .WithAzureDiscovery(setup =>
                    {
                        setup.HostName = endpointAddress;
                        setup.Port = managementPort;
                        setup.ServiceName = nameof(ShoppingCartService);
                        setup.ConnectionString = connectionString;
                    })
                    .WithClustering()
                    .WithRemoting(endpointAddress, remotePort)
                    .WithAzureTableJournal(connectionString)
                    .WithAzureBlobsSnapshotStore(connectionString);
            }
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}