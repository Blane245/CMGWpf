using CMGWpf.Services;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static CMGWpf.Types.DBTypes;

namespace CMGWpf.Utilities
{
    /// <summary>
    /// Custom converter to handle items field that comes as a JSON string instead of array
    /// </summary>
    public class SequenceItemArrayConverter : JsonConverter<SequenceItem[]>
    {
        public override SequenceItem[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            // If it's a string, parse it as JSON first (this is what the DB returns)
            if (reader.TokenType == JsonTokenType.String)
            {
                string? jsonString = reader.GetString();
                if (string.IsNullOrEmpty(jsonString))
                {
                    return Array.Empty<SequenceItem>();
                }

                // Deserialize the string as JSON without custom converters to avoid infinite recursion
                // This mimics the JavaScript client: JSON.parse(response.value.items)
                return JsonSerializer.Deserialize<SequenceItem[]>(jsonString);
            }

            // If it's already an array, deserialize normally (fallback for properly formatted JSON)
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                return JsonSerializer.Deserialize<SequenceItem[]>(ref reader);
            }

            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, SequenceItem[] value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }

    public class DbResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Value { get; set; }
        public DbErrorType? Error { get; set; }

        public static DbResult<T> Success(T value) => new DbResult<T> { IsSuccess = true, Value = value };
        public static DbResult<T> Failure(DbErrorType error) => new DbResult<T> { IsSuccess = false, Error = error };
    }

    public static class CMGDB
    {
        private static readonly HttpClient httpClient;

        static CMGDB()
        {
            var handler = new HttpClientHandler
            {
                UseProxy = false,
                MaxConnectionsPerServer = 10
            };

            httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        public static async Task<string> FetchAsync(string uri, string method, object? body = null)
        {
            Debug.WriteLine($"Starting fetch for uri={uri}");
            string server = GlobalService.Instance.DbServer;
            string port = GlobalService.Instance.DbPort;

            string hostname = server.Replace("http://", "").Replace("https://", "");

            List<string> ipv4Addresses = new();
            try
            {
                var addresses = await System.Net.Dns.GetHostAddressesAsync(hostname);
                ipv4Addresses = addresses
                    .Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(a => a.ToString())
                    .ToList();
            }
            catch
            {
                // DNS resolution failed, will use original hostname
            }

            if (ipv4Addresses.Count == 0)
            {
                ipv4Addresses.Add(hostname);
            }

            Exception? lastException = null;
            foreach (var ip in ipv4Addresses)
            {
                string url = $"http://{ip}:{port}/{uri}";

                try
                {
                    HttpResponseMessage response;

                    if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                    {
                        response = await httpClient.GetAsync(url);
                    }
                    else if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                    {
                        string jsonBody = body != null ? JsonSerializer.Serialize(body) : "";
                        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                        response = await httpClient.PostAsync(url, content);
                    }
                    else if (method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
                    {
                        string jsonBody = body != null ? JsonSerializer.Serialize(body) : "";
                        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                        response = await httpClient.PutAsync(url, content);
                    }
                    else if (method.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                    {
                        response = await httpClient.DeleteAsync(url);
                    }
                    else
                    {
                        throw new ArgumentException($"Unsupported HTTP method: {method}");
                    }

                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                }
                catch (TaskCanceledException ex)
                {
                    lastException = ex;
                }
            }

            throw lastException ?? new HttpRequestException("Failed to connect to any available IP address");
        }

        public static async Task<DbResult<T>> FetchAsync<T>(string uri, string method, object? body = null)
        {
            string json = await FetchAsync(uri, method, body);
            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("type", out JsonElement typeElement))
            {
                string typeString = typeElement.GetString() ?? "";
                if (typeString.Equals("error", StringComparison.OrdinalIgnoreCase))
                {
                    var deserializeOptions = new JsonSerializerOptions 
                    { 
                        IncludeFields = true,
                        Converters = { new JsonStringEnumConverter() }
                    };
                    var error = JsonSerializer.Deserialize<DbErrorType>(json, deserializeOptions);
                    return DbResult<T>.Failure(error);
                }
            }

            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                Converters = { new JsonStringEnumConverter(), new SequenceItemArrayConverter() }
            };

            var value = JsonSerializer.Deserialize<T>(json, options);
            if (value == null)
            {
                throw new JsonException("Deserialization returned null");
            }

            return DbResult<T>.Success(value);
        }
    }
}
