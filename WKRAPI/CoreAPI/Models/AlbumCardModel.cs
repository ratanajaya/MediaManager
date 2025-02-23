using CoreAPI.AL.Models;
using System;
using System.Collections.Generic;

namespace CoreAPI;

public class AlbumCardModel
{
    public required string Path { get; set; }
    public List<string> Languages { get; set; } = [];
    public bool IsRead { get; set; }
    public bool IsWip { get; set; }
    public int Tier { get; set; }
    public int LastPageIndex { get; set; }
    public string? LastPageAlRelPath { get; set; }
    public int PageCount { get; set; }
    public string? Note { get; set; }
    public int CorrectablePageCount { get; set; }
    public bool HasSource { get; set; }
    public DateTime EntryDate { get; set; }
    public FileInfoModel? CoverInfo { get; set; }

    public required string Title { get; set; }
    public string? ArtistDisplay { get; set; }
}

public class AlbumCardGroup
{
    public required string Name { get; set; }
    public List<AlbumCardModel> AlbumCms { get; set; } = [];
}