using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Diagnostics;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

namespace SharedLibrary;

#pragma warning disable CA1416
/// <summary>
/// Taken from yugee's Mass Image Compressor
/// https://sourceforge.net/p/icompress/code/
/// </summary>
public class ImageProcessor
{
    #region From ImageCompressor.cs
    public bool CompressImage(string filePath, string savePath, int quality, Size size, SupportedMimeType type) {
        if(Path.GetExtension(filePath) == Constants.Extension.Webp)
            return CompressWebp(filePath, savePath, quality, size);

        return CompressJpegPng(filePath, savePath, quality, size, type);
    }

    private bool CompressJpegPng(string filePath, string savePath, int quality, Size size, SupportedMimeType type) {
        Bitmap img = GetBitmap(filePath);
        byte[] originalFile = null;
        if(SavingAsSameMimeType(filePath, type)) {
            originalFile = File.ReadAllBytes(filePath);
        }

        ImageCodecInfo imageCodecInfo;

        if(type == SupportedMimeType.JPEG)
            imageCodecInfo = GetEncoderInfo("image/jpeg");
        else if(type == SupportedMimeType.PNG)
            imageCodecInfo = GetEncoderInfo("image/png");
        else
            imageCodecInfo = GetEncoderInfoFromOriginalFile(filePath);

        if(imageCodecInfo == null)
            return false;

        EncoderParameters encoderParameters;

        bool keepOriginalSize = false;
        long OriginalFileSize = GetFileSize(filePath);
        if(img.Size.Height <= size.Height || img.Size.Width <= size.Width) {
            size = img.Size;
            keepOriginalSize = true;
        }

        Bitmap imgCompressed = new Bitmap(size.Width, size.Height);
        using(Graphics gr = Graphics.FromImage(imgCompressed)) {
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            gr.DrawImage(img, new Rectangle(0, 0, size.Width, size.Height));
        }

        foreach(var id in img.PropertyIdList) {
            imgCompressed.SetPropertyItem(img.GetPropertyItem(id));
        }
        img.Dispose();


        if(quality > GetQualityIfCompressed(filePath, imgCompressed))
            quality = GetQualityIfCompressed(filePath, imgCompressed); //don't save higher qulaity than required.

        //SetImageComments(filePath, imgCompressed, quality);

        encoderParameters = new EncoderParameters(1);
        encoderParameters.Param[0] =
            new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

        string fileSavePath = ChangeExensionToMimeType(savePath, type);
        imgCompressed.Save(fileSavePath, imageCodecInfo, encoderParameters);

        imgCompressed.Dispose();

        if(fileSavePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) {
            if(quality < 100)
                CompressPNG(fileSavePath, quality, true);
            OptimizePNG(fileSavePath, true);
        }
        else if(fileSavePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || fileSavePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)) {
            MakeJpegProgressive(fileSavePath, true);
        }

        if(keepOriginalSize && SavingAsSameMimeType(filePath, type) && GetFileSize(fileSavePath) > OriginalFileSize) {
            File.WriteAllBytes(fileSavePath, originalFile);
        }

