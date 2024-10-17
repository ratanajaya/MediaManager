using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using C = SharedLibrary.Constants;
using Ext = SharedLibrary.Constants.Extension;

namespace SharedLibrary.Models;

public class AlbumInfoProvider
{
    public string[] Povs { get; set; }
    public string[] Focuses { get; set; }
    public string[] Others { get; set; }
    public string[] Rares { get; set; }
    public string[] Qualities { get; set; }

    public string[] Characters { get; set; }
    public string[] Categories { get; set; }
    public string[] Orientations { get; } = { C.Orientation.Portrait, C.Orientation.Landscape, C.Orientation.Auto };
    public string[] Languages { get; } = { C.Language.English, C.Language.Japanese, C.Language.Chinese, C.Language.Other };

    public string[] SuitableImageFormats { get; } = { Ext.Jpg, Ext.Jpeg, Ext.Jfif, Ext.Png, Ext.Gif, Ext.Webp };
    public string[] SuitableVideoFormats { get; } = { Ext.Webm, Ext.Mp4 };
    public string[] SuitableFileFormats { get { return SuitableImageFormats.Concat(SuitableVideoFormats).ToArray(); } }
    public string[] CorrectableImageFormats { get; } = { Ext.Jpg, Ext.Jpeg, Ext.Jfif, Ext.Png, Ext.Webp };

    public List<QueryModel> GenreQueries { get; set; }

    public string[] Tier1Artists { get; set; }
    public string[] Tier2Artists { get; set; }

    public string NUrl { get; set; }
    public string EUrl { get; set; }

    public bool IsImage(string path) => SuitableImageFormats.Contains(Path.GetExtension(path));

    public bool IsVideo(string path) => SuitableVideoFormats.Contains(Path.GetExtension(path));
}