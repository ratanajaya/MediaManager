using System.Collections.Generic;

namespace MetadataEditor.Models;

public class SplitTagModel
{
    public List<string> Povs { get; set; } = [];
    public List<string> Focuses { get; set; } = [];
    public List<string> Others { get; set; } = [];
    public List<string> Rares { get; set; } = [];
    public List<string> Qualities { get; set; } = [];
}