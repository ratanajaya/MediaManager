using SharedLibrary.Models;
using SQLite;
using System;
using System.Text.Json;

namespace CoreAPI.AL.Models.LogDb;

public class CrudLog
{
    public const string Insert = "I";
    public const string Update = "U";
    public const string Delete = "D";
    public const string FirstRead = "F";
    public static readonly TimeSpan MergeThreshold = TimeSpan.FromHours(1);

    [PrimaryKey, AutoIncrement]
    public long Id { get; set; }
    public string Operation { get; set; }
    public string AlbumPath { get; set; }
    public string AlbumFullTitle { get; set; }
    public string AlbumJson { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }

    public CrudLog() { }

    public CrudLog(string operation, string albumPath, Album album, DateTime createDate) {
        if(operation == Delete) {
            AlbumJson = JsonSerializer.Serialize(album);
        }

        Operation = operation;
        AlbumPath = albumPath;
        AlbumFullTitle = album.GetFullTitleDisplay();
        CreateDate = createDate;
    }
}