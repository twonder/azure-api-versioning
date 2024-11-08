using InventoryApi.Repositories;

namespace InventoryApi.Models;

public class InventoryCountResponse
{
    public string Id { get; set; } = string.Empty;
    public int StoreId { get; set; }
    public int CategoryId { get; set; }
    public decimal Count { get; set; }
    public string? Units { get; set; }
    public DateOnly InventoryDate { get; set; }
    public DateTime LastUpdated { get; set; }

    public static InventoryCountResponse FromInventoryCount(InventoryCount count)
    {
        return new InventoryCountResponse
        {
            StoreId = count.StoreId,
            CategoryId = count.CategoryId,
            Count = count.Count,
            Units = count.Units,
            InventoryDate = count.InventoryDate,
            LastUpdated = count.LastUpdated
        };
    }
}