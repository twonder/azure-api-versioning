using System.ComponentModel.DataAnnotations;

namespace InventoryApi.Models;

public class AddInventoryCountRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "StoreId must be greater than 0")]
    public int StoreId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0")]
    public int CategoryId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0")]
    public int ProductId { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Count cannot be negative")]
    public decimal Count { get; set; }

    public string? Units { get; set; }

    [Required]
    public DateOnly InventoryDate { get; set; }
}