// -----------------------------------------------------------------------
//  <copyright file="Startup.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
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
        services.AddMudServices();
        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddHttpContextAccessor();
        services.AddSingleton<ShoppingCartService>();
        services.AddSingleton<InventoryService>();
        services.AddSingleton<ProductService>();
        services.AddScoped<ComponentStateChangedObserver>();
        services.AddSingleton<ToastService>();
        services.AddLocalStorageServices();
        //services.AddApplicationInsights("Node");
        services.AddAkka("ShoppingCart", builder =>
        {
            builder.AddShoppingCartRegions();
            /*
            builder.ConfigureLoggers(log =>
                {
                    log.AddLoggerFactory();
                });
            */

            if (_context.HostingEnvironment.IsDevelopment())
            {
                builder
                    .WithInMemoryJournal()
                    .WithInMemorySnapshotStore()
                    .WithRemoting("localhost", 12552)
                    .WithClustering(new ClusterOptions{SeedNodes = new[]{new Address("akka.tcp", "ShoppingCart", "localhost", 12552)}})
                    .WithActors((_, registry) =>
                    {
                        var productRegion = registry.Get<RegistryKey.ProductRegion>();
                        var faker = new ProductDetails().GetBogusFaker();

                        foreach (var product in faker.GenerateLazy(50))
                        {
                            productRegion.Tell(new Product.CreateOrUpdate(product));
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
                        setup.Http.Port = managementPort;
                    })
                    .WithClusterBootstrap(setup =>
                    {
                        setup.ContactPointDiscovery = new ContactPointDiscoverySetup
                        {
                            ServiceName = nameof(ShoppingCartService),
                            RequiredContactPointsNr = 2
                        };
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