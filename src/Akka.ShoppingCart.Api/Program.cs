using Akka;
using Akka.HealthCheck.Hosting;
using Akka.ShoppingCart.Api;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var _context = builder;
// Add services to the container.
var akkaConfig = _context.Configuration.GetRequiredSection(nameof(AkkaClusterConfig))
            .Get<AkkaClusterConfig>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Akka Azure", Version = "v1" });
});
builder.Services.AddAkka("ShoppingCart", builder =>
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

    if (_context.Environment.IsDevelopment())
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
builder.Services.AddSingleton<ApiService>();
builder.Services.AddHostedService<ApiService>(sp => sp.GetRequiredService<ApiService>());
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Akka.Api v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
