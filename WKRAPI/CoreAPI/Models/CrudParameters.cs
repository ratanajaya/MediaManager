namespace CoreAPI;

public class AlbumOuterValueParam
{
    public required string AlbumPath { get; set; }
    public int LastPageIndex { get; set; }
    public string? LastPageAlRelPath { get; set; }
}

public class AlbumTierParam
{
    public required string AlbumPath { get; set; }
    public int Tier { get; set; }
}