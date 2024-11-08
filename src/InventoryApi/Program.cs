using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Cosmos;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WorkerService;
using System.Text.Json;
using System.Text.Json.Serialization;
using InventoryApi.Repositories;  // Your repository namespace
using Microsoft.Azure.Functions.Worker.Extensions.Http;
using Microsoft.Extensions.Logging;


var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder =>
    {
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context,services) =>
    {
        // Configuration setup
        var configuration = context.Configuration;
        
        // Add Application Insights
        services.AddApplicationInsightsTelemetryWorkerService(options =>
        {
            options.ConnectionString = context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        });

        // Register CosmosClient
        services.AddSingleton(sp =>
        {
            var cosmosEndpoint = configuration.GetSection("Values")["CosmosDb:EndpointUrl"];
            var cosmosKey = configuration.GetSection("Values")["CosmosDb:PrimaryKey"];

            if (string.IsNullOrEmpty(cosmosEndpoint) || string.IsNullOrEmpty(cosmosKey))
            {
                throw new InvalidOperationException("Cosmos DB connection settings are missing in configuration.");
            }
            
            return new CosmosClient(
                cosmosEndpoint,
                cosmosKey,
                new CosmosClientOptions
                {
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    }
                }
            );
        });

        // Register CosmosDB Service
        services.AddSingleton<IInventoryRepository>(sp =>
        {
            var cosmosClient = sp.GetRequiredService<CosmosClient>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<InventoryRepository>();
            
            var databaseName = configuration.GetSection("Values")["CosmosDb:DatabaseName"];
            var containerName = configuration.GetSection("Values")["CosmosDb:ContainerName"];
            
            return new InventoryRepository(
                cosmosClient,
                logger,
                databaseName,
                containerName
            );
        });
    })
    .Build();

await host.RunAsync();