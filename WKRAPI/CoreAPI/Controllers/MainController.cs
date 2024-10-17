using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoreAPI.AL.Services;
using CoreAPI.AL.Models;
using CoreAPI.CustomMiddlewares;
using CoreAPI.Controllers.Abstraction;
using System.IO;
using CoreAPI.Services;
using Microsoft.AspNetCore.Authorization;
using CoreAPI.AL.Models.Dto;
using CoreAPI.AL.Models.Config;
using SharedLibrary.Models;

namespace CoreAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class MainController : ControllerBase, ILibraryController
{
    AuthorizationService _auth;
    LibraryRepository _library;
    FileRepository _file;
    AlbumInfoProvider _ai;
    CoreApiConfig _config;
    CensorshipService _cs;

    public MainController(AuthorizationService auth, LibraryRepository library, FileRepository file, AlbumInfoProvider ai, CoreApiConfig config, CensorshipService cs) {
        _auth = auth;
        _library = library;
        _file = file;
        _ai = ai;
        _config = config;
        _cs = cs;
    }

    #region Private Methods
    (FileInfoModel fi, int count) GetCoverPageInfo(string query) {
        (var cpi, var count) = _file.GetRandomCoverPageInfoByQuery(query);

        return (cpi, count);
    }
    #endregion

    #region AlbumInfo
    [HttpGet]
    public IActionResult GetGenreCardModels() {
        var data = _ai.GenreQueries.GroupBy(gq => gq.Group)
            .Select(gr => new AlbumCardGroup {
                Name = gr.Key.ToString(),
                AlbumCms = gr.Select(gi => {
                    (var fi, var count) = GetCoverPageInfo(gi.Query);

                    return new AlbumCardModel {
                        Path = gi.Query,
                        Title = gi.Name,
                        CoverInfo = fi,
                        IsRead = true,
                        IsWip = false,
                        Languages = new List<string>(),
                        LastPageIndex = 0,
                        PageCount = count,
                        Tier = 0,
                    };
                }).ToList()
            })
            .ToList();

        return Ok(_cs.ConCensorAlbumCardGroups(data));
    }

    [HttpGet]
    public IActionResult GetFeaturedArtistCardModels() {
        var albumCMs = _ai.Tier1Artists.Select(a => {
            (var fi, var count) = GetCoverPageInfo($"artist={a}");
            return new AlbumCardModel {
                Path = $"artist={a}",
                Title = a,
                CoverInfo = fi,
                PageCount = count,
                #region Useless fields
                Note = "",
                IsRead = true,
                IsWip = false,
                Languages = new List<string>(),
                LastPageIndex = 0,
                Tier = 0
                #endregion
            };
        })
        .OrderBy(a => a.Title)
        .ToList();

        var data = new List<AlbumCardGroup>{
            new AlbumCardGroup {
                Name = $"Featured Artists",
                AlbumCms = albumCMs
            }
        };

        return Ok(_cs.ConCensorAlbumCardGroups(data));
    }

    [HttpGet]
    public IActionResult GetFeaturedCharacterCardModels() {
        var albumCMs = _ai.Characters.Select(a => {
            (var fi, var count) = GetCoverPageInfo($"character={a}");
            return new AlbumCardModel {
                Path = $"character={a}",
                Title = a,
                CoverInfo = fi,
                PageCount = count,
                #region Useless fields
                Note = "",
                IsRead = true,
                IsWip = false,
                Languages = new List<string>(),
                LastPageIndex = 0,
                Tier = 0
                #endregion
            };
        })
        .OrderBy(a => a.Title)
        .ToList();

        var data = new List<AlbumCardGroup>{
            new AlbumCardGroup {
                Name = $"Featured Characters",
                AlbumCms = albumCMs
            }
        };

        return Ok(_cs.ConCensorAlbumCardGroups(data));
    }
    #endregion

    #region Album Query
    [HttpGet]
    public IActionResult GetAlbumVM(string path) {
        var decensoredPath = _cs.ConDecensorPath(path);
        var data = _library.GetAlbumVM(decensoredPath);

        return Ok(_cs.ConCensorAlbumVM(data));
    }

