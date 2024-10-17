using System.Collections.Generic;

namespace CoreAPI.AL.Models.Sc;

public class ScDbModel
{
    public ScDbModel() {
        ScMetadatas = new List<ScMetadataModel>();
    }
    public List<ScMetadataModel> ScMetadatas { get; set; }
}

public class ScMetadataModel
{
    public string Path { get; set; }
    public int LastPageIndex { get; set; }
}