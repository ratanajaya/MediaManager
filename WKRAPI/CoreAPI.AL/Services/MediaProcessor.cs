using CoreAPI.AL.Models;
using Serilog;
using SharedLibrary;
using SharedLibrary.Helpers;
using SharedLibrary.Models;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace CoreAPI.AL.Services;

public class MediaProcessor
{
    ISystemIOAbstraction _io;
    AlbumInfoProvider _ai;
    ILogger _logger;

    public MediaProcessor(ISystemIOAbstraction io, AlbumInfoProvider ai, ILogger logger) {
        _io = io;
        _ai = ai;
        _logger = logger;
    }

    public FileInfoModel GenerateFileInfo(string rootPath, string fullFilePath, bool includeDetail, bool includeDimension) {
        long size = 0;
        string extension = null;
        DateTime? createDate = null;
        DateTime? updateDate = null;
        PageOrientation? orientation = null;
        int height = 0;
        int width = 0;

        try {
            if(includeDetail || includeDimension) {
                var fi = _io.GetFileInfo(fullFilePath);
                extension = fi.Extension;
                if(includeDetail) {
                    size = fi.Length;
                    createDate = fi.CreationTime;
                    updateDate = fi.LastWriteTime;
                }

                if(includeDimension) {
                    if(_ai.IsImage(fullFilePath)) {
                        using(var img = _io.LoadImage(fullFilePath)) {
                            orientation = img.Width > img.Height ? PageOrientation.Landscape : PageOrientation.Portrait;
                            height = img.Height;
                            width = img.Width;
                        }
                    }
                    else if(_ai.IsVideo(fullFilePath)) {
                        var media = _io.LoadMediaInfo(fullFilePath);
                        if(media.Success) {
                            orientation = media.Width > media.Height ? PageOrientation.Landscape : PageOrientation.Portrait;
                            height = media.Height;
                            width = media.Width;
                        }
                        else {
                            orientation = PageOrientation.Portrait;
                            _logger.Warning($"MediaInfoWrapper failed ['${fullFilePath}']");
                        }
                    }
                }
            }
            else {
                extension = Path.GetExtension(fullFilePath);
            }
        }
        catch(Exception e) {
            _logger.Warning($"GenerateFileInfo | " +
                $"[{rootPath},{fullFilePath},{includeDetail},{includeDimension}] " +
                $"| {e.Message}");
        }

        return new FileInfoModel {
            Name = Path.GetFileName(fullFilePath),
            LibRelPath = Path.GetRelativePath(rootPath, fullFilePath),
            Extension = extension,
            Size = size,
            CreateDate = createDate,
            UpdateDate = updateDate,
            Orientation = orientation,
            Height = height,
            Width = width
        };
    }

    public void GenerateResizedImage(string srcPath, string dstPath, int maxSize) {
        using(var image = _io.LoadImage(srcPath)) {
            image.ReziseToResolutionLimit(maxSize);

            image.SaveAsJpeg(dstPath);
        }
    }

    public async Task GenerateVideothumbnail(string srcPath, string dstPath, string tempPath, int maxSize) {
        var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(srcPath, tempPath, TimeSpan.FromSeconds(0));
        await conversion.Start();

        var bitmapOriginal = _io.ReadBitmap(tempPath);

        var bitmapResized = bitmapOriginal.ResizeToResolutionLimit(maxSize);
        bitmapOriginal.Dispose();
        var byteResized = bitmapResized.ToByteArray();
        bitmapResized.Dispose();
        await _io.WriteFile(dstPath, byteResized);
    }
}
