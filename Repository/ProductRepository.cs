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

    public async Task<OrderInvoice> PlaceOrderAsync(int customerId, int productId, int quantity)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // 1. Read price and stock with UPDLOCK
            const string readSql = """
            SELECT Price, StockQuantity
            FROM Products WITH (UPDLOCK, ROWLOCK)
            WHERE ProductId = @ProductId
            """;

            decimal price;
            int stock;

            await using (var command = new SqlCommand(readSql, connection, (SqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@ProductId", productId);
                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    throw new InvalidOperationException("Product not found.");

                price = reader.GetDecimal(0);
                stock = reader.GetInt32(1);
            }

            if (stock < quantity)
                throw new InvalidOperationException("Insufficient stock.");

            decimal totalAmount = price * quantity;

            // 2. Insert the order
            const string orderSql = """
            INSERT INTO Orders (CustomerId, ProductId, Quantity, TotalAmount)
            VALUES (@CustomerId, @ProductId, @Quantity, @TotalAmount);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

            int orderId;
            await using (var command = new SqlCommand(orderSql, connection, (SqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@CustomerId", customerId);
                command.Parameters.AddWithValue("@ProductId", productId);
                command.Parameters.AddWithValue("@Quantity", quantity);
                command.Parameters.AddWithValue("@TotalAmount", totalAmount);
                orderId = Convert.ToInt32(await command.ExecuteScalarAsync());
            }

            // 3. Reduce the stock
            const string stockSql = """
            UPDATE Products
            SET StockQuantity = StockQuantity - @Quantity
            WHERE ProductId = @ProductId
            """;

            await using (var command = new SqlCommand(stockSql, connection, (SqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@Quantity", quantity);
                command.Parameters.AddWithValue("@ProductId", productId);
                await command.ExecuteNonQueryAsync();
            }

            // 4. Write audit row
            const string auditSql = """
            INSERT INTO OrderAudit (OrderId, ActionName)
            VALUES (@OrderId, @ActionName)
            """;

            await using (var command = new SqlCommand(auditSql, connection, (SqlTransaction)transaction))
            {
                command.Parameters.AddWithValue("@OrderId", orderId);
                command.Parameters.AddWithValue("@ActionName", "ORDER_PLACED");
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();

            return new OrderInvoice
            {
                OrderId = orderId,
                CustomerId = customerId,
                ProductId = productId,
                Quantity = quantity,
                TotalAmount = totalAmount
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
