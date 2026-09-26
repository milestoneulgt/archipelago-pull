using System.Text;
using System.Text.Json;

namespace ulgtArchipelagoPull;

public class StreamResponse
{
    public List<object>? streams { get; set; }
    public int totalCount { get; set; }
}

public class Streams
{
    /*
     Snapshot properties

      {
       id
       organizationId   //cannot query       
       date
       description
       bound
       projectID //cannot query    
       visible
       displayDate
       isSovSnapshot
      }
     */

    /* 
     GraphQL query to fetch streams
    */
    string streams_query = $$"""               
            query {
                streamsPage(input:{offset:0,limit:{{StreamsHelpers.pageSize}}}) {
                    streams	{
                        id  
                        orgName
                        name
                        reit
                        broker
                        brokerEmail
                        status
                        isPublic
                        createdAt
                        submitterEmail
                        region
                        team                         
                        updatedAt
                        snapshots {
                          name
                          date
                          description
                          bound
                          visible
                          displayDate
                          isSovSnapshot
                        }
                        allowedExports
                        documentsCount
                        propertiesCount
                        totalInsuredValue                         
                        propertyStatusLabel
                        defaultSnapshot
                        displayCurrency
                        industry
                        businessType
                    }
                    totalCount
                }
            }
     """;

    // function to retrieve (first page of) Streams list
    public async Task<StreamResponse> GetStreams()
    {

        var results = new StreamResponse { streams = [], totalCount = 0 };

        // Request API token        
        var accessToken = StreamsHelpers.authManager != null 
            ? await StreamsHelpers.authManager.GetAccessTokenAsync() 
            : string.Empty;

        // prepare streamsPage query
        StringBuilder sb = new(streams_query);
        sb.Replace(streams_query, "null");
        var stringResult = await Utils.RunQuery(streams_query, accessToken, StreamsHelpers.api_url);

        // traverse streamsPage response
        var jsonResult = JsonSerializer.Deserialize<Dictionary<string, object>>(stringResult);
        if (jsonResult == null || jsonResult.ContainsKey("data")==false || jsonResult["data"] == null)
        {
            if(jsonResult != null && jsonResult.ContainsKey("message"))
            {
                throw new Exception(jsonResult["message"].ToString());
            }
            return results;
        }
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResult["data"].ToString() ?? "");
        if (jsonData == null)
        {
            return results;
        }
        var jsonStreamsPage = JsonSerializer.Deserialize<Dictionary<string, object>>((jsonData["streamsPage"] ?? "").ToString() ?? "");

        if (jsonStreamsPage == null)
        {
            return results;
        }

        var jsonStreams = JsonSerializer.Deserialize<List<object>>((jsonStreamsPage["streams"] ?? "").ToString() ?? "");
        var jsonPageInfo = JsonSerializer.Deserialize<int>(jsonStreamsPage["totalCount"].ToString() ?? "");

        if (jsonStreams != null)
        {
            StreamsHelpers.PrintStreams(jsonStreams);
        }

        results.streams = jsonStreams;
        results.totalCount = jsonPageInfo;

        return results;

    }

    // function to retrieve entire Streams list by traversing through all pages
    public async Task<List<object>> GetStreamsPagination(int totalCount)
    {      
        // traverse initial streamsPage response
        if (totalCount == 0) return [];

        // Continue to run streamsPage with next cursor while hasNextPage=true
        int currPage = 2;
        var results = new List<object>();
        var done = false;
        var prevOffset = 0;
        var adjustedOffset = StreamsHelpers.pageSize;

        while (done == false)
        {
            // Request API token        
            var accessToken = StreamsHelpers.authManager != null
                ? await StreamsHelpers.authManager.GetAccessTokenAsync()
                : string.Empty;

            adjustedOffset = prevOffset > 0 ? adjustedOffset + StreamsHelpers.pageSize : adjustedOffset;

            // pass current cursor to Streams query
            streams_query = streams_query.Replace($"offset:{prevOffset}", $"offset:{adjustedOffset}");

            // traverse next streamsPage response for next cursor / page
            var stringResult = await Utils.RunQuery(streams_query, accessToken, StreamsHelpers.api_url);
            var jsonResult = JsonSerializer.Deserialize<Dictionary<string, object>>(stringResult);
            if (jsonResult == null || jsonResult.ContainsKey("data") == false || jsonResult["data"] == null) {
                continue;
            }
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResult["data"].ToString() ?? "");
            if (jsonData == null) continue;
            var jsonStreamsPage = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonData["streamsPage"].ToString() ?? "");
            if (jsonStreamsPage == null) continue;
            var streams = JsonSerializer.Deserialize<List<object>>(jsonStreamsPage["streams"].ToString() ?? "");

            ++currPage;

            if (streams != null)
            {
                StreamsHelpers.PrintStreams(streams);
                results.AddRange(streams);
            }

            var retreivedCnt = results.Count;
            done = retreivedCnt + StreamsHelpers.pageSize >= totalCount;

            if (!done)
            {
                prevOffset = adjustedOffset;
            }
        }
        return results;
    }
}