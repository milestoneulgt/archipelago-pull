using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using ulgtArchipelagoPull.Interfaces;
using ulgtArchipelagoPull.Models;
using ulgtArchipelagoPull.Services;

namespace ulgtArchipelagoPull;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateBuilder(args).Build();

        // Handle Ctrl+C gracefully
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            await host.RunAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Application was cancelled");
        }
    }

    public static IHostBuilder CreateBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false);
                config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);

                if (hostContext.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<Program>(); // Ensure this is called AFTER AddJsonFile
                }
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHttpClient<ArchipelagoApiClient>(client =>
                {
                    client.BaseAddress = new Uri(hostContext.Configuration["ArchipelagoApi:BaseUrl"] ?? "");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                });

                services.AddSingleton<ISqlConnectionFactory>(provider =>
                    new SqlConnectionFactory(hostContext.Configuration.GetConnectionString("SqlDatabase") ?? ""));

                // Configure options
                services.Configure<ReadProcessConfiguration>(
                   hostContext.Configuration.GetSection(ReadProcessConfiguration.SectionName));

                // Register background service
                services.AddHostedService<ArchipelagoReadDataTransferBackgroundService>();

                services.AddLogging(builder =>
                {
                    builder.AddConsole(options => options.FormatterName = "messageOnly");
                    builder.AddConsoleFormatter<LogMessageFormatter, ConsoleFormatterOptions>();
                    builder.AddFile(hostContext.Configuration["Logging:FilePath"]);
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            });
    }
}