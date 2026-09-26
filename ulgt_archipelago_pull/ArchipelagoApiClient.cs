using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ulgtArchipelagoPull.Models;

namespace ulgtArchipelagoPull;

public class ArchipelagoApiClient(HttpClient httpClient, ILogger<ArchipelagoApiClient> logger)
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<ArchipelagoApiClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IEnumerable<ArchipelagoDataItem>> GetDataItemsAsync(
        string endpoint, 
        Dictionary<string, 
        string>? queryParams = null
    )
    {
        try
        {
            var uri = endpoint;

            if (queryParams != null && queryParams.Count > 0)
            {
                uri += "?";
                foreach (var param in queryParams)
                {
                    uri += $"{param.Key}={Uri.EscapeDataString(param.Value)}&";
                }
                uri = uri.TrimEnd('&');
            }

            _logger.LogInformation("Fetching data from Archipelago API: {uri}", uri);

            var response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            var items = await response.Content.ReadFromJsonAsync<IEnumerable<ArchipelagoDataItem>>();
            return items ?? [];
        }
        catch (Exception ex)
        {             
            _logger.LogError(ex, "Error fetching data from Archipelago API: {ErorrMessage}", ex.Message);
            throw;
        }
    }
}
