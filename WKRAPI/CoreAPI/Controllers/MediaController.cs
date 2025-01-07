using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using CoreAPI.AL.Services;
using SharedLibrary.Enums;
using MimeTypes;
using CoreAPI.Services;
using CoreAPI.AL.Models.Config;

namespace CoreAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class MediaController(
    CoreApiConfig _config,
    FileRepository _file,
    CensorshipService _cs
    ) : ControllerBase
{
    [HttpGet]
    public IActionResult StreamPage(string libRelPath, LibraryType type) {
        var decensoredLibRelPath = _cs.ConDecensorLibRelMediaPath(libRelPath);

        string fullPath = Path.Combine(_config.GetLibraryPath(type), decensoredLibRelPath);

        return PhysicalFile(fullPath, MimeTypeMap.GetMimeType(Path.GetExtension(fullPath)), true);
    }

    [HttpGet]
    public async Task<IActionResult> StreamResizedImage(string libRelPath, int maxSize, LibraryType type) {
        var decensoredLibRelPath = _cs.ConDecensorLibRelMediaPath(libRelPath);

        string fullPath = await _file.GetFullCachedPath(decensoredLibRelPath, maxSize, type);

        return PhysicalFile(fullPath, MimeTypeMap.GetMimeType(Path.GetExtension(fullPath)), true);
    }
}