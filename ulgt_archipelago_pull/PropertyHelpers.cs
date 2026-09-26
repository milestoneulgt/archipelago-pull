using System.Text.Json;

namespace ulgtArchipelagoPull;

internal static class PropertyHelpers
{
    public static int pageSize = 50;
    public static string api_url = string.Empty;
    public static string orgName = string.Empty;
    public static Auth0TokenManager? authManager;

    public static void PrintProperties(string stringResult)
    {
        var jsonResult = JsonSerializer.Deserialize<Dictionary<string, object>>(stringResult);
        if (jsonResult == null || jsonResult["data"] == null) return;
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResult["data"].ToString() ?? "");
        if (jsonData == null) return;
        var jsonStreamsProperties = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonData["streamProperties"].ToString() ?? "");
        if (jsonStreamsProperties == null) return;
        var jsonProperties = JsonSerializer.Deserialize<List<object>>(jsonStreamsProperties["properties"].ToString() ?? "");
        if (jsonProperties == null) return;
        foreach (var property in jsonProperties)
        {
            var propertyResult = JsonSerializer.Deserialize<Dictionary<string, object>>(property.ToString() ?? "");
            if (propertyResult == null) continue;
            Console.WriteLine(propertyResult["archipelagoID"]);
        }
    }
}