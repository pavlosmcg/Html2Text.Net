using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// stdout is only for our mcp action
// send logs to stderr instead
builder.Logging.AddConsole(o =>
{
    o.LogToStandardErrorThreshold = LogLevel.Information;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
