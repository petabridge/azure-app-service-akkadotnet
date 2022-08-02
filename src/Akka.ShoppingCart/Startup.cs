// -----------------------------------------------------------------------
//  <copyright file="Startup.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft
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
                var config = (Config)@$"
akka {{
    extensions = [""Akka.Management.Cluster.Bootstrap.ClusterBootstrapProvider, Akka.Management.Cluster.Bootstrap""]

    management {{
        http {{
            hostname = {endpointAddress}
            port = {managementPort}
        }}
        cluster.bootstrap {{
            contact-point-discovery {{
                discovery-method = akka.discovery
                service-name = {nameof(ShoppingCartService)}
                port-name = management
                required-contact-point-nr = 1
            }}
        }}
    }}

    discovery {{
        method = azure
        azure {{
            class = ""Akka.Discovery.Azure.AzureServiceDiscovery, Akka.Discovery.Azure""
            public-hostname = {endpointAddress}
            public-port = {managementPort}
            service-name = {nameof(ShoppingCartService)}
            connection-string = ""{connectionString}""
        }}
    }}
}}";
                
                builder
                    .AddHocon(config, HoconAddMode.Prepend)
                    .WithClustering()
                    .WithRemoting(endpointAddress, remotePort)
                    .WithAzurePersistence(connectionString);
            }
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseDeveloperExceptionPage();
        /*
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        */

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