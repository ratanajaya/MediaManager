namespace CoreAPI.AL.Models.Dto;

public class OcrResult
{
    public string Original { get; set; }
    public string Romanized { get; set; }
    public string English { get; set; }

    public string Error { get; set; }
}
