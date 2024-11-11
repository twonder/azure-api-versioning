using System.Text.Json.Serialization;
using InventoryApi.Repositories;

namespace InventoryApi.Models;

public class InventoryCountResponse
{
    [JsonPropertyName("storeId")]
    public int StoreId { get; set; }
    
    [JsonPropertyName("categoryId")]
    public int CategoryId { get; set; }
    
    [JsonPropertyName("productId")]
    public int ProductId { get; set; }
    
    [JsonPropertyName("count")]
    public decimal Count { get; set; }
    
    [JsonPropertyName("units")]
    public string? Units { get; set; }
    
    [JsonPropertyName("inventoryDate")]
    public DateOnly InventoryDate { get; set; }
    
    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; }

    public static InventoryCountResponse FromInventoryCount(InventoryCount count)
    {
        return new InventoryCountResponse
        {
            StoreId = count.StoreId,
            CategoryId = count.CategoryId,
            ProductId = count.ProductId,
            Count = count.Count,
            Units = count.Units,
            InventoryDate = count.InventoryDate,
            LastUpdated = count.LastUpdated
        };
    }
}