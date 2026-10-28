using Microsoft.Data.SqlClient;
using ECommerceStore.Models;
using ECommerceStore.Repository;
using ECommerceStore.Services;

string connectionString =
    @"Server=.\SQLEXPRESS;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;";

var customerService = new CustomerService(new CustomerRepository(connectionString));
var productService = new ProductService(new ProductRepository(connectionString));
var reportService = new ReportService(new ReportRepository(connectionString));

while (true)
{
    Console.WriteLine("\n=== E-COMMERCE STORE MANAGEMENT ===");
    Console.WriteLine("1. Add Customer");
    Console.WriteLine("2. View All Customers");
    Console.WriteLine("3. Search Customer By ID");
    Console.WriteLine("4. Add Product");
    Console.WriteLine("5. View All Products");
    Console.WriteLine("6. Search Product By ID");
    Console.WriteLine("7. Place Order & Generate Bill");
    Console.WriteLine("8. View Sales Performance Report");
    Console.WriteLine("9. Exit");
    Console.Write("Enter your choice: ");

    string? choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
            {
                Console.Write("Customer name: ");
                string name = Console.ReadLine() ?? "";
                Console.Write("Email: ");
                string email = Console.ReadLine() ?? "";
                Console.Write("City: ");
                string city = Console.ReadLine() ?? "";

                await customerService.AddCustomerAsync(new Customer
                {
                    Name = name,
                    Email = email,
                    City = city
                });

                Console.WriteLine("Customer added successfully.");
                break;
            }
            case "2":
            {
                var customers = await customerService.GetAllCustomersAsync();
                if (customers.Count == 0)
                {
                    Console.WriteLine("No customers found.");
                    break;
                }

                foreach (var c in customers)
                    Console.WriteLine($"{c.CustomerId} | {c.Name} | {c.Email} | {c.City}");
                break;
            }
            case "3":
            {
                Console.Write("Customer ID: ");
                if (!int.TryParse(Console.ReadLine(), out int id))
                {
                    Console.WriteLine("Invalid customer ID.");
                    break;
                }

                var c = await customerService.GetCustomerByIdAsync(id);
                if (c is null)
                    Console.WriteLine("Customer not found.");
                else
                    Console.WriteLine($"{c.CustomerId} | {c.Name} | {c.Email} | {c.City}");
                break;
            }
            case "4":
            {
                Console.Write("Product name: ");
                string productName = Console.ReadLine() ?? "";
                Console.Write("Price: ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal price))
                {
                    Console.WriteLine("Invalid price.");
                    break;
                }
                Console.Write("Stock quantity: ");
                if (!int.TryParse(Console.ReadLine(), out int stock))
                {
                    Console.WriteLine("Invalid stock quantity.");
                    break;
                }

                await productService.AddProductAsync(new Product
                {
                    ProductName = productName,
                    Price = price,
                    StockQuantity = stock
                });

                Console.WriteLine("Product added successfully.");
                break;
            }
            case "5":
            {
                var products = await productService.GetAllProductsAsync();
                if (products.Count == 0)
                {
                    Console.WriteLine("No products found.");
                    break;
                }

                foreach (var p in products)
                    Console.WriteLine($"{p.ProductId} | {p.ProductName} | {p.Price:N2} | Stock: {p.StockQuantity}");
                break;
            }
            case "6":
            {
                Console.Write("Product ID: ");
                if (!int.TryParse(Console.ReadLine(), out int id))
                {
                    Console.WriteLine("Invalid product ID.");
                    break;
                }

                var p = await productService.GetProductByIdAsync(id);
                if (p is null)
                    Console.WriteLine("Product not found.");
                else
                    Console.WriteLine($"{p.ProductId} | {p.ProductName} | {p.Price:N2} | Stock: {p.StockQuantity}");
                break;
            }
            case "7":
                {
                    Console.Write("Customer ID: ");
                    int.TryParse(Console.ReadLine(), out int custId);
                    Console.Write("Product ID: ");
                    int.TryParse(Console.ReadLine(), out int prodId);
                    Console.Write("Quantity: ");
                    int.TryParse(Console.ReadLine(), out int qty);

                    var invoice = await productService.PlaceOrderAsync(custId, prodId, qty);

                    Console.WriteLine("\n========================================");
                    Console.WriteLine("           E-COMMERCE TAX INVOICE       ");
                    Console.WriteLine("========================================");
                    Console.WriteLine($"Order ID     : #{invoice.OrderId}");
                    Console.WriteLine($"Customer ID  : {invoice.CustomerId}");
                    Console.WriteLine($"Product ID   : {invoice.ProductId}");
                    Console.WriteLine($"Quantity     : {invoice.Quantity}");
                    Console.WriteLine($"Total Amount : ₹{invoice.TotalAmount:N2}");
                    Console.WriteLine($"Date & Time  : {invoice.OrderedAt}");
                    Console.WriteLine("========================================");
                    Console.WriteLine("Status: Order placed successfully!");
                    break;
                }
            case "8":
                {
                    var report = await reportService.GetProductSalesSummaryAsync();
                    Console.WriteLine("\n=== PRODUCT SALES PERFORMANCE REPORT ===");
                    Console.WriteLine("ID | Product Name       | Units Sold | Total Revenue");
                    Console.WriteLine("--------------------------------------------------");
                    foreach (var r in report)
                    {
                        Console.WriteLine($"{r.ProductId,-2} | {r.ProductName,-18} | {r.TotalUnitsSold,-10} | ₹{r.TotalRevenue:N2}");
                    }
                    break;
                }
            case "9":
                Console.WriteLine("Application closed.");
                return;
            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Validation error: {ex.Message}");
    }
    catch (SqlException ex)
    {
        Console.WriteLine($"Database error: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
    }
}
