using SQLite;
using System;

namespace CoreAPI.AL.Models.LogDb;

public class AlbumCorrection
{
    [PrimaryKey]
    public string? LibRelPath { get; set; }
    public int CorrectedPage { get; set; }
    public DateTime CorrectionFinishDate { get; set; }
    public DateTime BatchStartDate { get; set; }
}