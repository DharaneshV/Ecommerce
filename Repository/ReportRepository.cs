using Microsoft.Data.SqlClient;
using ECommerceStore.Models;

namespace ECommerceStore.Repository;

public class ReportRepository
{
    private readonly string _connectionString;

    public ReportRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<ProductSalesSummary>> GetProductSalesSummaryAsync()
    {
        const string query = """
            SELECT p.ProductId, p.ProductName, 
                   ISNULL(SUM(o.Quantity), 0) AS TotalUnitsSold, 
                   ISNULL(SUM(o.TotalAmount), 0) AS TotalRevenue
            FROM Products AS p
            LEFT JOIN Orders AS o ON o.ProductId = p.ProductId
            GROUP BY p.ProductId, p.ProductName
            ORDER BY TotalRevenue DESC;
            """;

        var summaries = new List<ProductSalesSummary>();
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            summaries.Add(new ProductSalesSummary
            {
                ProductId = reader.GetInt32(0),
                ProductName = reader.GetString(1),
                TotalUnitsSold = reader.GetInt32(2),
                TotalRevenue = reader.GetDecimal(3)
            });
        }

        return summaries;
    }
}