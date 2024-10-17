using CoreAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Authorize]
[ApiController]
[Route("Censorship")]
public class CensorshipController : ControllerBase
{
    CensorshipService _cs;

    public CensorshipController(CensorshipService cs) {
        _cs = cs;
    }

    [HttpGet]
    public IActionResult Get() {
        return Ok(_cs.IsCensorshipOn());
    }

    [HttpPost]
    public IActionResult Post(bool status) {
        _cs.UpdateCensorshipStatus(status);
        return Ok();
    }
}