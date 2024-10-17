using SharedLibrary.Enums;
using System;
using System.Collections.Generic;

namespace CoreAPI.AL.Models;

public class FsNode
{
    public NodeType NodeType { get; set; }
    public string AlRelPath { get; set; }

    public FileInfoModel FileInfo { get; set; }
    public DirInfoModel DirInfo { get; set; }
}

public class DirInfoModel {
    public string Name { get; set; }
    public int Tier { get; set; }
    public List<FsNode> Childs { get; set; } = new();
}