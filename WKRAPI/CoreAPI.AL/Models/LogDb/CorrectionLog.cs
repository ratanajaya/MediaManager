using SQLite;
using System;

namespace CoreAPI.AL.Models.LogDb;

public class CorrectionLog
{
    [PrimaryKey]
    public string? Path { get; set; }
    public DateTime LastCorrectionDate { get; set; }
    public int CorrectablePageCount { get; set; }
}
