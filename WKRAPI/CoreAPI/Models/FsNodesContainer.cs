using CoreAPI.AL.Models;
using System.Collections.Generic;

namespace CoreAPI.Models;

public class FsNodeContainer
{
    public string Title { get; set; }
    public List<FsNode> FsNodes { get; set; }
}