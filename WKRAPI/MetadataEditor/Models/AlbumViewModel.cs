using System.Collections.Generic;
using SharedLibrary.Models;

namespace MetadataEditor.Models;

public class AlbumViewModel
{
    public required Album Album { get; set; }
    public required string Path { get; set; }
    public List<string> AlbumFiles { get; set; } = [];
}