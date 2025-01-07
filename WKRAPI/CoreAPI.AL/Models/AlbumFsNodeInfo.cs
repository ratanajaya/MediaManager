using System.Collections.Generic;

namespace CoreAPI.AL.Models;

public class AlbumFsNodeInfo
{
    public required string Title { get; set; }
    public required string Orientation { get; set; }
    public List<FsNode> FsNodes { get; set; } = [];
}