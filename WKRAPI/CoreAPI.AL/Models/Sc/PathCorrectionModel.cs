﻿using System;

namespace CoreAPI.AL.Models.Sc;

public class PathCorrectionModel
{
    public required string LibRelPath { get; set; }
    public DateTime? LastCorrectionDate { get; set; }
    public int CorrectablePageCount { get; set; }
}
