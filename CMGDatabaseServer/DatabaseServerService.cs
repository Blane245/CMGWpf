using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CMGDatabaseServer;

public class DatabaseServerService : BackgroundService
{
    private readonly ILogger<DatabaseServerService> _logger;
    private DatabaseServer? _server;

    public DatabaseServerService(ILogger<DatabaseServerService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("CMGDBServer service is starting.");

            _server = new DatabaseServer();

            // Run the server in a background task
            await Task.Run(async () => await _server.StartAsync(), stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while running the service.");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CMGDBServer service is stopping.");

        _server?.Stop();

        await base.StopAsync(cancellationToken);
    }
}
