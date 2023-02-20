// -----------------------------------------------------------------------
//  <copyright file="Startup.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2022 Petabridge, LLC <https://petabridge.com>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

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
        var akkaConfig = _context.Configuration.GetRequiredSection(nameof(AkkaClusterConfig))
            .Get<AkkaClusterConfig>();
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
                // Add Akka.HealthCheck
                .WithHealthCheck(options =>
                {
                    // Here we're adding all of the built-in providers
                    options.AddProviders(HealthCheckType.All);
                    options.Liveness.Transport = HealthCheckTransport.Tcp;
                    options.Liveness.TcpPort = 15000;
                    options.Readiness.Transport = HealthCheckTransport.Tcp;
                    options.Readiness.TcpPort = 15001;
                })
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
                    .WithClustering(new ClusterOptions()
                    {
                        Roles = akkaConfig.Roles,
                        SeedNodes = akkaConfig.SeedNodes
                    })
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
                var strPorts = _context.Configuration["WEBSITE_PRIVATE_PORTS"].Split(',');
                if (strPorts.Length < 2)
                    throw new Exception("Insufficient private ports configured.");
                var (remotePort, managementPort) = (int.Parse(strPorts[0]), int.Parse(strPorts[1]));
                var connectionString = _context.Configuration["AZURE_STORAGE_CONNECTION_STRING"];
                
                builder
                    .WithAkkaManagement(setup =>
                    {
                        setup.Http.Hostname = endpointAddress;
                        setup.Http.BindHostname = endpointAddress;
                        setup.Http.Port = managementPort;
                        setup.Http.BindPort = managementPort;
                    })
                    .WithClusterBootstrap(setup =>
                    {
                        setup.ContactPointDiscovery = new ContactPointDiscoverySetup
                        {
                            ServiceName = nameof(ShoppingCartService),
                            RequiredContactPointsNr = 1
                        };
                    })
                    .WithAzureDiscovery(setup =>
                    {
                        setup.HostName = endpointAddress;
                        setup.Port = managementPort;
                        setup.ServiceName = nameof(ShoppingCartService);
                        setup.ConnectionString = connectionString;
                    })
                    .WithClustering(new ClusterOptions()
                    {
                        Roles = akkaConfig.Roles,
                        SeedNodes = akkaConfig.SeedNodes
                    })
                    .WithRemoting(endpointAddress, remotePort)
                    .WithAzureTableJournal(connectionString)
                    .WithAzureBlobsSnapshotStore(connectionString);
            }
        });
        services.AddSingleton<BlazorService>();
        services.AddHostedService<BlazorService>(sp => sp.GetRequiredService<BlazorService>());
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