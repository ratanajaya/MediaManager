using CoreAPI.AL.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class PcController : ControllerBase {
    IPcService _pc;

    public PcController(IPcService pc) {
        _pc = pc;
    }

    [HttpPost]
    public IActionResult Sleep() {
        _pc.Sleep();
        return Ok();
    }

    [HttpPost]
    public IActionResult Hibernate() {
        _pc.Hibernate();
        return Ok();
    }
}