        return true;
    }

    private bool CompressWebp(string filePath, string savePath, int quality, Size size) {
        using(var image = SixLabors.ImageSharp.Image.Load(filePath))
        using(var saveStream = new FileStream(savePath, FileMode.Create)) {
            image.Mutate(c => c.Resize(size.Width, size.Height));
            image.Save(saveStream, new WebpEncoder() {
                    FileFormat = WebpFileFormatType.Lossy,
                    Quality = quality
                });
        }        

        return true;
    }
    #endregion

    #region From Helper.cs
    int GetQualityIfCompressed(string filePath, Bitmap bmp) {
        try {
            PropertyItem propItem;

            if(bmp.PropertyIdList.Contains(0x9286))
                propItem = bmp.GetPropertyItem(0x9286);
            else
                return 100;

            string comment = System.Text.Encoding.UTF8.GetString(propItem.Value);

            if(comment.Contains('\0'))
                comment = string.Join("", comment.Split('\0'));

            string[] splits = comment.Split(':');
            if(splits.Length > 1) {
                return int.Parse(splits[2]);
            }
            else
                return 100;
        }
        catch(Exception) {
            return 100; //default to 100 in case of error.
        }
    }

    Bitmap GetBitmap(string filepath) {
        return new System.Drawing.Bitmap(filepath);
    }

    string GetFullPath(string execName) {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Executables", execName);
    }

    bool SavingAsSameMimeType(string filePath, SupportedMimeType type) {
        string ext = Path.GetExtension(filePath);

        if(type == SupportedMimeType.ORIGINAL)
            return true;

        if((ext.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase) || ext.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase))
            && (type == SupportedMimeType.JPEG))
            return true;
        if(ext.Equals(".png", StringComparison.InvariantCultureIgnoreCase) && (type == SupportedMimeType.PNG))
            return true;
        return false;
    }

    ImageCodecInfo GetEncoderInfo(String mimeType) {
        int j;
        ImageCodecInfo[] encoders;
        encoders = ImageCodecInfo.GetImageEncoders();
        for(j = 0; j < encoders.Length; ++j) {
            if(encoders[j].MimeType == mimeType)
                return encoders[j];
        }
        return null;
    }

    ImageCodecInfo GetEncoderInfoFromOriginalFile(String filePath) {
        string inputfileExt = "*" + Path.GetExtension(filePath).ToUpper();

        int j;
        ImageCodecInfo[] encoders;
        encoders = ImageCodecInfo.GetImageEncoders();
        for(j = 0; j < encoders.Length; ++j) {
            if(encoders[j].FilenameExtension.Contains(inputfileExt))
                return encoders[j];
        }

        return null;
    }

    string ChangeExensionToMimeType(string fullFilePath, SupportedMimeType type) {
        string dirPath = RemoveFileName(fullFilePath);

        if(type == SupportedMimeType.JPEG)
            return
                AddDirectorySeparatorAtEnd(dirPath)
                + Path.GetFileNameWithoutExtension(fullFilePath)
                + Constants.Extension.Jpeg;
        else if(type == SupportedMimeType.PNG)
            return
                AddDirectorySeparatorAtEnd(dirPath)
                + Path.GetFileNameWithoutExtension(fullFilePath)
                + Constants.Extension.Png;
        else
            return fullFilePath;
    }

    string RemoveFileName(string pathWithFileName) {
        string dirPath = "";

        if(string.IsNullOrEmpty(Path.GetFileName(pathWithFileName)))
            return "";

        string[] DirsAndFile = pathWithFileName.Split(Path.DirectorySeparatorChar);
        if(DirsAndFile == null) return "";

        for(int i = 0; i < DirsAndFile.Length - 1; i++)
            dirPath += DirsAndFile[i] + Path.DirectorySeparatorChar;
        return dirPath;
    }

    string AddDirectorySeparatorAtEnd(string path) {
        if(!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            return path + Path.DirectorySeparatorChar.ToString();
        return path;
    }

    void CompressPNG(string filePath, int qualityLevel, bool waitForProcessToEnd) {
        var exeFilePath = GetFullPath("pngquant.exe");
        var args = " --quality=0-" + qualityLevel + " -f --ext .png ";

        RunExternalOperationOnImage(filePath, exeFilePath, args, waitForProcessToEnd);
    }

    void OptimizePNG(string filePath, bool waitForProcessToEnd) {
        var exeFilePath = GetFullPath("optipng.exe");
        var args = "-o2 -strip all";

        RunExternalOperationOnImage(filePath, exeFilePath, args, waitForProcessToEnd);
    }

    void RunExternalOperationOnImage(string filePath, string exeFilePath, string args, bool waitForProcessToEnd) {
        var f = new FileInfo(exeFilePath);
        var startInfo = new System.Diagnostics.ProcessStartInfo(f.FullName) {
            Arguments = args + " \"" + filePath + "\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        var process = System.Diagnostics.Process.Start(startInfo);

        if(waitForProcessToEnd)
            process.WaitForExit();
    }

    long GetFileSize(string filePath) {
        return new FileInfo(filePath).Length;
    }

    void MakeJpegProgressive(string filePath, bool waitForProcessToEnd) {
        var exeFilePath = GetFullPath("jpegtran.exe");
        var args = "-copy all -optimize -progressive -outfile " + " \"" + filePath + "\"";
        RunExternalOperationOnImage(filePath, exeFilePath, args, waitForProcessToEnd);
    }
    #endregion

    public string UpscaleImageRealEsrGan(string srcPath, string dstPath, int multiplier, UpscalerType modelType, int? tileSize) {
        var modelName = modelType == UpscalerType.RealesrganX4plusAnime ? "realesrgan-x4plus-anime" : "realesrgan-x4plus";

        var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Executables", "realesrgan", "realesrgan-ncnn-vulkan.exe");

        var tileArg = tileSize.HasValue ? $" -t {tileSize.Value}" : "";

        var arg = $"-i \"{srcPath}\" -o \"{dstPath}\" -s {multiplier} -n {modelName}{tileArg}";

        var startInfo = new ProcessStartInfo(exePath) {
            Arguments = arg,
            UseShellExecute = false,
            CreateNoWindow = true,
            ErrorDialog = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };

        string consoleOutput = "";

        using(Process process = new Process()) {
            process.StartInfo = startInfo;
            process.Start();

            consoleOutput += process.StandardError.ReadToEnd();

            process.WaitForExit();
        }

        return consoleOutput;
    }

    public string UpscaleImageRealSr(string srcPath, string dstPath, int multiplier, UpscalerType modelType, int? tileSize) {
        var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Executables", "realsr", "realsr-ncnn-vulkan.exe");

        var tileArg = tileSize.HasValue ? $" -t {tileSize.Value}" : "";

        var arg = $"-i \"{srcPath}\" -o \"{dstPath}\" -s {multiplier}{tileArg}";

        var startInfo = new ProcessStartInfo(exePath) {
            Arguments = arg,
            UseShellExecute = false,
            CreateNoWindow = true,
            ErrorDialog = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };

        string consoleOutput = "";

        using(Process process = new Process()) {
            process.StartInfo = startInfo;
            process.Start();

            consoleOutput += process.StandardError.ReadToEnd();

            process.WaitForExit();
        }

        return consoleOutput;
    }

    public string UpscaleImageWaifu2x(string srcPath, string dstPath, int multiplier, UpscalerType modelType, int? tileSize) {
        var modelName = modelType == UpscalerType.Waifu2xAnime ? "models-upconv_7_anime_style_art_rgb" : "models-cunet";

        var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Executables", "waifu2x", "waifu2x-ncnn-vulkan.exe");

        var tileArg = tileSize.HasValue ? $" -t {tileSize.Value}" : "";

        var arg = $"-i \"{srcPath}\" -o \"{dstPath}\" -s {multiplier} -n {modelName}{tileArg}";

        var startInfo = new ProcessStartInfo(exePath) {
            Arguments = arg,
            UseShellExecute = false,
            CreateNoWindow = true,
            ErrorDialog = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };

        string consoleOutput = "";

        using(Process process = new Process()) {
            process.StartInfo = startInfo;
            process.Start();

            consoleOutput += process.StandardError.ReadToEnd();

            process.WaitForExit();
        }

        return consoleOutput;
    }
}

public enum UpscalerType {
    RealesrganX4plus = 101,
    RealesrganX4plusAnime = 102,

    SrganD2fkJpeg = 201,

    Waifu2xCunet = 301,
    Waifu2xAnime = 302,
}

public enum SupportedMimeType
{
    JPEG,
    PNG,
    ORIGINAL
}