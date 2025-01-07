namespace MetadataEditor.Models;

public class MetadataEditorConfig
{
    public required string BrowsePath { get; set; }
    public string[] Args { get; set; } = [];
}