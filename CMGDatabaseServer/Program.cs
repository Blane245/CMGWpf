using CMGDatabaseServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<DatabaseServerService>();
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "CMGDBServer";
});

var host = builder.Build();
await host.RunAsync();
