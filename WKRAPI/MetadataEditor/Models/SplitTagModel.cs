using System.Collections.Generic;

namespace MetadataEditor.Models;

public class SplitTagModel
{
    public List<string> Povs { get; set; } = new List<string>();
    public List<string> Focuses { get; set; } = new List<string>();
    public List<string> Others { get; set; } = new List<string>();
    public List<string> Rares { get; set; } = new List<string>();
    public List<string> Qualities { get; set; } = new List<string>();
}