using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using InventoryApi.Models;
using InventoryApi.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace InventoryApi;

public class AddItemEndpoint
{
    private readonly IInventoryRepository _repository;
    private readonly ILogger<GetItemsEndpoint> _logger;
    
    public AddItemEndpoint(
        IInventoryRepository repository,
        ILogger<GetItemsEndpoint> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    [Function("AddItem")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "counts")]
        HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Processing inventory count submission");

            // Read and deserialize the request
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var addRequest = JsonSerializer.Deserialize<AddInventoryCountRequest>(requestBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (addRequest == null)
            {
                _logger.LogWarning("Invalid request body - deserialization returned null");
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new
                {
                    error = "Invalid request body"
                });
                return badRequestResponse;
            }

            // Validate the request
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(addRequest, new ValidationContext(addRequest), validationResults, true))
            {
                _logger.LogWarning("Validation failed for inventory count request");
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new
                {
                    errors = validationResults.Select(x => x.ErrorMessage)
                });
                return badRequestResponse;
            }

            // Additional validation for future dates
            if (addRequest.InventoryDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                _logger.LogWarning("Attempted to submit inventory count for past date");
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new
                {
                    error = "Cannot submit inventory count for past date"
                });
                return badRequestResponse;
            }

            // Map request to domain model
            var inventoryCount = new InventoryCount
            {
                StoreId = addRequest.StoreId,
                CategoryId = addRequest.CategoryId,
                ProductId = addRequest.ProductId,
                Count = addRequest.Count,
                Units = addRequest.Units,
                InventoryDate = addRequest.InventoryDate
            };

            // Save to repository
            var savedCount = await _repository.SaveInventoryCountAsync(inventoryCount);

            // Create success response
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new
            {
                storeId = savedCount.StoreId,
                categoryId = savedCount.CategoryId,
                productId = savedCount.ProductId,
                count = savedCount.Count,
                units = savedCount.Units,
                inventoryDate = savedCount.InventoryDate,
                lastUpdated = savedCount.LastUpdated
            });

            _logger.LogInformation("Successfully saved inventory count for Store {StoreId}, Category {CategoryId}, Product {ProductId}, Date {InventoryDate}", 
                savedCount.StoreId, savedCount.CategoryId, savedCount.ProductId, savedCount.InventoryDate);

            return response;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing request body");
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new
            {
                error = "Invalid request format"
            });
            return response;
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error when saving inventory count");
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new
            {
                errors = ex.Message
            });
            return response;
        }
        catch (RepositoryException ex)
        {
            _logger.LogError("Error saving inventory count {ex}", ex);

            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new
            {
                error = "Error saving inventory count"
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing inventory count");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new
            {
                error = "An unexpected error occurred"
            });
            return response;
        }
    }
}