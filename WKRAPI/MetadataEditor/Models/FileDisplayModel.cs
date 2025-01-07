using SharedLibrary.Helpers;
using SharedLibrary.Models;

namespace MetadataEditor.Models;

public class FileDisplayModel
{
    string? _fileNameDisplay;
    string? _subDirDisplay;
    public string? FileNameDisplay { get => _fileNameDisplay; set => _fileNameDisplay = Shorten(value, 20); }
    public string? SubDirDisplay { get => _subDirDisplay; set => _subDirDisplay = Shorten(value, 10); }

    string Shorten(string? value, int length) {
        string result = (value ?? "").Replace("\\", "");
        if(result.Length < length) {
            string emptyChar = "";
            int emptyCharCount = length - result.Length;
            for(int i = 0; i < emptyCharCount; i++) {
                emptyChar += " ";
            }
            result += emptyChar;
        }
        if(result.Length > length && length > 3) {
            result = result.Substring(0, length - 3);
            result += "...";
        }
        return result;
    }

    public required string UploadStatus { get; set; }
    public required string Path { get; set; }

    public required FileCorrectionModel CorrectionModel { get; set; }

    public long Area {
        get {
            return CorrectionModel.Area;
        }
    }

    public string SizeDisplay {
        get {
            return CorrectionModel.Byte.BytesToString();
        }
    }

    public string DimensionDisplay {
        get {
            return $"{CorrectionModel.Width}_{CorrectionModel.Height}";
        }
    }

    public int BytesPer100Pixel {
        get {
            return CorrectionModel.BytesPer100Pixel;
        }
    }

    public string BytesPer100PixelDisplay {
        get {
            return BytesPer100Pixel.ToString();
        }
    }
}