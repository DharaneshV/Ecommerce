namespace ECommerceStore.Models;

public class OrderInvoice
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderedAt { get; set; } = DateTime.Now;
}