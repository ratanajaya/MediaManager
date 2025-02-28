﻿using SharedLibrary;
using SharedLibrary.Models;
using System.Collections.Generic;

namespace CoreAPI.AL.Models.Sc;

public class CorrectPageParam
{
    public int Type { get; set; }
    public required string LibRelPath { get; set; }
    public int Thread { get; set; }
    public UpscalerType UpscalerType { get; set; }
    public List<FileCorrectionModel> FileToCorrectList { get; set; } = [];
    public bool ToWebp { get; set; }
}
