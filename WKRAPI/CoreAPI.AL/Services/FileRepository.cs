using SharedLibrary.Enums;
using CoreAPI.AL.DataAccess;
using CoreAPI.AL.Models;
using Serilog;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CoreAPI.AL.Helpers;
using SharedLibrary.Helpers;
using CoreAPI.AL.Models.Dto;
using CoreAPI.AL.Models.Config;
using SharedLibrary.Models;

namespace CoreAPI.AL.Services;

public class FileRepository(
    CoreApiConfig _config,
    AlbumInfoProvider _ai,
    ISystemIOAbstraction _io,
    IDbContext _db,
    LibraryRepository _lib,
    ILogger _logger,
    MediaProcessor _media
    )
{
    #region QUERY
    public async Task<string> GetFullCachedPath(string libRelOriginalPagePath, int maxSize, LibraryType type) {
        try {
            var fullCachedPagePath = Path.Combine(_config.GetCachePath(type), maxSize.ToString(), libRelOriginalPagePath + ".jpg");
            if(_io.IsFileExists(fullCachedPagePath)) { return fullCachedPagePath; }
            _io.CreateDirectory(Path.GetDirectoryName(fullCachedPagePath)!);

            var libPath = _config.GetLibraryPath(type);
            var fullOriginalPagePath = Path.Combine(libPath, libRelOriginalPagePath);

            if(_ai.IsImage(fullOriginalPagePath)) {
                _media.GenerateResizedImage(fullOriginalPagePath, fullCachedPagePath, maxSize);
            }
            else if(_ai.IsVideo(fullOriginalPagePath)) {
                var vidThumbnailDir = Path.Combine(_config.FullPageCachePath, "Vid");
                _io.CreateDirectory(vidThumbnailDir);

                var tempThumbnailPath = Path.Combine(vidThumbnailDir, Guid.NewGuid().ToString() + ".jpg");

                await _media.GenerateVideothumbnail(fullOriginalPagePath, fullCachedPagePath, tempThumbnailPath, maxSize);
            }
            else {
                _logger.Warning($"GetFullCachedPath | [{libRelOriginalPagePath}] | extension invalid");
            }

            return fullCachedPagePath;
        }
        catch(Exception e) {
            _logger.Error($"FileRepository.GetFullCachedPath{Environment.NewLine}" +
                $"Params=[{libRelOriginalPagePath},{maxSize},{type}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    //Hardcoded to only read 2 levels of file system hierarcy
    public AlbumFsNodeInfo GetAlbumFsNodeInfo(string albumPath, bool includeDetail = false, bool includeDimension = false) {
        try {
            var albumVm = _db.AlbumVMs.First(a => a.Path == albumPath);

            var fullAlbumPath = Path.Combine(_config.LibraryPath, albumPath);

            var subDirs = _io.GetDirectories(fullAlbumPath)
                .OrderByAlphaNumeric(d => Path.GetRelativePath(fullAlbumPath, d))
                .ToList();

            var subDirNodes = subDirs                
                .Select(d => {
                    var dirInfo = new DirectoryInfo(d);

                    return new FsNode {
                        NodeType = NodeType.Folder,
                        AlRelPath = Path.GetRelativePath(fullAlbumPath, d),
                        DirInfo = new() {
                            Name = dirInfo.Name,
                            Tier = albumVm.GetChapterTier(dirInfo.Name),
                        }
                    };
                }).ToList();

            var allFilePaths = _io.GetSuitableFilePathsWithNaturalSort(fullAlbumPath, _ai.SuitableFileFormats, 1);
            var allFileNodes = allFilePaths.Select(f => {
                return new FsNode {
                    NodeType = NodeType.File,
                    AlRelPath = Path.GetRelativePath(fullAlbumPath, f),
                };
            }).ToList();

            bool trueIncludeDimension = includeDimension || albumVm.Album.Orientation == "Auto";

            Parallel.For(0, allFileNodes.Count, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (i, state) => {
                allFileNodes[i].FileInfo = _media.GenerateFileInfo(fullAlbumPath, allFilePaths[i], includeDetail, trueIncludeDimension);
            });

            var rootFileNodes = new List<FsNode>();
            allFileNodes.ForEach(fs => { 
                //check if fs.AlRelPath is not located in a directory
                for(int i = 0; i < subDirNodes.Count; i++) {
                    if(fs.AlRelPath.StartsWith(subDirNodes[i].AlRelPath)) {
                        subDirNodes[i].DirInfo!.Childs.Add(fs);
                        return;
                    }
                }
                rootFileNodes.Add(fs);
            });

            var fsNodes = rootFileNodes.Concat(subDirNodes).ToList();

            return new AlbumFsNodeInfo {
                FsNodes = fsNodes,
                Title = albumVm.Album.GetFullTitleDisplay(),
                Orientation = albumVm.Album.Orientation
            };
        }
        catch(Exception e) {
            _logger.Error($"FileRepository.GetAlbumFsNodeInfo{Environment.NewLine}" +
                $"Params=[{albumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    [Obsolete]
    public AlbumPageInfo GetAlbumPageInfos(string albumPath, bool includeDetail = false, bool includeDimension = false) {
        try {
            var albumVm = _db.AlbumVMs.Get(albumPath);

            var fullAlbumPath = Path.Combine(_config.LibraryPath, albumPath);
            if(!_io.IsDirectoryExist(fullAlbumPath)) {
                _lib.DeleteAlbum(albumPath);

                throw new Exception("Album does not exist");
            }

            var allFilePaths = _io.GetSuitableFilePathsWithNaturalSort(fullAlbumPath, _ai.SuitableFileFormats, 1);

            bool trueIncludeDimension = includeDimension || albumVm.Album.Orientation == "Auto";

            var fileInfos = new FileInfoModel[allFilePaths.Count];
            Parallel.For(0, allFilePaths.Count, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (i, state) => {
                fileInfos[i] = _media.GenerateFileInfo(fullAlbumPath, allFilePaths[i], includeDetail, trueIncludeDimension);
            });

            return new AlbumPageInfo {
                FileInfos = fileInfos,
                Orientation = albumVm.Album.Orientation
            };
        }
        catch(Exception e) {
            _logger.Error($"FileRepository.GetAlbumPageInfos{Environment.NewLine}" +
                $"Params=[{albumPath},{includeDetail}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public (FileInfoModel fi, int count) GetRandomCoverPageInfoByQuery(string query) {
        #region Local Function
        FileInfoModel GetPageInfoByPath(string albumPath, int pageIndex) {
            var allFilePaths = _io.GetSuitableFilePathsWithNaturalSort(Path.Combine(_config.LibraryPath, albumPath), _ai.SuitableFileFormats, 1);
            if(allFilePaths.Count == 0) {
                return new FileInfoModel { 
                    Name = "No Cover",
                    LibRelPath = "No Cover"
                };
            }
            var targetFullPath = allFilePaths[pageIndex];

            return new FileInfoModel {
                Name = Path.GetFileName(targetFullPath),
                LibRelPath = Path.GetRelativePath(_config.LibraryPath, targetFullPath),
            };
        }
        #endregion

        var empty = new FileInfoModel {
            Extension = "",
            Name = "",
            Size = 0,
            LibRelPath = ""
        };

        try {
            var albumsOfQuery = _lib.GetAlbumVMs(0, 0, query).ToList();
            var count = albumsOfQuery.Count;

            if(count == 0) { return (empty, 0); }

            try {
                var now = DateTime.Now;
                var seed = now.Year + now.Month + now.Day;
                var index = seed % count;
                var albumForCover = albumsOfQuery[index];

                var result = GetPageInfoByPath(albumForCover.Path, 0);

                return (result, count);
            }
            catch(Exception e) {
                _logger.Error($"GetRandomCoverPageInfoByQuery inner try-catch " +
                    $"Params=[{query}]{Environment.NewLine}" +
                    $"{e}");

                return (empty, count);
            }
        }
        catch(Exception e) {
            _logger.Warning($"FileRepository.GetAlbumPageInfos{Environment.NewLine}" +
                $"Params=[{query}]{Environment.NewLine}" +
                $"{e}");

            return (empty, 0);
        }
    }

    #endregion

    #region COMMAND
    public Task InsertFileToAlbum(string albumPath, string subDir, string name, byte[] fileBytes) {
        var filePath = Path.Combine(_config.LibraryPath, albumPath, subDir, name);

        return _io.WriteFile(filePath, fileBytes);
    }

    public string MoveFile(FileMoveParamModel param) {
        try {
            if(param.Src.AlbumPath != param.Dst.AlbumPath)
                throw new Exception("Moving file between to different album is not allowed");

            var srcLibRelPath = Path.Combine(param.Src.AlbumPath, param.Src.AlRelPath);
            var dstLibRelPath = Path.Combine(param.Dst.AlbumPath, param.Dst.AlRelPath);

            var srcFullPath = Path.Combine(_config.LibraryPath, srcLibRelPath);
            var dstFullPath = Path.Combine(_config.LibraryPath, dstLibRelPath);

            if(_io.IsDirectoryExist(srcFullPath)) {
                if(_io.IsDirectoryExist(dstFullPath)) {
                    throw new Exception("Unable to overwrite folder");
                }
                UpdateAlbumChapter(new ChapterUpdateParamModel {
                    AlbumPath = param.Src.AlbumPath,
                    ChapterName = param.Src.AlRelPath,
                    NewChapterName = param.Dst.AlRelPath
                });

                return (JsonSerializer.Serialize(new {
                    message = "Success"
                }));
            }

            if(!_io.IsFileExists(srcFullPath)) {
                throw new Exception("Source file does not exist");
            }
            var targetExist = _io.IsFileExists(dstFullPath);
            if(!param.Overwrite && targetExist) {
                var srcInfo = new FileInfo(srcFullPath);
                var dstInfo = new FileInfo(dstFullPath);

                return (JsonSerializer.Serialize(new {
                    message = "Destination file already exist",
                    srcInfo = new {
                        name = srcInfo.Name,
                        size = srcInfo.Length,
                        createdDate = srcInfo.CreationTime
                    },
                    dstInfo = new {
                        name = dstInfo.Name,
                        size = dstInfo.Length,
                        createdDate = dstInfo.CreationTime
                    }
                }));//WARNING Magic string
            }
            else if(param.Overwrite && targetExist) {
                _io.DeleteFile(dstFullPath);
            }
            _io.MoveFile(srcFullPath, dstFullPath);

            DeleteAllPageCache(srcLibRelPath);
            DeleteAllPageCache(dstLibRelPath);

            if(param.Src.AlbumPath != param.Dst.AlbumPath) {
                _lib.RecountAlbumPages(param.Src.AlbumPath);
                _lib.RecountAlbumPages(param.Dst.AlbumPath);
                _db.SaveChanges();
            }

            return (JsonSerializer.Serialize(new {
                message = "Success"
            }));
        }
        catch(Exception e) {
            _logger.Error($"FileRepository.MoveFile{Environment.NewLine}" +
                $"Params=[{JsonSerializer.Serialize(param)}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public string DeleteFile(string libRelPath) {
        try {
            var fullTargetPath = Path.Combine(_config.LibraryPath, libRelPath);

            if(!_io.IsPathExist(fullTargetPath)) return "Path does not exist";

            _io.DeleteFileOrDirectory(fullTargetPath);
            DeleteAllPageCache(libRelPath);

            return "Success";
        }
        catch(Exception e) {
            _logger.Error($"FileRepository.DeleteFile{Environment.NewLine}" +
                $"Params=[{libRelPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public void UpdateAlbumChapter(ChapterUpdateParamModel param) {
        try {
            var targetAlbum = _db.AlbumVMs.Get(param.AlbumPath);
            var fullAlbumPath = Path.Combine(_config.LibraryPath, targetAlbum.Path);

            if(param.Tier.HasValue) {
                targetAlbum.UpdateChapterTier(param.ChapterName, param.Tier.Value);
            }
            if(!string.IsNullOrWhiteSpace(param.NewChapterName)) {
                _io.MoveDirectory(Path.Combine(fullAlbumPath, param.ChapterName), Path.Combine(fullAlbumPath, param.NewChapterName));

                targetAlbum.RenameChapter(param.ChapterName, param.NewChapterName);
            }

            _lib.SaveAlbumMetadata(targetAlbum, null);
            _db.SaveChanges();
        }
        catch(Exception e) {
            _logger.Error($"FileRepository.DeleteFile{Environment.NewLine}" +
                $"Params=[{JsonSerializer.Serialize(param)}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    private void DeleteAllPageCache(string libRelOriginalPagePath) {
        var sizeDirs = _io.GetDirectories(_config.FullPageCachePath);

        var fullCachePaths = sizeDirs.Select(e => Path.Combine(e, $"{libRelOriginalPagePath}.jpg")).ToList();

        foreach(var path in fullCachePaths) {
            _io.DeleteFileOrDirectory(path);
        }
    }
    #endregion
}