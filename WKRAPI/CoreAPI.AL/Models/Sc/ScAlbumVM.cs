namespace CoreAPI.AL.Models.Sc;

public class ScAlbumVM
{
    public required string Name { get; set; }
    public required string LibRelPath { get; set; }
    public int PageCount { get; set; }
    public int LastPageIndex { get; set; }
    public string? LastPageAlRelPath { get; set; }
    public FileInfoModel? CoverInfo { get; set; }
}