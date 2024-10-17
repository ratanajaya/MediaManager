using System;

namespace CoreAPI.AL.Models;

public class FileInfoModel
{
    public string Name { get; set; }
    public string Extension { get; set; }
    public string LibRelPath { get; set; }

    public long Size { get; set; }
    public DateTime? CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }

    public PageOrientation? Orientation { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
}

public enum PageOrientation
{
    Portrait = 1,
    Landscape = 2
}