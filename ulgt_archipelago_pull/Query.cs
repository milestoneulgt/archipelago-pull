using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ulgtArchipelagoPull;

public static class Utils
{

    // Read Settings needed to call the Archipelago API
    public static IConfiguration ReadSettings(string filename)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(filename, optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        return configuration;
    }

    // Retrieve a token which can be used to call the Archipelago API
    public static async Task<string> GetToken(IConfiguration configuration, int? environment = 0)
    {
        var environmentConfigKey = environment != 1 ? "ArchipelagoStageApi" : "ArchipelagoApi";
        var auth_url = configuration.GetSection($"{environmentConfigKey}:AuthUrl").Value;
        var client_id = configuration.GetSection($"{environmentConfigKey}:ClientId").Value;
        var client_secret = configuration.GetSection($"{environmentConfigKey}:ClientSecret").Value;
        var audience = configuration.GetSection($"{environmentConfigKey}:Audience").Value;

        var authHttpClient = new HttpClient
        {
            BaseAddress = new Uri(auth_url ?? "")
        };

        authHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
        string auth_query = @"{{""client_id"": ""{0}"", ""client_secret"": ""{1}"", ""audience"": ""{2}"", ""grant_type"": ""client_credentials""}}";
        auth_query = string.Format(auth_query, client_id, client_secret, audience);
        var auth_request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = new StringContent(auth_query, Encoding.UTF8, "application/json")
        };
        var auth_response = await authHttpClient.SendAsync(auth_request);
        var contents = await auth_response.Content.ReadAsStringAsync();
        var jsonOutput = JsonSerializer.Deserialize<Dictionary<string, object>>(contents);
        var access_token = jsonOutput != null ? jsonOutput["access_token"].ToString() : "";

        return access_token ?? "";
    }

    public class QueryObject
    {
        public required string query { get; set; }
    };

    public static async Task<string> RunQuery(string query, string access_token, string api_url)
    {       
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(api_url ?? "")
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);

        query = query.Replace("\n", string.Empty);
        query = query.Replace("\r", string.Empty);
        query = query.Replace("\t", string.Empty);

        var queryObject = new QueryObject { query = query };

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = JsonContent.Create(queryObject)
        };
        var response = await httpClient.SendAsync(request);
        var stringResult = await response.Content.ReadAsStringAsync();
        var jsonOutput = JsonSerializer.Deserialize<Dictionary<string, object>>(stringResult);


        return stringResult;
    }

    public static IEnumerable<List<T>> Partition<T>(this IList<T> source, int size)
    {
        for (int i = 0; i < Math.Ceiling(source.Count / (double)size); i++)
            yield return new List<T>(source.Skip(size * i).Take(size));
    }

}

