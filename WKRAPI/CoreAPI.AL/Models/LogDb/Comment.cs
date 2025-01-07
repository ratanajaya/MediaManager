using SQLite;
using System;

namespace CoreAPI.AL.Models.LogDb;

public class Comment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string? Url { get; set; } //Album.Sources.Url

    public string? Author { get; set; }
    public string? Content { get; set; }
    public double? Score { get; set; }
    public DateTime? PostedDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
}