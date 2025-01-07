namespace CoreAPI.AL.Models.Dto;

public class OcrResult
{
    public required string Original { get; set; }
    public required string Romanized { get; set; }
    public required string English { get; set; }

    public string? Error { get; set; }
}
