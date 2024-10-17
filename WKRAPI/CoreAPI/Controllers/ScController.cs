using CoreAPI.AL.Models;
using CoreAPI.AL.Models.Config;
using CoreAPI.AL.Models.Dto;
using CoreAPI.AL.Models.Sc;
using CoreAPI.AL.Services;
using CoreAPI.Controllers.Abstraction;
using CoreAPI.CustomMiddlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class ScController : ControllerBase, ILibraryController
{
    AuthorizationService _auth;
    ScRepository _sc;
    CoreApiConfig _config;

    public ScController(AuthorizationService auth, ScRepository sc, CoreApiConfig config) {
        _auth = auth;
        _sc = sc;
        _config = config;
    }

    #region Album Query
    [HttpGet]
    public IActionResult GetAlbumCardModels(int page, int row, string query, bool excludeCorrected) {
        var libRelPath = query != null ? query : "";

        var data = _config.IsPrivate
            ? _sc.GetAlbumVMs(libRelPath)
            : new List<ScAlbumVM>().AsEnumerable();

        var result = data
            .Select(e => new AlbumCardModel { 
                Path = e.LibRelPath,
                Title = e.Name,
                LastPageIndex = e.LastPageIndex,
                PageCount = e.PageCount,

                Languages = new List<string>(),
                Tier = 0,
                IsRead = true,
                IsWip = false,
                CoverInfo = e.CoverInfo
            })
            .ToList();

        return Ok(result);
    }

    [HttpGet]
    public IActionResult GetLibraryDirNodes(string path, bool includeChild) {
        return Ok(_sc.GetLibraryDirNodes(path ?? string.Empty, includeChild));
    }
    #endregion

    #region Album Command
    [HttpPut]
    public IActionResult UpdateAlbumTier(AlbumTierParam param) {
        return Ok("Success");
    }

    [HttpGet]
    public IActionResult RecountAlbumPages(string path) {
        return Ok(_sc.CountAlbumPages(path));
    }

    [HttpPost]
    public IActionResult UpdateAlbumOuterValue(AlbumOuterValueParam param) {
        _sc.UpdateAlbumOuterValue(param.AlbumPath, param.LastPageIndex);
        return Ok(param.AlbumPath);
    }

    [HttpGet]
    public IActionResult RefreshAlbum(string path) {
        _sc.DeleteAlbumCache(path);
        return Ok("Success");
    }

    #endregion

    #region Page Query
    [HttpGet]
    public IActionResult GetAlbumFsNodeInfo(string path, bool includeDetail, bool includeDimension) {
        return Ok(_sc.GetAlbumFsNodeInfo(path, includeDetail, includeDimension));
    }
    #endregion

    #region Page Command
    [HttpDelete]
    public IActionResult DeleteFile(string path, string alRelPath) {
        _auth.DisableRouteOnPublicBuild();

        return Ok(_sc.DeleteFile(path, alRelPath));
    }

    [HttpPost]
    public IActionResult MoveFile(FileMoveParamModel param) {
        _auth.DisableRouteOnPublicBuild();

        return Ok(_sc.MoveFile(param));
    }

    [HttpDelete]
    public IActionResult DeleteAlbumChapter(string path, string chapterName) {
        _auth.DisableRouteOnPublicBuild();

        return Ok(_sc.DeleteAlbumChapter(path, chapterName));
    }
    #endregion

    #region UNUSED
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult UpdateAlbum(AlbumVM albumVM) {
        throw new NotImplementedException();
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult GetAlbumVM(string path) {
        throw new NotImplementedException();
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult GetGenreCardModels() {
        throw new NotImplementedException();
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult GetFeaturedArtistCardModels() {
        throw new NotImplementedException();
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult DeleteAlbum(string path) {
        throw new NotImplementedException();
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    public Task ReloadDatabase() {
        throw new NotImplementedException();
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    public Task RescanDatabase() {
        throw new NotImplementedException();
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    public Task QuickScan() {
        throw new NotImplementedException();
    }
    #endregion
}