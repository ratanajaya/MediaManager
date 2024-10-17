using CoreAPI.AL.Models;
using CoreAPI.AL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class ExtraInfoController : ControllerBase
{
    ExtraInfoService _ei;

    public ExtraInfoController(ExtraInfoService ei) {
        _ei = ei;
    }

    [HttpGet]
    public IActionResult GetComments(string url) {
        return Ok(_ei.GetComments(url));
    }

    [HttpPost]
    public IActionResult ScrapeComment(string url) {
        return Ok(_ei.ScrapeComment(url));
    }

    [HttpPost]
    public IActionResult UpsertSourceAndContent(SourceAndContentUpsertModel param) {
        _ei.UpsertSourceAndContent(param);
        return Ok();
    }

    [HttpPost]
    public IActionResult DeleteSource(SourceDeleteModel param) {
        _ei.DeleteSource(param);
        return Ok();
    }

    [HttpPost]
    public IActionResult UpdateSource(SourceUpdateModel param) {
        _ei.UpdateSource(param);
        return Ok();
    }
}