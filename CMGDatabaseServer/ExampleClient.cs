using System.Configuration;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace CMGDatabaseServer;

/// <summary>
/// Example client for connecting to the CMG Database Server
/// </summary>
public class ExampleClient
{
    private readonly string _serverAddress;
    private readonly int _serverPort;

    public ExampleClient(string serverAddress = "localhost", int serverPort = 5555)
    {
        _serverAddress = serverAddress;
        _serverPort = serverPort;
    }

    public async Task<DatabaseResponse?> ExecuteQueryAsync(string query, Dictionary<string, object?>? parameters = null)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_serverAddress, _serverPort);

            var request = new DatabaseRequest
            {
                Query = query,
                Parameters = parameters
            };

            var requestJson = JsonSerializer.Serialize(request);
            var requestBytes = Encoding.UTF8.GetBytes(requestJson);

            using var stream = client.GetStream();
            await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

            var buffer = new byte[8192];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            return JsonSerializer.Deserialize<DatabaseResponse>(responseJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client error: {ex.Message}");
            return new DatabaseResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public static async Task RunExampleAsync()
    {
        var client = new ExampleClient();

        // Example 1: Create a table
        Console.WriteLine("Creating example table...");
        var createTableResponse = await client.ExecuteQueryAsync(
            @"CREATE TABLE IF NOT EXISTS example_users (
                id INT AUTO_INCREMENT PRIMARY KEY,
                name VARCHAR(100) NOT NULL,
                email VARCHAR(100) NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )"
        );
        Console.WriteLine($"Success: {createTableResponse?.Success}");

        // Example 2: Insert data with parameters
        Console.WriteLine("\nInserting data...");
        var insertResponse = await client.ExecuteQueryAsync(
            "INSERT INTO example_users (name, email) VALUES (@name, @email)",
            new Dictionary<string, object?>
            {
                { "@name", "John Doe" },
                { "@email", "john@example.com" }
            }
        );
        Console.WriteLine($"Success: {insertResponse?.Success}, Rows Affected: {insertResponse?.RowsAffected}");

        // Example 3: Query data
        Console.WriteLine("\nQuerying data...");
        var selectResponse = await client.ExecuteQueryAsync(
            "SELECT * FROM example_users WHERE name = @name",
            new Dictionary<string, object?>
            {
                { "@name", "John Doe" }
            }
        );

        if (selectResponse?.Success == true && selectResponse.Data != null)
        {
            Console.WriteLine($"Found {selectResponse.Data.Count} records:");
            foreach (var row in selectResponse.Data)
            {
                foreach (var kvp in row)
                {
                    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                }
                Console.WriteLine();
            }
        }

        // Example 4: Update data
        Console.WriteLine("Updating data...");
        var updateResponse = await client.ExecuteQueryAsync(
            "UPDATE example_users SET email = @email WHERE name = @name",
            new Dictionary<string, object?>
            {
                { "@email", "johndoe@example.com" },
                { "@name", "John Doe" }
            }
        );
        Console.WriteLine($"Success: {updateResponse?.Success}, Rows Affected: {updateResponse?.RowsAffected}");

        // Example 5: Delete data
        Console.WriteLine("\nDeleting data...");
        var deleteResponse = await client.ExecuteQueryAsync(
            "DELETE FROM example_users WHERE name = @name",
            new Dictionary<string, object?>
            {
                { "@name", "John Doe" }
            }
        );
        Console.WriteLine($"Success: {deleteResponse?.Success}, Rows Affected: {deleteResponse?.RowsAffected}");
    }
}
