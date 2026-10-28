using Microsoft.Data.SqlClient;
using ECommerceStore.Helpers;
using ECommerceStore.Models;
using System.Data;

namespace ECommerceStore.Repository;

public class ReportRepository
{
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
        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(query, connection);

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

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