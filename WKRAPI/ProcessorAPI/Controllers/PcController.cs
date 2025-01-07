using Microsoft.AspNetCore.Mvc;
using SharedLibrary;

namespace ProcessorAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class PcController(
    IPcService _pc,
    ISystemIOAbstraction _io
    ) : ControllerBase
{
    [HttpGet]
    public IActionResult Check() {
        var version = new Func<string>(() => {
            try {
                DateTime dllWriteTime = _io.GetLastWriteTime(Path.Combine(Directory.GetCurrentDirectory(), "ProcessorAPI.dll"));

                return dllWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch(Exception e) {
                return e.Message;
            }
        })();

        return Ok(new {
            Version = version
        });
    }

    [HttpPost]
    public IActionResult Sleep() {
        _pc.Sleep();
        return Ok();
    }

    [HttpPost]
    public IActionResult ShutDown() {
        _pc.Shutdown();
        return Ok();
    }
}