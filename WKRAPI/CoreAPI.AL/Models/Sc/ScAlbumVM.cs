namespace CoreAPI.AL.Models.Sc;

public class ScAlbumVM
{
    public string Name { get; set; }
    public string LibRelPath { get; set; }
    public int PageCount { get; set; }
    public int LastPageIndex { get; set; }
    public FileInfoModel CoverInfo { get; set; }
}