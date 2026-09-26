using System.Text.Json;

namespace ulgtArchipelagoPull;

internal static class StreamsHelpers
{
    public static int pageSize = 50;
    public static string api_url = string.Empty;
    public static Auth0TokenManager? authManager;

    // function to print streams information
    public static void PrintStreams(List<object> jsonStreams)
    {
        foreach (var stream in jsonStreams)
        {
            var streamResult = JsonSerializer.Deserialize<Dictionary<string, object>>(stream.ToString() ?? "");
            if (streamResult != null)
            {
                Console.WriteLine(streamResult["id"] ?? "");
            }
        }
    }
}