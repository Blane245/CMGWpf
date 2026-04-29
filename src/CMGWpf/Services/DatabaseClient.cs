using CMGDatabaseServer;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace CMGWpf.Services
{
    public class DatabaseClient (string serverAddress = "localhost", int serverPort = 5555)
    {
        private readonly string _serverAddress = serverAddress;
        private readonly int _serverPort = serverPort;

        public async Task<DatabaseResponse?> ExecuteQueryAsync(string query, Dictionary<string, object?>? parameters = null)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(_serverAddress, _serverPort);
                var request = new DatabaseRequest { Query = query, Parameters = parameters };
                var requestJson = JsonSerializer.Serialize(request);
                var requestBytes = Encoding.UTF8.GetBytes(requestJson);
                using var stream = client.GetStream();
                await stream.WriteAsync(requestBytes, 0, requestBytes.Length);
                var buffer = new byte[8192];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                var responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var response = JsonSerializer.Deserialize<DatabaseResponse>(responseJson);
                return response;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Error executing query: {ex.Message}");
                return new DatabaseResponse { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}
