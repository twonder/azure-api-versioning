using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace InventoryApi;

public static class InventoryFunctions
{
    [FunctionName("GetItems")]
    public static IActionResult GetItems(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "items")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Getting all items");
        // TODO: Implement actual inventory retrieval logic
        var items = new[] { new { Id = 1, Name = "Item 1" }, new { Id = 2, Name = "Item 2" } };
        return new OkObjectResult(items);
    }

    [FunctionName("GetItem")]
    public static IActionResult GetItem(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "items/{id}")] HttpRequest req,
        int id,
        ILogger log)
    {
        log.LogInformation("Getting item with id: {Id}", id);
        // TODO: Implement actual item retrieval logic
        var item = new { Id = id, Name = $"Item {id}" };
        return new OkObjectResult(item);
    }

    [FunctionName("AddItem")]
    public static async Task<IActionResult> AddItem(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "items")] HttpRequest req,
        ILogger log)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<ItemInput>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        // TODO: Implement actual item addition logic
        log.LogInformation("Adding new item: {Name}", data?.Name);
        return new OkObjectResult(new { Id = 3, Name = data?.Name });
    }
}

public record ItemInput(string? Name);