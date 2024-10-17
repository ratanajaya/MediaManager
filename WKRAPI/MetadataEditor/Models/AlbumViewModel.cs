using System.Collections.Generic;
using SharedLibrary.Models;

namespace MetadataEditor.Models;

public class AlbumViewModel
{
    public Album Album { get; set; }
    public string Path { get; set; }
    public List<string> AlbumFiles { get; set; }
}