using Microsoft.Data.SqlClient;
using ECommerceStore.Models;

namespace ECommerceStore.Repository;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task AddProductAsync(Product product)
    {
        const string query = """
            INSERT INTO Products (ProductName, Price, StockQuantity)
            VALUES (@ProductName, @Price, @StockQuantity)
            """;

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@ProductName", product.ProductName);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        const string query = """
            SELECT ProductId, ProductName, Price, StockQuantity
            FROM Products
            ORDER BY ProductId
            """;

        var products = new List<Product>();

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                ProductId = reader.GetInt32(0),
                ProductName = reader.GetString(1),
                Price = reader.GetDecimal(2),
                StockQuantity = reader.GetInt32(3)
            });
        }

        return products;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        const string query = """
            SELECT ProductId, ProductName, Price, StockQuantity
            FROM Products
            WHERE ProductId = @ProductId
            """;

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ProductId", id);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Product
            {
                ProductId = reader.GetInt32(0),
                ProductName = reader.GetString(1),
                Price = reader.GetDecimal(2),
                StockQuantity = reader.GetInt32(3)
            };
        }

        return null;
    }
}
