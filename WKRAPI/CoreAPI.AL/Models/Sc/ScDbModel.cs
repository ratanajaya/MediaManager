using System.Collections.Generic;

namespace CoreAPI.AL.Models.Sc;

public class ScDbModel
{
    public List<ScMetadataModel> ScMetadatas { get; set; } = [];
}

public class ScMetadataModel
{
    public required string Path { get; set; }
    public int LastPageIndex { get; set; }
    public string? LastPageAlRelPath { get; set; }
}