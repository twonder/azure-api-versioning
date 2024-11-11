using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryApi.Repositories;

public interface IInventoryRepository
{
    Task<IEnumerable<InventoryCount>> GetInventoryItemsAsync(int storeId, int categoryId, DateOnly date);
    Task<InventoryCount> SaveInventoryCountAsync(InventoryCount count);
}