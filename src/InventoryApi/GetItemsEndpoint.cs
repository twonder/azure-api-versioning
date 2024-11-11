using System.Net;
using InventoryApi.Models;
using InventoryApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace InventoryApi;

public class GetItemsEndpoint
{
    private readonly IInventoryRepository _repository;
    private readonly ILogger<GetItemsEndpoint> _logger;
    
    public GetItemsEndpoint(
        IInventoryRepository repository,
        ILogger<GetItemsEndpoint> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    [Function("GetItems")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "counts")] HttpRequestData req)
    {
        // Get query parameters
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
    
        // Validate and parse parameters
        if (!int.TryParse(query["storeId"], out int storeId) || storeId <= 0)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { error = "Invalid or missing storeId" });
            return badRequest;
        }

        if (!int.TryParse(query["categoryId"], out int categoryId) || categoryId <= 0)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { error = "Invalid or missing categoryId" });
            return badRequest;
        }

        // Parse date parameter with fallback to current date if not provided
        var inventoryDateStr = query["inventoryDate"];
        DateOnly inventoryDate;
        if (!DateOnly.TryParse(inventoryDateStr, out inventoryDate))
        {
            inventoryDate = DateOnly.FromDateTime(DateTime.Now);
        }

        try
        {
            var counts = await _repository.GetInventoryItemsAsync(storeId, categoryId, inventoryDate);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(counts.Select(c => InventoryCountResponse.FromInventoryCount(c)));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory items. StoreId: {StoreId}, CategoryId: {CategoryId}, Date: {Date}", 
                storeId, categoryId, inventoryDate);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "An error occurred processing your request" });
            return errorResponse;
        }
    }
    
    // [Function("GetItems")]
    // public async Task<HttpResponseData> Run(
    //     [HttpTrigger(AuthorizationLevel.Function, "get", Route = "counts")] HttpRequestData req)
    // {
    //     var counts = await _repository.GetInventoryItemsAsync(1, 1, DateOnly.FromDateTime(DateTime.Now));
    //     
    //     var response = req.CreateResponse(HttpStatusCode.OK);
    //     await response.WriteAsJsonAsync(counts);
    //
    //     return response;
    // }
}