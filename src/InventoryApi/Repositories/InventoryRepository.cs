using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace InventoryApi.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;
    private readonly ILogger<IInventoryRepository> _logger;

    public InventoryRepository(
        CosmosClient cosmosClient,
        ILogger<IInventoryRepository> logger,
        string databaseName,
        string containerName)
    {
        _cosmosClient = cosmosClient;
        _container = _cosmosClient.GetContainer(databaseName, containerName);
        _logger = logger;
    }

    public async Task<IEnumerable<InventoryCount>> GetInventoryItemsAsync(int storeId, int categoryId, DateOnly date)
    {
        try
        {
            var partitionKey = $"{storeId}_{categoryId}";
            var queryText = @"
                SELECT * FROM c 
                WHERE c.partitionKey = @partitionKey 
                AND c.inventoryDate = @date";

            var queryDefinition = new QueryDefinition(queryText)
                .WithParameter("@partitionKey", partitionKey)
                .WithParameter("@date", date.ToString("yyyy-MM-dd"));

            var results = new List<InventoryCount>();
            var queryResultSetIterator = _container.GetItemQueryIterator<InventoryCount>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                var response = await queryResultSetIterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            _logger.LogInformation("Found {Count} inventory items", results.Count);
            return results;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Log that no items were found but don't treat it as an error
            _logger.LogInformation(
                "No inventory items found for Store: {StoreId}, Category: {CategoryId}, Date: {Date}", 
                storeId, categoryId, date);
            return Enumerable.Empty<InventoryCount>();
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error querying inventory items from Cosmos DB. Store: {StoreId}, Category: {CategoryId}, Date: {Date}", 
                storeId, categoryId, date);
            throw new RepositoryException("Error retrieving inventory items", ex);
        }
    }

    public async Task<InventoryCount> SaveInventoryCountAsync(InventoryCount count)
    {
        try
        {
           _logger.LogInformation("Saving inventory count for Store: {StoreId}, Category: {CategoryId}, ProductId: {ProductId}, InventoryDate: {InventoryDate}", 
                count.StoreId, count.CategoryId, count.ProductId, count.InventoryDate);
            
            var existingItem = await GetExistingInventoryCountAsync(count.PartitionKey, count.ProductId, count.InventoryDate);
           count.LastUpdated = DateTime.UtcNow; 
            if (existingItem != null)
            {
                // Update existing record
                var response = await _container.ReplaceItemAsync(
                    count,
                    count.Id,
                    new PartitionKey(count.PartitionKey)
                );

                _logger.LogInformation("Updated existing inventory count for date");
                return response.Resource;
            }
            else
            {
                _logger.LogInformation("Creating item {Count}", 
                    JsonSerializer.Serialize(count, new JsonSerializerOptions 
                    { 
                        WriteIndented = true 
                    }));
                
                count.SetId();
                
                // Create new record
                var response = await _container.CreateItemAsync(
                    count,
                    new PartitionKey(count.PartitionKey)
                );

                _logger.LogInformation("Created new inventory count for date");
                return response.Resource;
            }
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error saving inventory count to Cosmos DB. Store: {StoreId}, Category: {CategoryId}, Date: {Date}. Error: {ErrorMessage}. StackTrace: {StackTrace}", 
                count.StoreId, 
                count.CategoryId, 
                count.InventoryDate,
                ex.Message,
                ex.StackTrace);
            throw new RepositoryException("Error saving inventory count", ex);
        }
    }
    
    private async Task<InventoryCount?> GetExistingInventoryCountAsync(
        string partitionKey,
        int productId,
        DateOnly inventoryDate)
    {
        // Format date for consistency
        var formattedDate = inventoryDate.ToString("yyyy-MM-dd");
        
        var queryText = @"
        SELECT * FROM c 
        WHERE c.partitionKey = @partitionKey 
            AND c.productId = @productId 
            AND c.inventoryDate = @inventoryDate";
        
        try
        {
            var queryResultSetIterator = _container.GetItemQueryIterator<InventoryCount>(
                queryDefinition: new QueryDefinition(queryText)
                    .WithParameter("@partitionKey", partitionKey)
                    .WithParameter("@productId", productId)
                    .WithParameter("@inventoryDate", formattedDate),
                requestOptions: new QueryRequestOptions 
                { 
                    PartitionKey = new PartitionKey(partitionKey),
                    MaxItemCount = 1
                });

            var response = await queryResultSetIterator.ReadNextAsync();
            return response?.FirstOrDefault();
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogInformation("No existing inventory count found for partition {PartitionKey}, product {ProductId}, date {InventoryDate}", 
                partitionKey, productId, inventoryDate);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying for inventory count: partition {PartitionKey}, product {ProductId}, date {InventoryDate}", 
                partitionKey, productId, inventoryDate);
            throw;
        }
    }
}

public class RepositoryException : Exception
{
    public RepositoryException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}