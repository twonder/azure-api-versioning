using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace InventoryApi;

public class GetItemEndpoint
{
    [Function("GetItem")]
    public static IActionResult GetItem(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "counts/{id}")] HttpRequestData req,
        int id,
        ILogger log)
    {
        log.LogInformation("Getting item with id: {Id}", id);
        // TODO: Implement actual item retrieval logic
        var item = new { Id = id, Name = $"Item {id}" };
        return new OkObjectResult(item);
    }
}
