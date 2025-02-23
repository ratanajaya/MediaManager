using CoreAPI.AL.Models.Sc;
using CoreAPI.AL.Services;
using CoreAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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

    [HttpPost]
    public IActionResult CorrectPagesSignalr(CorrectPageParam param) {
        // Generate a unique operationId
        var operationId = Guid.NewGuid().ToString();

        // Start the CorrectPagesAsync operation without awaiting
        Task.Run(async () =>
        {
            await _os.CorrectPagesSignalr(operationId, param);
        });

        // Return the operationId immediately
        return Ok(operationId);
    }
    #endregion

    #region OCR
    [HttpPost]
    //NOTE: Swagger will fail to generate documentation due to [FromForm]
    public IActionResult TranscribeAndTranslateImage([FromForm] IFormFile file) {
        if(file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        return Ok(_os.TranscribeAndTranslateImage(file.OpenReadStream(), System.IO.Path.GetExtension(file.FileName)));
    }
    #endregion
}
