namespace ECommerceStore.Models;

public class ProductSalesSummary
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalUnitsSold { get; set; }
    public decimal TotalRevenue { get; set; }
}