using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharedLibrary;
using System.Threading.Tasks;
using CoreAPI.AL.Services;
using SharedLibrary.Helpers;
using MetadataEditor.Models;
using SharedLibrary.Models;
using CoreAPI.AL.Models;
using System.Text;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Diagnostics;

namespace MetadataEditor.Services;
#pragma warning disable CA1416

public class AppLogic(
    AlbumInfoProvider _ai,
    ISystemIOAbstraction _io,
    LibraryRepository _library,
    FileRepository _file,
    ExtraInfoService _ei,
    ImageProcessor _ip,
    CoreAPI.AL.Models.Config.CoreApiConfig _apiConf
    )
{
    #region QUERY
    public async Task<AlbumViewModel> GetAlbumViewModelAsync(string path, Album oldAlbum) {
        AlbumViewModel result = new AlbumViewModel {
            Album = _io.IsFileExists(Path.Combine(path, Constants.FileSystem.JsonFileName))
                ? (await _io.DeserializeJson<Album>(Path.Combine(path, Constants.FileSystem.JsonFileName)))!
                : CreateStarterAlbumAsync(path, oldAlbum),
            Path = path,
            AlbumFiles = await GetAllFilesAsync(path)
        };
            
        return result;
    }

    Album CreateStarterAlbumAsync(string path, Album oldAlbum) {
        var subDirNames = Directory.GetDirectories(path).Select(a => new DirectoryInfo(a).Name).ToList();

        string folderName = new DirectoryInfo(path).Name;

        (var title, var artists, var languages, var isWip) = AlbumHelper.InferMetadataFromName(folderName, subDirNames);

        return new Album {
            Title = title,
            Category = !string.IsNullOrEmpty(oldAlbum?.Category) 
                ? oldAlbum.Category 
                : _ai.Categories.First(),
            Orientation = !string.IsNullOrEmpty(oldAlbum?.Orientation) 
                ? oldAlbum.Orientation 
                : Constants.Orientation.Portrait,

            Artists = artists,
            Povs = oldAlbum?.Povs != null ? oldAlbum.Povs : [],
            Focuses = oldAlbum?.Focuses != null ? oldAlbum.Focuses : [],
            Others = oldAlbum?.Others != null ? oldAlbum.Others : [],
            Rares = oldAlbum?.Rares != null ? oldAlbum.Rares : [],
            Qualities = oldAlbum?.Qualities != null ? oldAlbum.Qualities : [],
            Languages = languages,

            Tier = 0,

            IsWip = isWip,
            IsRead = false,

            EntryDate = DateTime.Now
        };
    }

    private async Task<List<string>> GetAllFilesAsync(string path) {
        var subDirTask = Task.Run(() => _io.GetDirectories(path)
            .Where(subDir => !subDir.Contains("[[Temp"))
            .ToArray());

        var result = _io.GetFiles(path)
            .Where(a => _ai.SuitableFileFormats.Contains(Path.GetExtension(a)))
            .OrderByAlphaNumeric(a => a)
            .ToList();

        string[] filteredSubDirs = await subDirTask;

        var resultFromSubDirs = await Task.WhenAll(filteredSubDirs.Select(a => GetAllFilesAsync(a)));

        return result.Concat(resultFromSubDirs.SelectMany(a => a)).ToList();
    }

    public async Task<List<SourceAndContent>> GetOrInitializeSourceAndContents(string path) {
        var jsonPath = Path.Combine(path, Constants.FileSystem.SourceAndContentFileName);
        if(!_io.IsFileExists(jsonPath))
            return new();

        var snc = await _io.DeserializeJsonCamelCase<SourceAndContent>(jsonPath);
        return new() { snc! };
    }
    #endregion

    #region COMMAND
    public async Task<string> SaveAlbumJson(AlbumViewModel vm) {
        try {
            await Task.Run(() => _io.SerializeToJson(Path.Combine(vm.Path!, Constants.FileSystem.JsonFileName), vm.Album));

            return "Success";
        }
        catch (Exception e) {
            return "Failed | " + e.ToString();
        }
    }

    public string PostAlbumMetadataOffline(AlbumViewModel vm) {
        string originalFolder = new DirectoryInfo(vm.Path).Name;
        return _library.UpdateAlbumMetadata(originalFolder, vm.Album);
    }

    public async Task<string> PostAlbumAndImmediatelyDelete(AlbumViewModel vm) {
        string originalFolder = new DirectoryInfo(vm.Path).Name;
        string libRelPath = await _library.InsertAlbum(originalFolder, vm.Album);

        _library.DeleteAlbum(libRelPath, true);

        return libRelPath;
    }

    public async Task<string> PostAlbumJsonOffline(AlbumViewModel vm, List<SourceAndContent> sncs, IProgress<FileDisplayModel> progress) {
        string originalFolder = new DirectoryInfo(vm.Path).Name;
        try {
            string albumId = await _library.InsertAlbum(originalFolder, vm.Album);

            foreach(string filePath in vm.AlbumFiles) {
                var reportModel = new FileDisplayModel {
                    Path = filePath,
                    UploadStatus = "Uploading..",
                    CorrectionModel = new()
                };
                progress.Report(reportModel);

                try {
                    string fileName = Path.GetFileName(filePath);
                    string subDir = filePath.Replace(vm.Path, "").Replace(fileName, "").Replace("\\", "");
                    var fileBytes = _io.ReadFile(filePath);

                    await _file.InsertFileToAlbum(albumId, subDir, fileName, fileBytes);
                    reportModel.UploadStatus = "Success";
                }
                catch(Exception e) {
                    reportModel.UploadStatus = e.Message;
                }

                progress.Report(reportModel);
            }
            int pageCount = _library.RecountAlbumPages(albumId);

            foreach(var snc in sncs) {
                _ei.UpsertSourceAndContent(new() {
                    AlbumPath = albumId,
                    SourceAndContent = snc
                });
            }

            return albumId;
        }
        catch(Exception e) {
            return "Failed | " + e.ToString();
        }
    }

    public AlbumViewModel RenameAlbumPath(AlbumViewModel src, string newFolderName) {
        _io.MoveDirectory(src.Path, newFolderName);
        src.AlbumFiles = src.AlbumFiles.Select(a => a.Replace(src.Path, newFolderName)).ToList();
        src.Path = newFolderName;
        return src;
    }

    public async Task<(List<FileDisplayModel>, FileCorrectionReportModel[])> CorrectPages(string rootPath, List<FileCorrectionModel> fileToCorrectList, int upscaleTarget, bool clampToTarget, IProgress<string> progress) {
        var trueThread = 2;
        var upscalerType = UpscalerType.Waifu2xCunet;

        var fileCount = fileToCorrectList.Count;

        var fileList = new Func<FileCorrectionModel[]>(() => {
            int cStart = fileToCorrectList.FindIndex(a => a.CorrectionType == FileCorrectionType.Compress);
            if(cStart == fileCount - 1 || cStart == -1)
                return fileToCorrectList.ToArray();

            int uStart = 0;
            int cCount = fileCount - cStart;
            int uCount = fileCount - cCount;

            var sortedArr = new FileCorrectionModel[fileCount];
            for(int i = 0; i < fileCount; i++) {
                if(i % 2 == 0) {
                    if(uStart < uCount) {
                        sortedArr[i] = fileToCorrectList[uStart];
                        uStart++;
                    }
                    else {
                        sortedArr[i] = fileToCorrectList[cStart];
                        cStart++;
                    }
                }
                else {
                    if(cStart < fileCount) {
                        sortedArr[i] = fileToCorrectList[cStart];
                        cStart++;
                    }
                    else {
                        sortedArr[i] = fileToCorrectList[uStart];
                        uStart++;
                    }
                }
            }

            return sortedArr;
        })();

        var report = new FileCorrectionReportModel[fileCount];

        int[] possibleUpscaleMultipliers = new List<UpscalerType> { UpscalerType.Waifu2xCunet, UpscalerType.Waifu2xAnime }.Contains(upscalerType)
                ? new int[] { 2, 4, 8 }
                : new int[] { 4, 8 };

        Func<string, string, int, UpscalerType, int?, string> upscaleMethod = new List<UpscalerType> { UpscalerType.Waifu2xCunet, UpscalerType.Waifu2xAnime }.Contains(upscalerType)
                ? _ip.UpscaleImageWaifu2x
                : UpscalerType.SrganD2fkJpeg == upscalerType
                ? _ip.UpscaleImageRealSr
                : _ip.UpscaleImageRealEsrGan;

        progress.Report("Starting correction...");

        await Parallel.ForAsync(0, fileCount, new ParallelOptions { MaxDegreeOfParallelism = trueThread }, async (i, state) => {
            var messageSb = new StringBuilder();
            var src = fileList[i];
            var toJpegExcludingWebp = src.Extension == Constants.Extension.Webp ? false : true;
            try {
                if(src.CorrectionType == null) {
                    report[i] = new() {
                        AlRelPath = src.AlRelPath,
                        AlRelDstPath = src.AlRelPath,
                        Success = true
                    };

                    progress.Report(src.AlRelPath + " | ~");
                    return;
                }

                var processorApiParam = new UpscaleCompressApiParam {
                    UpscaleMultiplier = src.UpscaleMultiplier,
                    UpscalerType = upscalerType,
                    ToJpeg = toJpegExcludingWebp,

                    CorrectionType = src.CorrectionType.Value,
                    Compression = src.Compression,
                    Extension = src.Extension!
                };

                var fullOriPath = Path.Combine(rootPath, src.AlRelPath!);
                var fullDstPath = toJpegExcludingWebp ? $"{Path.Combine(Path.GetDirectoryName(fullOriPath)!, Path.GetFileNameWithoutExtension(fullOriPath))}.jpeg" : fullOriPath;

                using(var client = new HttpClient())
                using(var form = new MultipartFormDataContent()) {
                    var sw = new Stopwatch();
                    sw.Start();

                    form.Add(new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(processorApiParam)), "paramJson");

                    // Replace with your file path
                    var fileContent = new ByteArrayContent(_io.ReadFile(fullOriPath));
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    form.Add(fileContent, "file", Path.GetFileName(fullOriPath));

                    var response = await client.PostAsync($"{_apiConf.ProcessorApiUrl}/Image/UpscaleCompress", form);

                    if(response.IsSuccessStatusCode) {
                        var responseStream = response.Content.ReadAsStream();

                        _io.DeleteFile(fullOriPath);

                        using(var fileStream = new FileStream(fullDstPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                            await responseStream.CopyToAsync(fileStream);
                        }

                        report[i] = new() {
                            AlRelPath = src.AlRelPath,
                            AlRelDstPath = Path.GetRelativePath(rootPath, fullDstPath),
                            Success = true
                        };

                        progress.Report($"{src.AlRelPath} | {sw.Elapsed.TotalSeconds.ToString("F1")}");
                    }
                    else {
                        var message = await response.Content.ReadAsStringAsync();
                        report[i] = new() {
                            AlRelPath = src.AlRelPath,
                            AlRelDstPath = Path.GetRelativePath(rootPath, fullDstPath),
                            Success = false,
                            Message = $"{src.AlRelPath} | Code: {response.StatusCode} | {message}"
                        };

                        progress.Report($"{src.AlRelPath} | Code: {response.StatusCode} | {message}");
                    }
                }
            }
            catch(Exception e) {
                report[i] = new() {
                    AlRelPath = src.AlRelPath,
                    Success = false,
                    Message = messageSb.Append(" | " + e.Message).ToString()
                };

                progress.Report(messageSb.ToString());
            }
        });

        var dstPathForSuccessFiles = report.Where(a => a.Success)
            .Select(a => Path.Combine(rootPath, a.AlRelDstPath!))
            .ToList();

        var displayModelForSuccessFiles = GetFileDisplayModels(rootPath, dstPathForSuccessFiles, upscaleTarget, clampToTarget);

        return (displayModelForSuccessFiles, report);
    }

    private FileCorrectionModel GetFileCorrectionModel(string filePath, string folderPath, int upscaleTarget, bool clampToTarget) {
        var fileInfo = new FileInfo(filePath);

        var newFcm = new FileCorrectionModel {
            AlRelPath = Path.GetRelativePath(folderPath, fileInfo.FullName),
            Extension = fileInfo.Extension,
            Byte = fileInfo.Length,
            ModifiedDate = fileInfo.LastWriteTime,

            CorrectionType = null,
        };

        var isFileCorrectable = _ai.CorrectableImageFormats.Contains(fileInfo.Extension);
        if(!isFileCorrectable)
            return newFcm;

        using(var img = SixLabors.ImageSharp.Image.Load(fileInfo.FullName)) {
            newFcm.Height = img.Height;
            newFcm.Width = img.Width;

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
                    null //Don't clamp to target if compression is the only operation
                );
            }

            return newFcm;
        }
    }

    public List<FileDisplayModel> GetFileDisplayModels(string folderPath, List<string> filePaths, int upscaleTarget, bool clampToTarget) {
        return filePaths.Select(path => {
            return new FileDisplayModel {
                Path = path,
                FileNameDisplay = Path.GetFileName(path),
                SubDirDisplay = Path.GetDirectoryName(path)!.Replace(folderPath, ""),
                UploadStatus = "-",
                CorrectionModel = GetFileCorrectionModel(path, folderPath, upscaleTarget, clampToTarget),
            };
        }).ToList();
    }
    #endregion
}