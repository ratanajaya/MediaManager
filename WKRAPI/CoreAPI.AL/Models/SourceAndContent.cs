using CoreAPI.AL.Models.LogDb;
using SharedLibrary.Models;
using System.Collections.Generic;

namespace CoreAPI.AL.Models;

public class SourceAndContent
{
    public required Source Source { get; set; }
    public List<Comment> Comments { get; set; } = [];
}

public class SourceAndContentUpsertModel
{
    public required string AlbumPath { get; set; }
    public required SourceAndContent SourceAndContent { get; set; }
}

public class SourceDeleteModel 
{
    public required string AlbumPath { get; set; }
    public required string Url { get; set; }
}

public class SourceUpdateModel
{
    public required string AlbumPath { get; set; }
    public required Source Source { get; set; }
}