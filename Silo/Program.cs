using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering()
            .AddMemoryGrainStorageAsDefault()
            .ConfigureLogging(logging => logging.AddConsole())
            .UseTransactions();
    })
    .UseConsoleLifetime();

using IHost host = builder.Build();

await host.RunAsync();