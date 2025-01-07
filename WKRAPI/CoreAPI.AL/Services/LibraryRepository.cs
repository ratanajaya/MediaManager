using CoreAPI.AL.DataAccess;
using CoreAPI.AL.Helpers;
using CoreAPI.AL.Models;
using CoreAPI.AL.Models.Config;
using CoreAPI.AL.Models.Dto;
using CoreAPI.AL.Models.LogDb;
using Serilog;
using SharedLibrary;
using SharedLibrary.Helpers;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Qh = CoreAPI.AL.Helpers.QueryHelpers;

namespace CoreAPI.AL.Services;

public class LibraryRepository (
    ILogger _logger,
    CoreApiConfig _config,
    AlbumInfoProvider _ai,
    ISystemIOAbstraction _io,
    IDbContext _db,
    ILogDbContext _logDb
    )
{
    #region Query
    public IEnumerable<AlbumVM> GetAlbumVMs(int page, int row, string? query, bool excludeCorrected = false) {
        var now = DateTime.Now;
        var querySegments = Qh.GetQuerySegments(query);
        DateTime? minRecentBatch = querySegments.Any(a => a.Key == "SPECIAL" && a.Val.Equals("Recent", StringComparison.OrdinalIgnoreCase)) 
            ? _db.AlbumVMs.Max(a => a.Album.EntryDate).AddDays(-1)
            : null;

        var filteredAlbum = _db.AlbumVMs.Where(a => Qh.MatchAllQueries(a.Album, querySegments, _ai.Tier1Artists.Concat(_ai.Tier2Artists).ToArray(), _ai.Characters, now, minRecentBatch));

        if(excludeCorrected) {
            var correctedPaths = _logDb.GetAlbumCorrections().Select(ac => ac.LibRelPath).ToList();
            filteredAlbum = filteredAlbum.Where(a => !correctedPaths.Contains(a.Path));
        }

        var pagedAlbum = (page > 0 && row > 0) ? filteredAlbum.Skip((page - 1) * row).Take(row) : filteredAlbum;

        return pagedAlbum;
    }
        
    public AlbumVM GetAlbumVM(string albumPath) {
        return _db.AlbumVMs.Get(albumPath);
    }
    #endregion

    #region Command
    public void SaveAlbumMetadata(AlbumVM albumVM, string? crudLog) {
        _io.SerializeToJson(Path.Combine(_config.LibraryPath, albumVM.Path, Constants.FileSystem.JsonFileName), albumVM.Album);
        if(crudLog != null)
            _logDb.InsertCrudLog(crudLog, albumVM);
    }

    public string UpdateAlbum(AlbumVM albumVM) {
        try {
            var existing = _db.AlbumVMs.Get(albumVM.Path);

            if(existing == null)
                throw new InvalidOperationException("Album with specified id not found");
            if(existing.Album.EntryDate != albumVM.Album.EntryDate)
                throw new InvalidOperationException("Update on album's EntryDate is forbidden");

            albumVM.Album.ValidateAndCleanup();

            existing.Album = albumVM.Album;

            SaveAlbumMetadata(existing, CrudLog.Update);
            _db.SaveChanges();

            return existing.Path;
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.UpdateAlbum{Environment.NewLine}" +
                $"Params=[{JsonSerializer.Serialize(albumVM)}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public string DeleteAlbum(string albumPath, bool insertCrudLog = true) {
        try {
            var existing = _db.AlbumVMs.Get(albumPath);

            if(existing == null)
                throw new InvalidOperationException("Album with specified id not found");

            DeleteAlbumFromFileSystem(albumPath);

            _db.RemoveAlbumVM(existing);
            _db.SaveChanges();

            _logDb.DeleteAlbumCorrection(albumPath);

            if(insertCrudLog)
                _logDb.InsertCrudLog(CrudLog.Delete, existing);

            return albumPath;
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.DeleteAlbum{Environment.NewLine}" +
                $"Params=[{albumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public void DeleteAlbumFromFileSystem(string albumPath) {
        var albumFullPath = Path.Combine(_config.LibraryPath, albumPath);

        DeleteAlbumCache(albumPath);
        _io.DeleteDirectory(albumFullPath);
    }

    public int DeleteAlbumChapter(string albumPath, string subDir) {
        try {
            var existing = _db.AlbumVMs.Get(albumPath);

            if(existing == null)
                throw new Exception("Album not found");

            var chapterPath = Path.Combine(_config.LibraryPath, existing.Path, subDir);

            _io.DeleteDirectory(chapterPath);

            int newPageCount = RecountAlbumPages(albumPath);

            return newPageCount;
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.DeleteAlbumChapter{Environment.NewLine}" +
                $"Params=[{albumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public string UpdateAlbumOuterValue(string albumPath, int lastPageIndex) {
        try {
            var existing = _db.AlbumVMs.Get(albumPath);

            existing.LastPageIndex = lastPageIndex == existing.PageCount - 1 ? 0 : lastPageIndex;

            if(!existing.Album.IsRead && lastPageIndex == existing.PageCount - 1) {
                existing.Album.IsRead = true;
                existing.LastPageIndex = 0;
                if(existing.Album.Note == "HP")
                    existing.Album.Note = "";
                SaveAlbumMetadata(existing, CrudLog.FirstRead);
            }

            _db.SaveChanges();
            return existing.Path;
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.UpdateAlbumOuterValue{Environment.NewLine}" +
                $"Params=[{albumPath},{lastPageIndex}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public string UpdateAlbumTier(string albumPath, int tier) {
        try {
            var existing = _db.AlbumVMs.Get(albumPath);

            existing.Album.Tier = tier;

            SaveAlbumMetadata(existing, CrudLog.Update);

            _db.SaveChanges();
            return existing.Path;
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.UpdateAlbumTier{Environment.NewLine}" +
                $"Params=[{albumPath},{tier}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public int RecountAlbumPages(string albumPath) {
        try {
            var targetAlbum = _db.AlbumVMs.Get(albumPath);
            var fullAlbumPath = Path.Combine(_config.LibraryPath, targetAlbum.Path);

            var suitableFilePaths = _io.GetSuitableFilePaths(fullAlbumPath, _ai.SuitableFileFormats, 1);

            targetAlbum.PageCount = suitableFilePaths.Count;
            targetAlbum.CoverInfo = _db.GetFirstFileInfo(suitableFilePaths);

            _db.SaveChanges();

            return targetAlbum.PageCount;
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.RecountAlbumPages{Environment.NewLine}" +
                $"Params=[{albumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public void DeleteAlbumCache(string albumPath) {
        try {
            if(string.IsNullOrWhiteSpace(albumPath))
                throw new InvalidOperationException("albumPath is null or empty");

            var sizeFolders = _io.GetDirectories(_config.FullPageCachePath);
            foreach(var sizeFolder in sizeFolders) {
                var fullALbumCachePath = Path.Combine(sizeFolder, albumPath);
                _io.DeleteDirectory(fullALbumCachePath);
            }
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.DeleteAlbumCache{Environment.NewLine}" +
                $"Params=[{albumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public async Task ReloadDatabase(Func<EventStreamData, Task> report) {
        try {
            await _db.Reload(report);
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.ReloadDatabase{Environment.NewLine}" +
                $"Params=[]" +
                $"{e}");

            throw;
        }
    }

    public async Task RescanDatabase(Func<EventStreamData,Task> report) {
        try {
            await _db.Rescan(report);
        }
        catch(Exception e) {
            _logger.Error($"LibraryRepository.RescanDatabase{Environment.NewLine}" +
                $"Params=[]" +
                $"{e}");

            throw;
        }
    }
    #endregion

    #region Command By MetadataEditor
    string GetLibRelAlbumLocation(string param) {
        var firstLetter = param.ToLower();
        return firstLetter == "a" ? "ABC" : firstLetter == "b" ? "ABC" : firstLetter == "c" ? "ABC" :
                firstLetter == "d" ? "DEF" : firstLetter == "e" ? "DEF" : firstLetter == "f" ? "DEF" :
                firstLetter == "g" ? "GHI" : firstLetter == "h" ? "GHI" : firstLetter == "i" ? "GHI" :
                firstLetter == "j" ? "JKL" : firstLetter == "k" ? "JKL" : firstLetter == "l" ? "JKL" :
                firstLetter == "m" ? "MNO" : firstLetter == "n" ? "MNO" : firstLetter == "o" ? "MNO" :
                firstLetter == "p" ? "PQR" : firstLetter == "q" ? "PQR" : firstLetter == "r" ? "PQR" :
                firstLetter == "s" ? "STU" : firstLetter == "t" ? "STU" : firstLetter == "u" ? "STU" :
                firstLetter == "v" ? "VWXYZ" : firstLetter == "w" ? "VWXYZ" : firstLetter == "x" ? "VWXYZ" :
                firstLetter == "y" ? "VWXYZ" : firstLetter == "z" ? "VWXYZ" :
                "000";
    }

    string GetFirstLetter(string source) {
        string normalizedSource = source.RemoveNonLetterDigit();
        return normalizedSource[0].ToString();
    }

    public async Task<string> InsertAlbum(string originalFolderName, Album album) {
        string firstLetter = GetFirstLetter(originalFolderName);
        string libRelSubDir = GetLibRelAlbumLocation(firstLetter);
        string libRelAlbumPath = Path.Combine(libRelSubDir, originalFolderName);

        DeleteAlbumFromFileSystem(libRelAlbumPath);
        await Task.Delay(20);

        _logDb.DeleteAlbumCorrection(libRelAlbumPath);

        var existing = _db.AlbumVMs.Get(libRelAlbumPath);

        if(existing != null) {
            ReplaceAlbumWhileKeepingSources(existing, album);
            _io.CreateDirectory(Path.Combine(_config.LibraryPath, libRelAlbumPath));

            _io.SerializeToJson(Path.Combine(_config.LibraryPath, libRelAlbumPath, Constants.FileSystem.JsonFileName), existing.Album);
            _db.SaveChanges();
            return existing.Path;
        }
        else {
            _io.CreateDirectory(Path.Combine(_config.LibraryPath, libRelAlbumPath));
            _io.SerializeToJson(Path.Combine(_config.LibraryPath, libRelAlbumPath, Constants.FileSystem.JsonFileName), album);

            var newAlbum = new AlbumVM {
                Album = album,
                Path = libRelAlbumPath,
                PageCount = 0,
                LastPageIndex = 0,
            };
            _db.AddAlbumVM(newAlbum);
            _db.SaveChanges();
            _logDb.InsertCrudLog(CrudLog.Insert, newAlbum);

            return newAlbum.Path;
        }
    }

    public string UpdateAlbumMetadata(string originalFolderName, Album album) {
        string firstLetter = GetFirstLetter(originalFolderName);
        string libRelSubDir = GetLibRelAlbumLocation(firstLetter);
        string libRelAlbumPath = Path.Combine(libRelSubDir, originalFolderName);

        var existing = _db.AlbumVMs.Get(libRelAlbumPath);
        if(existing == null) return "Album does not exist";

        ReplaceAlbumWhileKeepingSources(existing, album);

        _io.CreateDirectory(Path.Combine(_config.LibraryPath, libRelAlbumPath));
        _io.SerializeToJson(Path.Combine(_config.LibraryPath, libRelAlbumPath, Constants.FileSystem.JsonFileName), existing.Album);

        _db.SaveChanges();

        return existing.Path;
    }

    private void ReplaceAlbumWhileKeepingSources(AlbumVM albumVm, Album newAlbum) {
        var unchangedSources = albumVm.Album.Sources;
        albumVm.Album = newAlbum;
        albumVm.Album.Sources = unchangedSources;
    }
    #endregion
}