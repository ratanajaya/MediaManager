using CoreAPI.AL.DataAccess;
using CoreAPI.AL.Models;
using CoreAPI.AL.Services;
using CoreAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary;
using System.Linq;

namespace CoreAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class StaticController : ControllerBase
{
    StaticInfoService _si;
    CensorshipService _cs;

    public StaticController(StaticInfoService si, CensorshipService cs) {
        _si = si;
        _cs = cs;
    }

    [HttpGet]
    public IActionResult GetAlbumInfo() {
        var data = _si.GetAlbumInfoVm();

        var censoredData = _cs.ConCensorAlbumInfoVm(data);

        return Ok(censoredData);
    }

    [HttpGet]
    public IActionResult GetUpscalers() {
        return Ok(_si.GetUpscalers());
    }
}