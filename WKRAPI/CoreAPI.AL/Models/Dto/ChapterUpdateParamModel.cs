namespace CoreAPI.AL.Models.Dto;

public class ChapterUpdateParamModel
{
    public required string AlbumPath { get; set; }
    public required string ChapterName { get; set; }
    public string? NewChapterName { get; set; }
    public int? Tier { get; set; }
}