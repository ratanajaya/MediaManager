using CoreAPI.AL.Models.LogDb;
using SharedLibrary.Models;
using System.Collections.Generic;

namespace CoreAPI.AL.Models;

public class SourceAndContent
{
    public Source Source { get; set; }
    public List<Comment> Comments { get; set; }
}

public class SourceAndContentUpsertModel
{
    public string AlbumPath { get; set; }
    public SourceAndContent SourceAndContent { get; set; }
}

public class SourceDeleteModel 
{
    public string AlbumPath { get; set; }
    public string Url { get; set; }
}

public class SourceUpdateModel
{
    public string AlbumPath { get; set; }
    public Source Source { get; set; }
}