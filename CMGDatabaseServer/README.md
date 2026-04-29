# CMG Database Server

A C# MySQL database server for the CMG application.

## Features

- TCP/IP server for handling database requests
- MySQL connectivity using MySql.Data
- JSON-based request/response protocol
- Parameterized query support
- Connection pooling
- Async/await pattern for scalability
- **Keep-alive mechanism** to prevent MySQL connection timeouts (pings every hour with `SELECT 1 + 1`)

## Configuration

Edit `appsettings.json` to configure:

### Server Settings
- **Port**: TCP port the server listens on (default: 5555)
- **MaxConnections**: Maximum concurrent connections
- **KeepAliveIntervalMinutes**: Interval in minutes for MySQL keep-alive pings (default: 60)

### Database Settings
- **Server**: MySQL server address (default: localhost)
- **Port**: MySQL server port (default: 3306)
- **Database**: Database name (default: cmg)
- **UserId**: MySQL username (default: root)
- **Password**: MySQL password
- **ConnectionTimeout**: Connection timeout in seconds
- **MinimumPoolSize**: Minimum connection pool size
- **MaximumPoolSize**: Maximum connection pool size

## Usage

### Running as a Windows Service (Recommended)

The CMG Database Server can be installed as a Windows Service that starts automatically when your computer boots.

📖 **See [SERVICE-INSTALLATION.md](SERVICE-INSTALLATION.md) for complete installation instructions.**

Quick install (as Administrator):
```powershell
dotnet publish -c Release
.\Install-Service.ps1
```

### Running from Command Line (Development)

For development and testing:

```bash
dotnet run --project CMGDatabaseServer
```

### Client Request Format

Send JSON requests via TCP:

```json
{
  "Query": "SELECT * FROM users WHERE id = @id",
  "Parameters": {
    "@id": 1
  }
}
```

### Response Format

```json
{
  "Success": true,
  "Data": [
    {
      "id": 1,
      "name": "John Doe",
      "email": "john@example.com"
    }
  ],
  "RowsAffected": 1,
  "ErrorMessage": null
}
```

## Example Client Code (C#)

```csharp
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

var client = new TcpClient();
await client.ConnectAsync("localhost", 5555);

var request = new DatabaseRequest
{
    Query = "SELECT * FROM users WHERE id = @id",
    Parameters = new Dictionary<string, object?> { { "@id", 1 } }
};

var requestJson = JsonSerializer.Serialize(request);
var requestBytes = Encoding.UTF8.GetBytes(requestJson);

using var stream = client.GetStream();
await stream.WriteAsync(requestBytes);

var buffer = new byte[4096];
var bytesRead = await stream.ReadAsync(buffer);
var responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
var response = JsonSerializer.Deserialize<DatabaseResponse>(responseJson);
```

## Security Notes

⚠️ **Important**: This is a basic implementation. For production use, consider:

- Authentication and authorization
- Encrypted connections (TLS/SSL)
- Input validation and SQL injection prevention
- Rate limiting
- Logging and monitoring
- Error handling improvements
- Connection pooling optimization

## Requirements

- .NET 10.0
- MySQL Server
- MySql.Data NuGet package (included)

## MySQL Connector Note

While you mentioned downloading the MySQL C++ connector, this C# implementation uses the `MySql.Data` NuGet package which is the official MySQL connector for .NET. The C++ connector would be used if you were writing a C++ application.

If you need to use the C++ connector instead, the project would need to be created as a C++/CLI or native C++ project rather than a C# project.
