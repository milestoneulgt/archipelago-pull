namespace ulgtArchipelagoPull.Models;

public class ReadProcessConfiguration
{
    public const string SectionName = "ArchipelagoReadSettings";

    public int Environment { get; set; } // 1 means Production, otherwise Stage
    public bool IsEnabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 30;
    public string EffectiveFrom { get; set; } = string.Empty;
    public bool IsFirstRunCheckActive { get; set; } = true;
    public string SqlDatabase { get; set; } = string.Empty;
    public int PageSize { get; set; } = 200;
    public bool IsPropertyLossRetrievalEnabled { get; set; } = false; 
}
