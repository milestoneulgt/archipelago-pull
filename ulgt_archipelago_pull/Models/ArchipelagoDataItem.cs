using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ulgtArchipelagoPull.Models;

public class ArchipelagoDataItem
{
    public string? Name { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();
}
