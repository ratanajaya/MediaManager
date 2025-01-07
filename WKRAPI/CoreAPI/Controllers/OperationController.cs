using CoreAPI.AL.Models.Sc;
using CoreAPI.AL.Services;
using CoreAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class OperationController(
    OperationService _os
    ) : ControllerBase
{
    #region Correction
    [HttpPost]
    public IActionResult HScanCorrectiblePaths(HScanCorrectiblePathParam param) {
        return Ok(_os.HScanCorrectiblePages(param.Paths, param.Thread, param.UpscaleTarget));
    }

    [HttpGet]
    public IActionResult ScGetCorrectablePaths() {
        return Ok(_os.ScGetCorrectablePaths());
    }

    [HttpPost]
    public IActionResult ScFullScanCorrectiblePaths(int thread, int upscaleTarget) {
        return Ok(_os.ScFullScanCorrectiblePaths(thread, upscaleTarget));
    }

    [HttpGet]
    public IActionResult GetCorrectablePages(int type, string path, int thread, int upscaleTarget, bool clampToTarget) {
        return Ok(_os.GetCorrectablePages(type, path, thread, upscaleTarget, clampToTarget));
    }

    [HttpPost]
    public async Task<IActionResult> CorrectPages(CorrectPageParam param) {
        return Ok(await _os.CorrectPages(param));
    }
    #endregion

    #region OCR
    [HttpPost]
    public IActionResult TranscribeAndTranslateImage([FromForm] IFormFile file) {
        if(file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        return Ok(_os.TranscribeAndTranslateImage(file.OpenReadStream(), System.IO.Path.GetExtension(file.FileName)));
    }
    #endregion
}
