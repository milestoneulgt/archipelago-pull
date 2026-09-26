using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ulgtArchipelagoPull.Interfaces;
using ulgtArchipelagoPull.Models;

namespace ulgtArchipelagoPull.Services;

public class ArchipelagoReadDataTransferBackgroundService(
    ISqlConnectionFactory connectionFactory,
        ILogger<ArchipelagoReadDataTransferBackgroundService> logger,
        IOptionsMonitor<ReadProcessConfiguration> processConfig,
        IConfiguration configuration,
        IHostApplicationLifetime appLifetime) : BackgroundService
{
    private readonly ISqlConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    private readonly ILogger<ArchipelagoReadDataTransferBackgroundService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOptionsMonitor<ReadProcessConfiguration> _processConfig = processConfig ?? throw new ArgumentNullException(nameof(processConfig));
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly IHostApplicationLifetime _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
    private bool _isProcessing = false;

    private static readonly TimeSpan DelayBetweenBatches = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10)
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Archipelago Read Data Transfer Background Service (ARDT) started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var config = _processConfig.CurrentValue;

            if (!config.IsEnabled)
            {
                _logger.LogDebug("ARDT Service is disabled, waiting...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                continue;
            }

            try
            {
                await _processingSemaphore.WaitAsync(stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                _isProcessing = true;
                _logger.LogInformation("Starting ARDT process execution");

                // Your actual work goes here
                await DoWorkAsync(stoppingToken);

                _logger.LogInformation("ARDT Process execution completed");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ARDT Process was cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during ARDT process execution");
            }
            finally
            {
                _isProcessing = false;
                _processingSemaphore.Release();
            }

            if (config.IntervalSeconds > 0)
            {
                var delay = TimeSpan.FromSeconds(config.IntervalSeconds);
                _logger.LogDebug("Waiting {Delay} seconds before next execution (ARDT)", delay.TotalSeconds);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                    _logger.LogInformation("ARDT service run completed");
                    _logger.LogInformation("ARDT service paused for {Inverval} {Seconds}", config.IntervalSeconds, config.IntervalSeconds < 2 ? "second" : "seconds");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("ARDT service stopped");
                    _appLifetime.StopApplication();
                    break;
                }
            }
            else
            {
                _isProcessing = false;
                _logger.LogInformation("ARDT service stopped");
                _appLifetime.StopApplication();
                break;
            }
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting ARDT data transfer from external API to database");


            _logger.LogInformation("Obtaining Streams..");

            var config = _processConfig.CurrentValue;

            var environmentConfigKey = config?.Environment != 1 ? "ArchipelagoStageApi" : "ArchipelagoApi";

            var api_url = _configuration.GetSection($"{environmentConfigKey}:BaseUrl").Value ?? string.Empty;
            var auth_url = _configuration.GetSection($"{environmentConfigKey}:AuthDomain").Value ?? string.Empty;
            var clientId = _configuration.GetSection($"{environmentConfigKey}:ClientId").Value ?? string.Empty;
            var clientSecret = _configuration.GetSection($"{environmentConfigKey}:ClientSecret").Value ?? string.Empty;
            var audience = _configuration.GetSection($"{environmentConfigKey}:Audience").Value ?? string.Empty;

            var authManager = new Auth0TokenManager(auth_url, clientId, clientSecret, audience);

            var pageSize = config?.PageSize ?? 50;
            var isPropertyLossRetrievalEnabled = config?.IsPropertyLossRetrievalEnabled ?? false;
            IConfiguration configuration = Utils.ReadSettings("appsettings.json");

            StreamsHelpers.pageSize = pageSize;
            StreamsHelpers.api_url = api_url;
            StreamsHelpers.authManager = authManager;

            Streams streamClass = new();

            var streams = await streamClass.GetStreams();

            var dataStreams = streams?.streams ?? [];

            _logger.LogInformation("Current Page Size: {PageSize}", StreamsHelpers.pageSize);
            _logger.LogInformation("Number of Streams: {Count}", streams?.totalCount);

            if (streams != null && streams.totalCount > StreamsHelpers.pageSize && dataStreams.Count <= streams.totalCount)
            {
                var moreStreams = await streamClass.GetStreamsPagination(streams.totalCount);
                if (moreStreams != null)
                {
                    dataStreams.AddRange(moreStreams);
                }
            }

            _logger.LogInformation("Streams Obtained: {Count}", dataStreams.Count);

            PropertyHelpers.pageSize = pageSize;
            PropertyHelpers.api_url = api_url;
            PropertyHelpers.authManager = authManager;

            

            List<PropertyResponse> dataProperties = [];
            List<Dictionary<string, object>> dataPropertyLosses = [];

            string? firstRunExecutionTime = null;

            bool? lastRunExecutionTime = config?.IsFirstRunCheckActive;
            if (lastRunExecutionTime.HasValue && lastRunExecutionTime.Value)
            {
                firstRunExecutionTime = await ReadDataToSqlServerAsync();
            }

            var effectiveFrom = string.IsNullOrEmpty(firstRunExecutionTime) ? null : config?.EffectiveFrom;

            // Parse up front, preserving stream order
            var parsedStreams = dataStreams
                .Select(stream => JsonSerializer.Deserialize<Dictionary<string, object>>(stream.ToString() ?? ""))
                .Where(js => js != null)
                .Select(js => js!)
                .ToList();

            // Fetch + build each stream's data items concurrently, in one pass per stream
            var dataItemTasks = parsedStreams.Select(async jsonStream =>
            {
                var streamId = jsonStream["id"].ToString() ?? "";
                var streamItems = new List<ArchipelagoDataItem>
                {
                    new() { Name = "stream", Attributes = jsonStream }
                };

                _logger.LogInformation("Obtaining Stream Properties For Stream ({Stream})", streamId);
                
                StreamProperties streamProperties = new();
                var response = await streamProperties.GetStreamProperties(streamId, effectiveFrom);

                if (response == null || response.totalCount <= 0)
                {
                    _logger.LogInformation("Stream ({Stream}) Properties Obtained : {PropertyCount}", streamId, response?.properties?.Count ?? 0);
                    return streamItems;
                }

                List<object> properties = response.properties ?? [];
                List<Dictionary<string, object>> propertyLosses = [];

                if (isPropertyLossRetrievalEnabled && response.totalCount <= StreamsHelpers.pageSize)
                {
                    if (properties.Count > 0)
                    {
                        propertyLosses = await streamProperties.GetPropertyLosses(streamId, properties);
                    }
                    _logger.LogInformation("Stream ({Stream}) Properties Obtained : {PropertyCount}", streamId, properties.Count);
                }
                else
                {
                    // Pagination stays as-is / sequential within a stream
                    var moreProperties = await streamProperties.GetPropertiesPagination(streamId, response.totalCount, effectiveFrom);
                    if (moreProperties != null && moreProperties.Count() > 0)
                    {
                        properties.AddRange(moreProperties);
                    }
                    response.properties = properties;

                    _logger.LogInformation("Stream ({Stream}) Properties Obtained : {PropertyCount}", streamId, properties.Count);

                    if (isPropertyLossRetrievalEnabled && properties.Count > 0)
                    {
                        propertyLosses = await streamProperties.GetPropertyLosses(streamId, properties);
                    }
                }

                foreach (var chunk in properties.Chunk(pageSize))
                {
                    if (chunk.Length <= 0) continue;

                    var chunkedResponse = new PropertyResponse
                    {
                        streamId = response.streamId,
                        properties = [.. chunk],
                        totalCount = chunk.Length
                    };

                    var jsonStreamProperties = JsonSerializer.Serialize(chunkedResponse);
                    var jsonStreamPropertiesDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStreamProperties ?? "");
                    if (jsonStreamPropertiesDict != null)
                    {
                        streamItems.Add(new ArchipelagoDataItem { Name = "streamProperties", Attributes = jsonStreamPropertiesDict });
                    }
                }

                if (propertyLosses.Count > 0)
                {
                    streamItems.Add(new ArchipelagoDataItem
                    {
                        Name = "propertyLosses",
                        Attributes = new Dictionary<string, object> { { "propertyLosses", propertyLosses } }
                    });
                }

                return streamItems;
            });

            var streamItemResults = await Task.WhenAll(dataItemTasks);
            List<ArchipelagoDataItem> dataItems = streamItemResults.SelectMany(items => items).ToList();

            if (dataItems.Count <= 0)
            {
                _logger.LogWarning("No data items retrieved from API. Nothing to transfer.");
                return;
            }

           
            //Save To Database
            await SaveDataToSqlServerAsync(dataItems.AsEnumerable());
            await UpdateDataToSqlServerAsync();
            _logger.LogInformation("Data transfer process completed successfully (ARDT");

        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Data transfer was cancelled (ARDT) ");
            throw; // Re-throw to be handled by the caller
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data transfer (ARDT): {Message}", ex.Message);
            throw; // Re-throw to be handled by the caller
        }
    }

    private async Task SaveBatchWithRetryAsync(ArchipelagoDataItem[] batch)
    {
        var config = _processConfig.CurrentValue;

        int attempt = 0;

        while (true)
        {
            using var connection = _connectionFactory.CreateConnection(config.SqlDatabase);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var item in batch)
                {
                    await InsertOrUpdateDataItemAsync(connection, transaction, item);
                }

                transaction.Commit();
                return;
            }
            catch (SqlException ex) when (IsTransientMemoryPressure(ex) && attempt < RetryDelays.Length)
            {
                transaction.Rollback();
                _logger.LogWarning(ex, "Transient SQL resource pressure on batch, retrying in {Delay}s (attempt {Attempt})",
                    RetryDelays[attempt].TotalSeconds, attempt + 1);
                await Task.Delay(RetryDelays[attempt]);
                attempt++;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error saving batch to SQL Server (ARDT): {Message}", ex.Message);
                throw;
            }
        }
    }

    private static bool IsTransientMemoryPressure(SqlException ex) =>
    ex.Message.Contains("resource pool", StringComparison.OrdinalIgnoreCase)
    || ex.Number == -2; // timeout

    private static bool IsFatalConnectionOrConfigError(SqlException ex) =>
    ex.Class >= 20 // connection-terminating severity per SqlException docs
    || ex.Number is 18456   // login failed
                  or 4060   // cannot open database
                  or 40615  // firewall rule
                  or 229;   // permission denied on object (broken/missing stored proc grant)

    private async Task SaveDataToSqlServerAsync(IEnumerable<ArchipelagoDataItem> items)
    {
        var config = _processConfig.CurrentValue;
        var batchSize = config.PageSize;
        int batchNumber = 0;
        int failedBatches = 0;

        logger.LogInformation("");
        logger.LogInformation("Processing items to SQL Server database [SaveDataToSqlServerAsync] (ARDT)");
        logger.LogInformation("Batch Size: {batchSize}",batchSize);
        logger.LogInformation("Number of Records: {count}", items.Count());
        logger.LogInformation("");

        foreach (var batch in items.Chunk(batchSize))
        {
            try
            {
                await SaveBatchWithRetryAsync(batch);
            }
            catch (SqlException ex) when (IsFatalConnectionOrConfigError(ex))
            {
                _logger.LogError(ex, "Fatal SQL error on batch {BatchNumber} (ARDT) - stopping run: {Message}", batchNumber, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                failedBatches++;
                _logger.LogError(ex, "Batch {BatchNumber} failed and was skipped (ARDT): {Message}", batchNumber, ex.Message);
            }

            await Task.Delay(DelayBetweenBatches);
        }

        if (failedBatches > 0)
        {
            _logger.LogWarning("Completed with {FailedBatches} of {TotalBatches} batches failed (ARDT)", failedBatches, batchNumber);
        }
        else
        {
            _logger.LogInformation("All data items successfully saved to SQL Server (ARDT)");
        }
    }

    private async Task UpdateDataToSqlServerAsync()
    {
        var config = _processConfig.CurrentValue;
        using var connection = _connectionFactory.CreateConnection(config.SqlDatabase);
        await connection.OpenAsync();

        try
        {
            UpdateProcessSetting(connection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating data in SQL Server (ARDT): {Message}", ex.Message);
            throw;
        }
    }

    private async Task<string> ReadDataToSqlServerAsync()
    {

        var config = _processConfig.CurrentValue;
        using var connection = _connectionFactory.CreateConnection(config.SqlDatabase);
        await connection.OpenAsync();

        try
        {
            return ReadProcessSetting(connection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading data to SQL Server (ARDT): {Message}", ex.Message);

            throw;
        }
    }

    private static void UpdateProcessSetting(SqlConnection connection)
    {
        using var checkCmd = new SqlCommand($"Archipelago.ArchipelagoInsertUpdateFirstRunExecutionTime", connection);

        checkCmd.CommandType = CommandType.StoredProcedure;

        checkCmd.Parameters.Add(new SqlParameter("@FirstRunExecutionTime", SqlDbType.DateTime2, 7) { Value = DateTime.Now });

        checkCmd.ExecuteNonQuery();
    }

    private static async Task InsertOrUpdateDataItemAsync(SqlConnection connection, SqlTransaction transaction, ArchipelagoDataItem item)
    {
        // Check if the item already exists
        using var checkCmd = new SqlCommand($"Archipelago.InsertArchipelagoJSON", connection, transaction);

        checkCmd.CommandType = CommandType.StoredProcedure;

        checkCmd.Parameters.Add(new SqlParameter("@streamExport", SqlDbType.NVarChar, 50) { Value = item.Name });
        checkCmd.Parameters.Add(new SqlParameter("@JSONtxt", SqlDbType.NVarChar) { Value = JsonSerializer.Serialize(item.Attributes) });

        await checkCmd.ExecuteNonQueryAsync();
    }

    private static string ReadProcessSetting(SqlConnection connection)
    {
        using var checkCmd = new SqlCommand($"Archipelago.ArchipelagoReadFirstRunExecutionTime", connection);

        checkCmd.CommandType = CommandType.StoredProcedure;

        object result = checkCmd.ExecuteScalar();

        string? firstRunExecutionTime = result?.ToString();

        return firstRunExecutionTime ?? "";
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stop requested - waiting for ARDT process to complete");

        // Wait for current processing to complete
        if (_isProcessing)
        {
            _logger.LogInformation("ARDT Process is running, waiting for completion...");
            await _processingSemaphore.WaitAsync(cancellationToken);
            _processingSemaphore.Release();
        }

        await base.StopAsync(cancellationToken);
        _logger.LogInformation("ARDT service gracefully stopped");
    }

    public override void Dispose()
    {
        _processingSemaphore?.Dispose();
        base.Dispose();
        _logger.LogInformation("ARDT Service disposed.");
        GC.SuppressFinalize(this);
    }
}

