using CoreAPI.AL.DataAccess;
using CoreAPI.AL.Models.LogDb;
using CoreAPI.AL.Models.Sc;
using Serilog;
using SharedLibrary;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CoreAPI.AL.Models.Dto;
using CoreAPI.AL.Models.Config;
using CoreAPI.AL.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading;

namespace CoreAPI.AL.Services;

public class OperationService(
    CoreApiConfig _config,
    ISystemIOAbstraction _io,
    AlbumInfoProvider _ai,
    ILogger _logger,
    ImageProcessor _ip,
    ILogDbContext _logDb,
    LibraryRepository _library,
    GoogleService _google,
    IHubContext<ProgressHub> _hubContext
    )
{
    private readonly ConcurrentDictionary<string, bool> _activeOperations = new();

    #region Correction
    public List<PathCorrectionModel> HScanCorrectiblePages(List<string> libRelPaths, int thread, int upscaleTarget) {
        var result = libRelPaths.Select(a => new PathCorrectionModel {
            LibRelPath = a,
            CorrectablePageCount = (GetCorrectablePages(0, a, thread, upscaleTarget, false)).Count
        }).ToList();

        return result;
    }

    public List<PathCorrectionModel> ScGetCorrectablePaths() {
        var allDirs = _io.GetDirectories(_config.ScLibraryPath, SearchOption.AllDirectories);
        int validPathCount = _config.ScLibraryDepth + 1;

        var libRelCorrectablePaths = allDirs
            .Where(a => !a.StartsWith(_config.ScFullExtraInfoPath)
                && Path.GetRelativePath(_config.ScLibraryPath, a).Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Length == validPathCount)
            .Select(a => Path.GetRelativePath(_config.ScLibraryPath, a))
            .ToList();

        var correctionLogs = _logDb.GetCorrectionLogs(libRelCorrectablePaths);

        var pathCorrectionModels = libRelCorrectablePaths
            .Select(a => {
                var log = correctionLogs.FirstOrDefault(b => b.Path == a);

                return new PathCorrectionModel {
                    LibRelPath = a,
                    LastCorrectionDate = log?.LastCorrectionDate,
                    CorrectablePageCount = (log?.CorrectablePageCount).GetValueOrDefault()
                };
            }).ToList();

        return pathCorrectionModels;
    }

    public List<PathCorrectionModel> ScFullScanCorrectiblePaths(int thread, int upscaleTarget) {
        var correctablePaths = ScGetCorrectablePaths();

        foreach(var path in correctablePaths) {
            var correctablePageCount = (GetCorrectablePages(1, path.LibRelPath, thread, upscaleTarget, false)).Count;

            _logDb.UpsertCorrectionLog(path.LibRelPath, null, correctablePageCount);

            path.CorrectablePageCount = correctablePageCount;
        }

        return correctablePaths;
    }

    //Warning: Duplicate logic with AppLogic.GetFileCorrectionModel
    public List<FileCorrectionModel?> GetCorrectablePages(int type, string libRelAlbumPath, int thread, int upscaleTarget, bool clampToTarget) {
        int trueThread = Math.Clamp(thread, 1, 5);
        //This method becomes unstable and produces faulty images if performing upscaling/compression using 6 threads
        //Tested with 6 Core Ryzen 5 5600 6-Core CPU

        try {
            var libPath = _config.GetLibraryPath((LibraryType)type);

            var fullAlbumPath = Path.Combine(libPath, libRelAlbumPath);

            var filePaths = _io.GetSuitableFilePaths(fullAlbumPath, _ai.CorrectableImageFormats, 2);

            FileInfo?[] allFileInfos = new FileInfo[filePaths.Count];
            Parallel.For(0, filePaths.Count, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, (i, state) => {
                var filePath = filePaths[i];
                try {
                    allFileInfos[i] = new FileInfo(filePath);
                }
                catch(Exception ex) {
                    allFileInfos[i] = null;
                    _logger.Error($"GetCorrectableFiles | Parallel.For 1 | {filePath} | {ex.Message}");
                }
            });

            var correctionLog = _logDb.GetCorrectionLog(libRelAlbumPath);
            var lastCorrectionDate = (correctionLog?.LastCorrectionDate).GetValueOrDefault();

            var fileInfoAboveLastDate = allFileInfos.Where(a => a != null && a.LastWriteTime > lastCorrectionDate).ToList();

            FileCorrectionModel?[] correctionModelAboveLastDate = new FileCorrectionModel[fileInfoAboveLastDate.Count];
            Parallel.For(0, fileInfoAboveLastDate.Count, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, (i, state) => {
                var fileInfo = fileInfoAboveLastDate[i];
                if(fileInfo == null) {
                    correctionModelAboveLastDate[i] = null;
                    return;
                }

                try {
                    var newFcm = GetFileCorrectionModel(fileInfo, fullAlbumPath);

                    if(newFcm.IsAnimated) {
                        newFcm.CorrectionType = null;
                    }
                    else {
                        float? multiplier = ImageHelper.DetermineUpscaleMultiplier(newFcm, upscaleTarget);
                        if(multiplier.HasValue) {
                            newFcm.CorrectionType = FileCorrectionType.Upscale;
                            newFcm.UpscaleMultiplier = multiplier;
                            newFcm.Compression = ImageHelper.DetermineCompressionCondition(
                                (int)(multiplier.Value * newFcm.Height),
                                (int)(multiplier.Value * newFcm.Width),
                                newFcm.Extension == Constants.Extension.Png,
                                clampToTarget ? upscaleTarget : null
                            );
                        }
                        else if(ImageHelper.IsLargeEnoughForCompression(newFcm)) {
                            newFcm.CorrectionType = FileCorrectionType.Compress;
                            newFcm.Compression = ImageHelper.DetermineCompressionCondition(
                                newFcm.Height,
                                newFcm.Width,
                                newFcm.Extension == Constants.Extension.Png,
                                null //clampToTarget ? upscaleTarget : null //Don't to target if compression is the only operation
                            );
                        }
                    }

                    correctionModelAboveLastDate[i] = newFcm;
                }
                catch(Exception ex) {
                    correctionModelAboveLastDate[i] = null;
                    _logger.Error($"GetFileViewModels | Parallel.For 2 | {fileInfo.FullName} | {ex.Message}");
                }
            });

            var fileToCorrect = correctionModelAboveLastDate.Where(a => a?.CorrectionType != null).ToList();

            return fileToCorrect;
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.GetCorrectableFiles{Environment.NewLine}" +
                $"Params=[{libRelAlbumPath}{thread}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    //Warning: Duplicate logic with AppLogic.CorrectPages
    public async Task<FileCorrectionReportModel[]> CorrectPages(CorrectPageParam param) {
        var trueThread = Math.Clamp(param.Thread, 1, 5);

        var batchStart = DateTime.Now;

        try {
            var sw = new Stopwatch();
            sw.Start();

            var libPath = _config.GetLibraryPath((LibraryType)param.Type);
            var fullAlbumPath = Path.Combine(libPath, param.LibRelPath);

            var fileCount = param.FileToCorrectList.Count;

            FileCorrectionModel[] fileList = new Func<FileCorrectionModel[]>(() => {
                int cStart = param.FileToCorrectList.FindIndex(a => a.CorrectionType == FileCorrectionType.Compress);
                if(cStart == fileCount - 1 || cStart == -1)
                    return param.FileToCorrectList.ToArray();

                int uStart = 0;
                int cCount = fileCount - cStart;
                int uCount = fileCount - cCount;

                var sortedArr = new FileCorrectionModel[fileCount];
                for(int i = 0; i < fileCount; i++) {
                    if(i % 2 == 0) {
                        if(uStart < uCount) {
                            sortedArr[i] = param.FileToCorrectList[uStart];
                            uStart++;
                        }
                        else {
                            sortedArr[i] = param.FileToCorrectList[cStart];
                            cStart++;
                        }
                    }
                    else {
                        if(cStart < fileCount) {
                            sortedArr[i] = param.FileToCorrectList[cStart];
                            cStart++;
                        }
                        else {
                            sortedArr[i] = param.FileToCorrectList[uStart];
                            uStart++;
                        }
                    }
                }

                return sortedArr;
            })();

            var report = new FileCorrectionReportModel[fileCount];

            int[] possibleUpscaleMultipliers = new List<UpscalerType> { UpscalerType.Waifu2xCunet, UpscalerType.Waifu2xAnime }.Contains(param.UpscalerType) 
                ? new int[] { 2, 4, 8 } 
                : new int[] { 4, 8 };

            Func<string, string, int, UpscalerType, int?, string> upscaleMethod = new List<UpscalerType> { UpscalerType.Waifu2xCunet, UpscalerType.Waifu2xAnime }.Contains(param.UpscalerType)
                ? _ip.UpscaleImageWaifu2x
                : UpscalerType.SrganD2fkJpeg == param.UpscalerType
                ? _ip.UpscaleImageRealSr
                : _ip.UpscaleImageRealEsrGan;

            await Parallel.ForAsync(0, fileCount, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, async (i, state) => {
                var messageSb = new StringBuilder();
                var src = fileList[i];
                var toWebp = src.Extension == Constants.Extension.Webp ? false : param.ToWebp;

                try {
                    var processorApiParam = new UpscaleCompressApiParam {
                        UpscaleMultiplier = src.UpscaleMultiplier,
                        UpscalerType = param.UpscalerType,
                        ToWebp = toWebp,

                        CorrectionType = src.CorrectionType!.Value,
                        Compression = src.Compression,
                        Extension = src.Extension!
                    };

                    var fullOriPath = Path.Combine(fullAlbumPath, src.AlRelPath!);

                    var fullDstPath = toWebp ? $"{Path.Combine(Path.GetDirectoryName(fullOriPath)!, Path.GetFileNameWithoutExtension(fullOriPath))}.webp" : fullOriPath;

                    using(var client = new HttpClient())
                    using(var form = new MultipartFormDataContent()) {
                        form.Add(new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(processorApiParam)), "paramJson");

                        // Replace with your file path
                        var fileContent = new ByteArrayContent(_io.ReadFile(fullOriPath));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                        form.Add(fileContent, "file", Path.GetFileName(fullOriPath));

                        var response = await client.PostAsync($"{_config.ProcessorApiUrl}/Image/UpscaleCompress", form);

                        if(response.IsSuccessStatusCode) {
                            var responseStream = response.Content.ReadAsStream();

                            _io.DeleteFile(fullOriPath);

                            using(var fileStream = new FileStream(fullDstPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                                await responseStream.CopyToAsync(fileStream);
                            }

                            report[i] = new() {
                                AlRelPath = src.AlRelPath,
                                AlRelDstPath = Path.GetRelativePath(fullAlbumPath, fullDstPath),
                                Success = true
                            };
                        }
                        else {
                            var message = await response.Content.ReadAsStringAsync();
                            report[i] = new() {
                                AlRelPath = src.AlRelPath,
                                AlRelDstPath = Path.GetRelativePath(fullAlbumPath, fullDstPath),
                                Success = false,
                                Message = $"Code: {response.StatusCode} | {message}"
                            };
                        }
                    }
                }
                catch(Exception e) {
                    report[i] = new() {
                        AlRelPath = src.AlRelPath,
                        Success = false,
                        Message = messageSb.Append(" | " + e.Message).ToString()
                    };
                }
            });

            if(report.All(a => a.Success)) {
                if(param.Type == 0) {
                    _logDb.UpsertAlbumCorrection(new AlbumCorrection {
                        LibRelPath = param.LibRelPath,
                        CorrectedPage = fileCount,
                        BatchStartDate = batchStart,
                        CorrectionFinishDate = DateTime.Now,
                    });
                }
                else
                    _logDb.UpsertCorrectionLog(param.LibRelPath, DateTime.Now, 0);
            }

            if((LibraryType)param.Type == LibraryType.Regular) {
                _library.RecountAlbumPages(param.LibRelPath);
                _library.DeleteAlbumCache(param.LibRelPath);
            }

            _logger.Information($"Corrected {fileCount} pages in {sw.Elapsed.TotalSeconds} secs");

            Parallel.For(0, report.Length, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, (i, state) => {
                var src = report[i];

                if(src.Success) {
                    var cm = GetFileCorrectionModel(new FileInfo(Path.Combine(fullAlbumPath, src.AlRelDstPath!)), fullAlbumPath);

                    src.Height = cm.Height;
                    src.Width = cm.Width;
                    src.Byte = cm.Byte;
                    src.BytesPer100Pixel = cm.BytesPer100Pixel;
                }
            });

            return report;
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.CorrectPages{Environment.NewLine}" +
                $"Params=[{System.Text.Json.JsonSerializer.Serialize(param)}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }


    public async Task<FileCorrectionReportModel[]> CorrectPagesSignalr(string operationId, CorrectPageParam param) {
        _activeOperations.TryAdd(operationId, true);

        var trueThread = Math.Clamp(param.Thread, 1, 5);

        var batchStart = DateTime.Now;

        try {
            var sw = new Stopwatch();
            sw.Start();

            #region Setup parameters
            var libPath = _config.GetLibraryPath((LibraryType)param.Type);
            var fullAlbumPath = Path.Combine(libPath, param.LibRelPath);

            var fileCount = param.FileToCorrectList.Count;

            FileCorrectionModel[] fileList = new Func<FileCorrectionModel[]>(() => {
                int cStart = param.FileToCorrectList.FindIndex(a => a.CorrectionType == FileCorrectionType.Compress);
                if(cStart == fileCount - 1 || cStart == -1)
                    return param.FileToCorrectList.ToArray();

                int uStart = 0;
                int cCount = fileCount - cStart;
                int uCount = fileCount - cCount;

                var sortedArr = new FileCorrectionModel[fileCount];
                for(int i = 0; i < fileCount; i++) {
                    if(i % 2 == 0) {
                        if(uStart < uCount) {
                            sortedArr[i] = param.FileToCorrectList[uStart];
                            uStart++;
                        }
                        else {
                            sortedArr[i] = param.FileToCorrectList[cStart];
                            cStart++;
                        }
                    }
                    else {
                        if(cStart < fileCount) {
                            sortedArr[i] = param.FileToCorrectList[cStart];
                            cStart++;
                        }
                        else {
                            sortedArr[i] = param.FileToCorrectList[uStart];
                            uStart++;
                        }
                    }
                }

                return sortedArr;
            })();

            var report = new FileCorrectionReportModel[fileCount];

            int[] possibleUpscaleMultipliers = new List<UpscalerType> { UpscalerType.Waifu2xCunet, UpscalerType.Waifu2xAnime }.Contains(param.UpscalerType)
                ? new int[] { 2, 4, 8 }
                : new int[] { 4, 8 };

            Func<string, string, int, UpscalerType, int?, string> upscaleMethod = new List<UpscalerType> { UpscalerType.Waifu2xCunet, UpscalerType.Waifu2xAnime }.Contains(param.UpscalerType)
                ? _ip.UpscaleImageWaifu2x
                : UpscalerType.SrganD2fkJpeg == param.UpscalerType
                ? _ip.UpscaleImageRealSr
                : _ip.UpscaleImageRealEsrGan;
            #endregion

            await Parallel.ForAsync(0, fileCount, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, async (i, state) => {
                var src = fileList[i];
                var toWebp = src.Extension == Constants.Extension.Webp ? false : param.ToWebp;

                try {
                    var processorApiParam = new UpscaleCompressApiParam {
                        UpscaleMultiplier = src.UpscaleMultiplier,
                        UpscalerType = param.UpscalerType,
                        ToWebp = toWebp,

                        CorrectionType = src.CorrectionType!.Value,
                        Compression = src.Compression,
                        Extension = src.Extension!
                    };

                    var fullOriPath = Path.Combine(fullAlbumPath, src.AlRelPath!);

                    var fullDstPath = toWebp ? $"{Path.Combine(Path.GetDirectoryName(fullOriPath)!, Path.GetFileNameWithoutExtension(fullOriPath))}.webp" : fullOriPath;

                    using(var client = new HttpClient())
                    using(var form = new MultipartFormDataContent()) {
                        form.Add(new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(processorApiParam)), "paramJson");

                        var fileContent = new ByteArrayContent(_io.ReadFile(fullOriPath));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                        form.Add(fileContent, "file", Path.GetFileName(fullOriPath));

                        var response = await client.PostAsync($"{_config.ProcessorApiUrl}/Image/UpscaleCompress", form);

                        if(response.IsSuccessStatusCode) {
                            var responseStream = response.Content.ReadAsStream();

                            _io.DeleteFile(fullOriPath);

                            using(var fileStream = new FileStream(fullDstPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                                await responseStream.CopyToAsync(fileStream);
                            }

                            var alRelDstPath = Path.GetRelativePath(fullAlbumPath, fullDstPath);

                            var cm = GetFileCorrectionModel(new FileInfo(Path.Combine(fullAlbumPath, alRelDstPath)), fullAlbumPath);

                            report[i] = new() {
                                AlRelPath = src.AlRelPath,
                                AlRelDstPath = alRelDstPath,
                                Success = true,
                                Height = cm.Height,
                                Width = cm.Width,
                                Byte = cm.Byte,
                                BytesPer100Pixel = cm.BytesPer100Pixel,
                            };

                            await _hubContext.Clients.Group(operationId).SendAsync("ReceiveProgress", new SignalrMessage<FileCorrectionReportModel> {
                                MessageType = SignalrMessageType.Progress,
                                Data = report[i],
                            });
                        }
                        else {
                            var message = await response.Content.ReadAsStringAsync();
                            report[i] = new() {
                                AlRelPath = src.AlRelPath,
                                AlRelDstPath = Path.GetRelativePath(fullAlbumPath, fullDstPath),
                                Success = false,
                                Message = $"Code: {response.StatusCode} | {message}"
                            };

                            await _hubContext.Clients.Group(operationId).SendAsync("ReceiveProgress", new SignalrMessage<FileCorrectionReportModel> {
                                MessageType = SignalrMessageType.Progress,
                                Data = report[i],
                            });
                        }
                    }
                }
                catch(Exception e) {
                    report[i] = new() {
                        AlRelPath = src.AlRelPath,
                        Success = false,
                        Message = e.Message
                    };

                    await _hubContext.Clients.Group(operationId).SendAsync("ReceiveProgress", new SignalrMessage<FileCorrectionReportModel> {
                        MessageType = SignalrMessageType.Progress,
                        Data = report[i]
                    });
                }
            });

            if(report.All(a => a.Success)) {
                if(param.Type == 0) {
                    _logDb.UpsertAlbumCorrection(new AlbumCorrection {
                        LibRelPath = param.LibRelPath,
                        CorrectedPage = fileCount,
                        BatchStartDate = batchStart,
                        CorrectionFinishDate = DateTime.Now,
                    });
                }
                else
                    _logDb.UpsertCorrectionLog(param.LibRelPath, DateTime.Now, 0);
            }

            if((LibraryType)param.Type == LibraryType.Regular) {
                _library.RecountAlbumPages(param.LibRelPath);
                _library.DeleteAlbumCache(param.LibRelPath);
            }

            #region To delete
            //_logger.Information($"Corrected {fileCount} pages in {sw.Elapsed.TotalSeconds} secs");

            //Parallel.For(0, report.Length, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, (i, state) => {
            //    var src = report[i];

            //    if(src.Success) {
            //        var cm = GetFileCorrectionModel(new FileInfo(Path.Combine(fullAlbumPath, src.AlRelDstPath!)), fullAlbumPath);

            //        src.Height = cm.Height;
            //        src.Width = cm.Width;
            //        src.Byte = cm.Byte;
            //        src.BytesPer100Pixel = cm.BytesPer100Pixel;
            //    }
            //});
            #endregion

            await _hubContext.Clients.Group(operationId).SendAsync("ReceiveProgress", new SignalrMessage<FileCorrectionReportModel> {
                MessageType = SignalrMessageType.Complete,
                Message = $"Corrected {fileCount} pages in {sw.Elapsed.TotalSeconds} secs",
            });

            return report;
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.CorrectPages{Environment.NewLine}" +
                $"Params=[{System.Text.Json.JsonSerializer.Serialize(param)}]{Environment.NewLine}" +
                $"{e}");

            await _hubContext.Clients.Group(operationId).SendAsync("ReceiveProgress", new SignalrMessage<FileCorrectionReportModel> {
                MessageType = SignalrMessageType.Error,
                Message = e.Message,
            });

            throw;
        }
        finally {
            _activeOperations.TryRemove(operationId, out _);
        }
    }

    FileCorrectionModel GetFileCorrectionModel(FileInfo fileInfo, string fullAlbumPath) {
        using(var img = SixLabors.ImageSharp.Image.Load(fileInfo.FullName)) {
            return new FileCorrectionModel {
                AlRelPath = Path.GetRelativePath(fullAlbumPath, fileInfo.FullName),
                Extension = fileInfo.Extension,
                Byte = fileInfo.Length,
                ModifiedDate = fileInfo.LastWriteTime,
                Height = img.Height,
                Width = img.Width,
                IsAnimated = img.Frames.Count > 1
            };
        }
    }
    #endregion

    #region OCR
    public OcrResult TranscribeAndTranslateImage(Stream stream, string ext) {
        var tempFilePath = Path.Combine(_config.TempPath, "_ocr", $"{Guid.NewGuid().ToString()}{ext}");

        var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Executables", "OcrJp.py");

        try {
            //write fileStream to tempFilePath
            using(var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                stream.CopyTo(fileStream);
            }

            ProcessStartInfo start = new ProcessStartInfo {
                FileName = "C:\\Python311\\python.exe",
                Arguments = $"\"{scriptPath}\" \"{tempFilePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,   // Capture standard output
                RedirectStandardError = true,    // Capture standard error (for logs)
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            // Set environment variable for UTF-8 encoding
            start.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

            var (output, error) = new Func<(string, string)>(() => {
                using(Process process = Process.Start(start)!) {
                    // Read the output (OCR result) from the Python script
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();  // Capture the error stream (logs)

                    process.WaitForExit();

                    return (output, error);
                }
            })();

            if(string.IsNullOrWhiteSpace(output)) {
                _logger.Warning("TranscribeAndTranslateImage | " + error);
                return new OcrResult {
                    English = "[OCR output is empty] " + error,
                    Original = "",
                    Romanized = "",

                    Error = error
                };
            }

            var translated = _google.TranslateDetectLanguageToEnglish(output);

            return new OcrResult {
                English = translated,
                Original = output,
                Romanized = "[Not Implemented]",

                Error = error
            };
        }
        catch(Exception e) {
            _logger.Error($"TranscribeAndTranslateImage{Environment.NewLine}" +
                $"Params=[{tempFilePath}]{Environment.NewLine}" +
                $"{e}");

            return new OcrResult {
                English = e.Message,
                Original = "",
                Romanized = "",

                Error = e.Message
            };
        }
    }
    #endregion
}