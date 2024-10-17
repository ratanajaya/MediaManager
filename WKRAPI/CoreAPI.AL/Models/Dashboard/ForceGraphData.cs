using System;
using System.Collections.Generic;

namespace CoreAPI.AL.Models.Dashboard;

public class ForceGraphData
{
    public required List<ForceGraphNode> Nodes { get; set; }
    public required List<ForceGraphLink> Links { get; set; }
}

public class ForceGraphNode 
{
    public required string Id { get; set; }
    public int Group { get; set; }
    public required int Count { get; set; }
}

public class ForceGraphLink 
{
    public required string Source { get; set; }
    public required string Target { get; set; }

    public int SourceCount { get; set; }
    public int TargetCount { get; set; }
    public int LinkCount { get; set; }

    public double Value { 
        get {
            return SourceCount == 0 || TargetCount == 0 ? 0
                : Math.Round((double)LinkCount / Math.Min(SourceCount, TargetCount), 3)
            ;
        }
    }
}