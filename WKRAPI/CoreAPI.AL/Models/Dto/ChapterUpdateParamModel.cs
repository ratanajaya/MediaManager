namespace CoreAPI.AL.Models.Dto;

public class ChapterUpdateParamModel
{
    public string AlbumPath { get; set; }
    public string ChapterName { get; set; }
    public string NewChapterName { get; set; }
    public int? Tier { get; set; }
}