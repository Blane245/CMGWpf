using MySql.Data.MySqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace CMGDatabaseServer;

public class DatabaseServer
{
    private readonly TcpListener _listener;
    private readonly string _connectionString;
    private readonly Timer _keepAliveTimer;
    private bool _isRunning;

    public DatabaseServer(int port = 5555, string? connectionString = null)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _connectionString = connectionString ?? "Server=localhost;Port=3306;Database=cmg;Uid=root;Pwd=;";
        _isRunning = false;

        // Set up keep-alive timer to run every hour
        _keepAliveTimer = new Timer(KeepAliveCallback, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
    }

    public async Task StartAsync()
    {
        _listener.Start();
        _isRunning = true;

        Console.WriteLine($"CMG Database Server started on port {((IPEndPoint)_listener.LocalEndpoint).Port}");
        Console.WriteLine($"MySQL Connection: {GetSafeConnectionString()}");
        Console.WriteLine("Press Ctrl+C to stop the server.");

        try
        {
            while (_isRunning)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server error: {ex.Message}");
        }
        finally
        {
            _listener.Stop();
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using var stream = client.GetStream();
            var buffer = new byte[4096];
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                var requestJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var request = JsonSerializer.Deserialize<DatabaseRequest>(requestJson);

                if (request != null)
                {
                    var response = await ProcessRequestAsync(request);
                    var responseJson = JsonSerializer.Serialize(response);
                    var responseBytes = Encoding.UTF8.GetBytes(responseJson);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client handling error: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    private async Task<DatabaseResponse> ProcessRequestAsync(DatabaseRequest request)
    {
        var response = new DatabaseResponse();

        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand(request.Query, connection);

            if (request.Parameters != null)
            {
                foreach (var param in request.Parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            if (request.Query.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = await command.ExecuteReaderAsync();
                var results = new List<Dictionary<string, object?>>();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    results.Add(row);
                }

                response.Success = true;
                response.Data = results;
                response.RowsAffected = results.Count;
            }
            else
            {
                var rowsAffected = await command.ExecuteNonQueryAsync();
                response.Success = true;
                response.RowsAffected = rowsAffected;
            }
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
            Console.WriteLine($"Query error: {ex.Message}");
        }

        return response;
    }

    private string GetSafeConnectionString()
    {
        var builder = new MySqlConnectionStringBuilder(_connectionString);
        if (!string.IsNullOrEmpty(builder.Password))
        {
            builder.Password = "***";
        }
        return builder.ToString();
    }

    private void KeepAliveCallback(object? state)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT 1 + 1 AS solution", connection);
            var result = command.ExecuteScalar();
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Keep-alive ping successful. Result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Keep-alive ping failed: {ex.Message}");
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _keepAliveTimer?.Dispose();
        _listener.Stop();
        Console.WriteLine("Server stopped.");
    }
}

public class DatabaseRequest
{
    public string Query { get; set; } = string.Empty;
    public Dictionary<string, object?>? Parameters { get; set; }
}

public class DatabaseResponse
{
    public bool Success { get; set; }
    public List<Dictionary<string, object?>>? Data { get; set; }
    public int RowsAffected { get; set; }
    public string? ErrorMessage { get; set; }
}
