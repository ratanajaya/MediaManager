using System;

namespace SharedLibrary.Models;

public class FileCorrectionModel
{
    public string? AlRelPath { get; set; }
    public string? Extension { get; set; }
    public long Byte { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public DateTime ModifiedDate { get; set; }

    public long Area {
        get {
            return Height * Width;
        }
    }

    public int BytesPer100Pixel {
        get {
            if(Area == 0 || Byte == 0) return 0;

            return (int)(100 * (decimal)Byte / (decimal)Area);
        }
    }

    public FileCorrectionType? CorrectionType { get; set; }

    public float? UpscaleMultiplier { get; set; }
    public CompressionCondition? Compression { get; set; }
}

public class FileCorrectionReportModel
{
    public string? AlRelPath { get; set; }
    public string? AlRelDstPath { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }

    public int Height { get; set; }
    public int Width { get; set; }

    public long Byte { get; set; }
    public int BytesPer100Pixel { get; set; }
}

public class UpscaleCompressApiParam
{
    public UpscalerType UpscalerType { get; set; }
    public FileCorrectionType CorrectionType { get; set; }

    public float? UpscaleMultiplier { get; set; }
    public CompressionCondition? Compression { get; set; }
    public bool ToJpeg { get; set; }
    public required string Extension { get; set; }
}

public enum FileCorrectionType
{
    Upscale = 1,
    Compress = 2
}