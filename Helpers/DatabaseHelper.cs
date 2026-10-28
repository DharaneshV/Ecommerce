using Microsoft.Data.SqlClient;

namespace ECommerceStore.Helpers;

public static class DatabaseHelper
{
    // Update your server instance name here if needed (e.g., .\SQLEXPRESS)
    private const string ConnectionString =
        @"Server=.\SQLEXPRESS;Database=ECommerceDB;" +
        @"Trusted_Connection=True;TrustServerCertificate=True;";

    public static SqlConnection CreateConnection()
        => new SqlConnection(ConnectionString);
}