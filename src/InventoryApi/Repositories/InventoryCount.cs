using System;

namespace InventoryApi.Repositories;

public class InventoryCount
{
    public string Id { get; private set; }
    public int StoreId { get; set; }
    public int CategoryId { get; set; }
    public string PartitionKey { get => $"{StoreId}_{CategoryId}"; }
    public int ProductId { get; set; }
    public decimal Count { get; set; }
    public string? Units { get; set; }
    public DateOnly InventoryDate { get; set; }
    public DateTime LastUpdated { get; set; }

    public InventoryCount()
    {
        SetId();
        LastUpdated = DateTime.Now;
    }
    
    public void SetId()
    {
        // Create a deterministic id based on the natural key fields
        Id = $"{StoreId}_{CategoryId}_{ProductId}_{InventoryDate:yyyy-MM-dd}";
    }
}