using System.IO;

namespace CoreAPI.AL.Models.Dto;

public class FileMoveParamModel
{
    public FileMoveModel Src { get; set; }
    public FileMoveModel Dst { get; set; }
    public bool Overwrite { get; set; }
}
public class FileMoveModel
{
    public string AlbumPath { get; set; }
    public string AlRelPath { get; set; }
    public string LibRelPath
    {
        get
        {
            return Path.Combine(AlbumPath, AlRelPath);
        }
    }
}