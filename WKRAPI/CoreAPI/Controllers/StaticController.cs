using CoreAPI.AL.Services;
using CoreAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class StaticController(
    StaticInfoService _si,
    CensorshipService _cs
    ) : ControllerBase
{
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