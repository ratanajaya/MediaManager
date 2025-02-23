using CoreAPI.AL.DataAccess;
using CoreAPI.AL.Models;
using CoreAPI.AL.Models.Config;
using CoreAPI.AL.Models.Dto;
using CoreAPI.AL.Models.Sc;
using Serilog;
using SharedLibrary;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreAPI.AL.Services;

public class ScRepository(
     CoreApiConfig _config,
    ISystemIOAbstraction _io,
    AlbumInfoProvider _ai,
    ILogger _logger,
    MediaProcessor _media
    )
{
    #region Sc Db
    ScDbModel? _scDb;

    private void InitializeScDb() {
        if(_scDb != null) return;
        _scDb = _io.IsFileExists(_config.ScFullAlbumDbPath)
            ? _io.DeserializeJsonSync<ScDbModel>(_config.ScFullAlbumDbPath)
            : new ScDbModel();
    }

    private ScMetadataModel GetScAlbumMetadata(string albumPath) {
        InitializeScDb();

        var scMetadata = _scDb!.ScMetadatas.FirstOrDefault(m => m.Path == albumPath);
        if(scMetadata != null) return scMetadata;

        var newScMetadata = new ScMetadataModel {
            Path = albumPath,
            LastPageIndex = 0,
            LastPageAlRelPath = null
        };
        _scDb.ScMetadatas.Add(newScMetadata);
        SaveChanges();

        return newScMetadata;
    }

    private void SaveChanges() {
        InitializeScDb();
        _io.SerializeToJson(_config.ScFullAlbumDbPath, _scDb!);
    }
    #endregion

    #region Query
    public IEnumerable<ScAlbumVM> GetAlbumVMs(string libRelPath) {
        FileInfoModel GetCoverInfo(string? path) {
            var finalPath = path ?? _config.ScFullDefaultThumbnailPath;

            var fi = new FileInfo(finalPath);
            var libRelPath = Path.GetRelativePath(_config.ScLibraryPath, finalPath);

            return new FileInfoModel {
                Extension = fi.Extension,
                Name = fi.Name,
                Size = fi.Length,
                LibRelPath = libRelPath
            };
        }

        try {
            var fullPath = Path.Combine(_config.ScLibraryPath, libRelPath);
            bool isSubdirsAlbum = new Func<bool>(() => {
                var count = string.IsNullOrEmpty(libRelPath) ? 0 : libRelPath.Split(Path.DirectorySeparatorChar).Count();

                return count == _config.ScLibraryDepth;
            })();

            var subDirs = _io.GetDirectories(fullPath);

            return subDirs
                .Where(e => e != _config.ScFullExtraInfoPath)
                .Select(e => {
                    var libRelAlbumPath = Path.GetRelativePath(_config.ScLibraryPath, e);
                    var filePaths = _io.GetSuitableFilePathsWithNaturalSort(e, _ai.SuitableFileFormats, 1);
                    var metadata = GetScAlbumMetadata(libRelAlbumPath);

                    return new ScAlbumVM {
                        Name = Path.GetFileName(e),
                        LibRelPath = libRelAlbumPath,
                        LastPageIndex = metadata.LastPageIndex,
                        LastPageAlRelPath = metadata.LastPageAlRelPath,
                        PageCount = isSubdirsAlbum ? filePaths.Count : 0,
                        CoverInfo = GetCoverInfo(filePaths.FirstOrDefault())
                    };
                });
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.GetAlbumVMs{Environment.NewLine}" +
                $"Params=[{libRelPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public AlbumFsNodeInfo GetAlbumFsNodeInfo(string libRelAlbumPath, bool includeDetail = false, bool includeDimension = false) {
        try {
            var fullAlbumPath = Path.Combine(_config.ScLibraryPath, libRelAlbumPath);

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
                            Tier = 0,
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

            bool trueIncludeDimension = includeDimension;

            Parallel.For(0, allFileNodes.Count, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (i, state) => {
                allFileNodes[i].FileInfo = _media.GenerateFileInfo(fullAlbumPath, allFilePaths[i], includeDetail, trueIncludeDimension);
            });

            var rootFileNodes = new List<FsNode>();
            allFileNodes.ForEach(fs => {
                //check if fs.AlRelPath is not located in a directory
                for(int i = 0; i < subDirNodes.Count; i++) {
                    if(fs.AlRelPath.StartsWith(subDirNodes[i].AlRelPath)) {
                        subDirNodes[i].DirInfo!.Childs!.Add(fs);
                        return;
                    }
                }
                rootFileNodes.Add(fs);
            });

            var fsNodes = rootFileNodes.Concat(subDirNodes).ToList();

            return new AlbumFsNodeInfo {
                FsNodes = fsNodes,
                Title = Path.GetFileName(fullAlbumPath),
                Orientation = Constants.Orientation.Auto
            };
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.GetAlbumFsNodeInfo{Environment.NewLine}" +
                $"Params=[{libRelAlbumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public List<FsNode> GetLibraryDirNodes(string libRelAlbumPath, bool includeChild = false) {
        List<FsNode> GetFsNodes(string path, bool includeChild) {
            var subDirs = _io.GetDirectories(path);

            return subDirs
                .Where(e => e != _config.ScFullCachePath)
                .Select(e => new FsNode {
                    AlRelPath = Path.GetRelativePath(_config.ScLibraryPath, e),
                    NodeType = NodeType.Folder,
                    DirInfo = new() {
                        Name = Path.GetRelativePath(_config.ScLibraryPath, e),
                        Childs = includeChild ? GetFsNodes(e, includeChild) : null
                    }
                })
                .ToList();
        }

        try {
            var fullAlbumPath = Path.Combine(_config.ScLibraryPath, libRelAlbumPath);

            var result = GetFsNodes(fullAlbumPath, includeChild);

            return result;
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.GetLibraryDirNodes{Environment.NewLine}" +
                $"Params=[{libRelAlbumPath},{includeChild}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }
    #endregion

    #region Command
    public string DeleteFile(string libRelAlbumPath, string alRelPath) {
        try {
            var fullAlbumPath = Path.Combine(_config.ScLibraryPath, libRelAlbumPath);
            var targetPath = Path.Combine(fullAlbumPath, alRelPath);

            if(!_io.IsPathExist(targetPath)) return "Path does not exist";

            _io.DeleteFile(targetPath);
            DeleteAllPageCache(targetPath);

            return "Success";
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.DeleteFile{Environment.NewLine}" +
                $"Params=[{libRelAlbumPath},{alRelPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public int DeleteAlbumChapter(string libRelAlbumPath, string subDir) {
        try {
            var chapterPath = Path.Combine(_config.ScLibraryPath, libRelAlbumPath, subDir);

            _io.DeleteDirectory(chapterPath);

            return CountAlbumPages(libRelAlbumPath);
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.DeleteAlbumChapter{Environment.NewLine}" +
                $"Params=[{libRelAlbumPath},{subDir}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public int CountAlbumPages(string libRelAlbumPath) {
        try {
            var fullAlbumPath = Path.Combine(_config.ScLibraryPath, libRelAlbumPath);

            var suitableFilePaths = _io.GetSuitableFilePaths(fullAlbumPath, _ai.SuitableFileFormats, 1);

            return suitableFilePaths.Count;
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.CountAlbumPages{Environment.NewLine}" +
                $"Params=[{libRelAlbumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public void UpdateAlbumOuterValue(string albumPath, int lastPageIndex, string? lastPageAlRelPath) {
        try {
            var scMetadata = GetScAlbumMetadata(albumPath);

            scMetadata.LastPageIndex = lastPageIndex;
            scMetadata.LastPageAlRelPath = lastPageAlRelPath;

            SaveChanges();
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.CountAlbumPages{Environment.NewLine}" +
                $"Params=[{albumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public void DeleteAlbumCache(string albumPath) {
        try {
            var sizeFolders = _io.GetDirectories(_config.ScFullCachePath);
            foreach(var sizeFolder in sizeFolders) {
                var fullALbumCachePath = Path.Combine(sizeFolder, albumPath);
                _io.DeleteDirectory(fullALbumCachePath);
            }
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.DeleteAlbumCache{Environment.NewLine}" +
                $"Params=[{albumPath}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public string MoveFile(FileMoveParamModel param) {
        try {
            var srcLibRelPath = Path.Combine(param.Src.AlbumPath, param.Src.AlRelPath);
            var dstLibRelPath = Path.Combine(param.Dst.AlbumPath, param.Dst.AlRelPath);

            var srcFullPath = Path.Combine(_config.ScLibraryPath, srcLibRelPath);
            var dstFullPath = Path.Combine(_config.ScLibraryPath, dstLibRelPath);

            if(_io.IsDirectoryExist(srcFullPath)) {
                if(_io.IsDirectoryExist(dstFullPath)) {
                    throw new Exception("Unable to overwrite folder");
                }
                _io.MoveDirectory(srcFullPath, dstFullPath);
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

            DeleteAllPageCache(srcFullPath);
            DeleteAllPageCache(dstFullPath);

            return (JsonSerializer.Serialize(new {
                message = "Success"
            }));
        }
        catch(Exception e) {
            _logger.Error($"ScRepository.DeleteAlbumCache{Environment.NewLine}" +
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