using System.Text.Json;
using InventoryApi.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.CosmosDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ChangesFeedFunction.cs
public class CosmosTrigger
{
    private readonly ILogger<CosmosTrigger> _logger;

    public CosmosTrigger(
        ILogger<CosmosTrigger> logger)
    {
        _logger = logger;
    }

    [Function("CosmosDBTrigger")]
    public async Task Run(
        [CosmosDBTrigger(
            databaseName: "%CosmosDb:DatabaseName%",
            containerName: "%CosmosDb:ContainerName%",
            Connection = "CosmosDb",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)]
        IReadOnlyList<Dictionary<string, object>> input)
    {
        foreach (var item in input)
        {
            try
            {
                // Convert to your model
                var json = JsonSerializer.Serialize(item);
                // var yourModel = JsonSerializer.Deserialize<InventoryCount>(json);
                
                _logger.LogInformation("Processing document: {json}", json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing document");
            }
        }
    }
}