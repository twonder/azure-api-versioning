using System.ComponentModel.DataAnnotations;

namespace InventoryApi.Models;

public class GetInventoryCountRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "StoreId must be greater than 0")]
    public int StoreId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0")]
    public int CategoryId { get; set; }

    [Required]
    public DateOnly InventoryDate { get; set; }
}