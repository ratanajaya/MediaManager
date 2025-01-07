using System.IO;

namespace CoreAPI.AL.Models.Dto;

public class FileMoveParamModel
{
    public required FileMoveModel Src { get; set; }
    public required FileMoveModel Dst { get; set; }
    public bool Overwrite { get; set; }
}
public class FileMoveModel
{
    public required string AlbumPath { get; set; }
    public required string AlRelPath { get; set; }
    public string LibRelPath
    {
        get
        {
            return Path.Combine(AlbumPath, AlRelPath);
        }
    }
}