    [HttpGet]
    public IActionResult GetAlbumCardModels(int page, int row, string query, bool excludeCorrected) {
        var cleanQuery = HttpUtility.UrlDecode(query);
        var decensoredQuery = _cs.ConDecensorQuery(cleanQuery);

        var albumVms = _library.GetAlbumVMs(page, row, decensoredQuery, excludeCorrected);

        var data = albumVms.Select(a => new AlbumCardModel {
            Path = a.Path,
            Title = a.Album.Title,
            ArtistDisplay = a.Album.GetArtistsDisplay(),
            Languages = a.Album.Languages,
            IsRead = a.Album.IsRead,
            IsWip = a.Album.IsWip,
            Tier = a.Album.Tier,
            Note = a.Album.Note,
            LastPageIndex = a.LastPageIndex,
            PageCount = a.PageCount,
            EntryDate = a.Album.EntryDate,
            HasSource = a.Album.Sources.Any(),
            CoverInfo = a.CoverInfo
        })
        .ToList();

        return Ok(_cs.ConCensorAlbumCardModels(data));
    }
    #endregion

    #region Album Command
    [HttpPost]
    public IActionResult UpdateAlbum(AlbumVM albumVM) {
        _auth.DisableRouteOnPublicBuild();

        return Ok(_library.UpdateAlbum(albumVM));
    }

    [HttpPost]
    public IActionResult UpdateAlbumOuterValue(AlbumOuterValueParam param) {
        if(_config.IsPublic || _cs.IsCensorshipOn()) return Ok();

        param.LastPageIndex = _config.IsPublic ? 0 : param.LastPageIndex;
        return Ok(_library.UpdateAlbumOuterValue(param.AlbumPath, param.LastPageIndex));
    }

    [HttpPut]
    public IActionResult UpdateAlbumTier(AlbumTierParam param) {
        _auth.DisableRouteOnPublicBuild();

        _library.UpdateAlbumTier(param.AlbumPath, param.Tier);
        return Ok("Success");
    }

    [HttpGet]
    public IActionResult RecountAlbumPages(string path) {
        _auth.DisableRouteOnPublicBuild();

        return Ok(_library.RecountAlbumPages(path));
    }

    [HttpGet]
    public IActionResult RefreshAlbum(string path) {
        if(_config.IsPublic || _cs.IsCensorshipOn()) return Ok();

        _library.RecountAlbumPages(path);
        _library.DeleteAlbumCache(path);
        return Ok("Success");
    }

    [HttpDelete]
    public IActionResult DeleteAlbum(string path) {
        _auth.DisableRouteOnPublicBuild();

        _library.DeleteAlbum(path);
        _library.DeleteAlbumCache(path);
        return Ok("Success");
    }
    #endregion

    #region Page Query
    [HttpGet]
    public IActionResult GetAlbumFsNodeInfo(string path, bool includeDetail, bool includeDimension) {
        var decensoredPath = _cs.ConDecensorPath(path);

        var result = _file.GetAlbumFsNodeInfo(decensoredPath, includeDetail, includeDimension);

        var censoredResult = _cs.ConCensorAlbumFsNodeInfo(result);

        return Ok(censoredResult);
    }
    #endregion

    #region Page Command
    [HttpPost]
    public IActionResult MoveFile(FileMoveParamModel param) {
        _auth.DisableRouteOnPublicBuild();

        return Ok(_file.MoveFile(param));
    }

    [HttpDelete]
    public IActionResult DeleteFile(string path, string alRelPath) {
        _auth.DisableRouteOnPublicBuild();

        var libRelPath = Path.Combine(path, alRelPath);
        return Ok(_file.DeleteFile(libRelPath));
    }

    [HttpDelete]
    public IActionResult DeleteAlbumChapter(string path, string chapterName) {
        _auth.DisableRouteOnPublicBuild();

        return Ok(_library.DeleteAlbumChapter(path, chapterName));
    }

    [HttpPost]
    public IActionResult UpdateAlbumChapter(ChapterUpdateParamModel param) {
        //should only be used to update Tier
        _auth.DisableRouteOnPublicBuild();

        _file.UpdateAlbumChapter(param);

        return Ok("Success");
    }
    #endregion
}