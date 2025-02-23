using CoreAPI.AL.Helpers;
using CoreAPI.AL.Models;
using CoreAPI.AL.Models.Config;
using CoreAPI.AL.Models.Dto;
using Serilog;
using SharedLibrary;
using SharedLibrary.Helpers;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreAPI.AL.DataAccess;

public interface IDbContext
{
    List<AlbumVM> AlbumVMs { get; }

    void AddAlbumVM(AlbumVM newEntity);
    void RemoveAlbumVM(AlbumVM existing);
    bool SaveChanges();
    Task Reload(Func<EventStreamData, Task> report);
    Task Rescan(Func<EventStreamData, Task> report);

    FileInfoModel GetFirstFileInfo(List<string> filePaths);
}

/// <summary>
/// WARNING This IDbContext implementation may not be thread safe (untested) and doesn't guarantee ACID transaction. 
/// If this app will ever be made public, an actual RDBMS implemnentation will be done beforehand.
/// </summary>
public class JsonDbContext(
    CoreApiConfig _config,
    AlbumInfoProvider _ai,
    ISystemIOAbstraction _io,
    ILogger _logger,
    IFlagDbContext _flagDb
    ) : IDbContext
{
    List<AlbumVM>? _albumVMs;

    public bool SaveChanges() {
        if(_albumVMs == null) {
            return false;
        }

        _io.SerializeToMsgpack(_config.FullAlbumDbPath, _albumVMs);
        _flagDb.UpdateLastModified();

        return true;
    }

    public List<AlbumVM> AlbumVMs { 
        get {
            if(_albumVMs == null || !_flagDb.IsLastModifiedByThisApp()) {
                _albumVMs = Task.Run(() => LoadAlbumVMs()).GetAwaiter().GetResult();
                _flagDb.UpdateLastModified();
            }
            return _albumVMs;
        }
    }

    public void AddAlbumVM(AlbumVM newEntity) {
        if(_albumVMs == null)
            throw new Exception("AlbumVMs is null");

        _albumVMs.Add(newEntity);
    }

    public void RemoveAlbumVM(AlbumVM existing) {
        if(_albumVMs == null)
            throw new Exception("AlbumVMs is null");

        _albumVMs.Remove(existing);
    }

    private async Task<List<AlbumVM>> LoadAlbumVMs(bool rescan = false) {
        List<AlbumVM>? result = null;
        if(!rescan) {
            try {
                result = await _io.DeserializeMsgpack<List<AlbumVM>>(_config.FullAlbumDbPath);
            }
            catch(Exception e) {
                _logger.Error("LoadAlbumVMs() - " + e.Message);
            }
        }

        if(result == null) {
            result = await LoadAlbumVMsRecursive(_config.LibraryPath);
            _io.SerializeToMsgpack(_config.FullAlbumDbPath, result);
        }

        return result;
    }

    private async Task<List<AlbumVM>> LoadAlbumVMsRecursive(string folderPath) {
        var result = new List<AlbumVM>();

        var metadataFilePath = Path.Combine(folderPath, Constants.FileSystem.JsonFileName);

        if(_io.IsFileExists(metadataFilePath)) {
            var album = await _io.DeserializeJson<Album>(metadataFilePath);

            if(album == null)
                throw new Exception("Unable to deserialize into album: " + metadataFilePath);

            var suitableFilePaths = _io.GetSuitableFilePaths(folderPath, _ai.SuitableFileFormats, 1);

            var coverInfo = GetFirstFileInfo(suitableFilePaths);

            result.Add(new AlbumVM {
                Path = Path.GetRelativePath(_config.LibraryPath, folderPath),
                PageCount = suitableFilePaths.Count,
                LastPageIndex = 0,
                LastPageAlRelPath = null,
                CoverInfo = coverInfo,
                Album = album
            });
        }

        string[] subDirs = _io.GetDirectories(folderPath);
        foreach(string subDir in subDirs) {
            result.AddRange(await LoadAlbumVMsRecursive(subDir));
        }

        return result;
    }

    public FileInfoModel GetFirstFileInfo(List<string> filePaths) {
        var coverPath = filePaths.FirstOrDefault();
        return coverPath != null ?
            new FileInfoModel {
                Name = Path.GetFileName(coverPath),
                LibRelPath = Path.GetRelativePath(_config.LibraryPath, coverPath)
            }
            : new FileInfoModel {
                Name = "No Cover",
                LibRelPath = "No Cover"
            };
    }


    public async Task Reload(Func<EventStreamData, Task> report) {
        int maxStep = 1;
        var sw = new Stopwatch();
        await report(new EventStreamData(maxStep, 0, $"{sw.Elapsed.Format()} - Task Starting"));
        sw.Start();
        _albumVMs = await LoadAlbumVMs();
        await report(new EventStreamData(maxStep, 1, $"{sw.Elapsed.Format()} - LoadAlbumVMs finished"));
    }

    public async Task Rescan(Func<EventStreamData, Task> report) {
        int maxStep = 2;
        var sw = new Stopwatch();
        await report(new EventStreamData(maxStep, 0, $"{sw.Elapsed.Format()} - Task Starting"));
        sw.Start();

        var oldAlbumVMs = await LoadAlbumVMs();

        _io.DeleteFile(_config.FullAlbumDbPath);

        await report(new EventStreamData(maxStep, 1, $"{sw.Elapsed.Format()} - Delete Existing Db finished"));

        var newAlbumVms = await LoadAlbumVMs(true);
        _albumVMs = newAlbumVms.Select(a => {
            var oldAlbumVM = oldAlbumVMs.GetOrDefault(a.Path);

            a.LastPageIndex = (oldAlbumVM?.LastPageIndex).GetValueOrDefault();
            a.LastPageAlRelPath = oldAlbumVM?.LastPageAlRelPath;
            return a;
        }).ToList();

        SaveChanges();

        await report(new EventStreamData(maxStep, 2, $"{sw.Elapsed.Format()} - LoadAlbumVMs finished"));
    }

    private List<string> LoadAlbumPathsRecursive(string folderPath) {
        if(_io.IsFileExists(Path.Combine(folderPath, Constants.FileSystem.JsonFileName))) {
            var librelAlbumPath = Path.GetRelativePath(_config.LibraryPath, folderPath);
            return new List<string> { librelAlbumPath };
        }
        else {
            var subDirs = _io.GetDirectories(folderPath);

            var result = new List<string>();
            foreach(var dir in subDirs) {
                result.AddRange(LoadAlbumPathsRecursive(dir));
            }
            return result;
        }
    }
}