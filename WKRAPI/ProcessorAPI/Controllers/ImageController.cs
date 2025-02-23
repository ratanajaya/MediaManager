using Microsoft.AspNetCore.Mvc;
using SharedLibrary;
using SharedLibrary.Models;
using System.Drawing;
using System.Text.Json;

namespace ProcessorAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ImageController(
    ImageProcessor _ip,
    ProcessorApiConfig _config,
    Serilog.ILogger _logger
    ) : ControllerBase
{
    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    //NOTE: Swagger will fail to generate documentation due to [FromForm]
    public IActionResult UpscaleCompress([FromForm] string paramJson, [FromForm] IFormFile file) {
    //public IActionResult UpscaleCompress(string paramJson, IFormFile file) {
        try {
            var param = JsonSerializer.Deserialize<UpscaleCompressApiParam>(paramJson);

            if(param == null)
                return BadRequest("Invalid parameter");

            if(file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            int[] possibleUpscaleMultipliers = new List<UpscalerType> { UpscalerType.Waifu2xCunet, UpscalerType.Waifu2xAnime }.Contains(param.UpscalerType)
                    ? [2, 4, 8]
                    : [4, 8];

            Func<string, string, int, UpscalerType, int?, string> upscaleMethod = new List<UpscalerType> { UpscalerType.Waifu2xCunet, UpscalerType.Waifu2xAnime }.Contains(param.UpscalerType)
                    ? _ip.UpscaleImageWaifu2x
                    : UpscalerType.SrganD2fkJpeg == param.UpscalerType
                    ? _ip.UpscaleImageRealSr
                    : _ip.UpscaleImageRealEsrGan;

            #region Directory Setup
            var originalDirPath = Path.Combine(_config.TemDirPath, "original");
            var upscaledDirPath = Path.Combine(_config.TemDirPath, "upscaled");
            var finalDirPath = Path.Combine(_config.TemDirPath, "final");
            //Don't programatically create these directories, make sure they're already there

            var guid = Guid.NewGuid().ToString();
            var stagingName = $"{guid}{param.Extension}";

            var originalFilePath = Path.Combine(originalDirPath, stagingName);
            var upscaledFilePath = Path.Combine(upscaledDirPath, stagingName);
            var finalFilePath = Path.Combine(finalDirPath, param.ToWebp ? $"{guid}.webp" : stagingName);
            #endregion

            using(var stream = new FileStream(originalFilePath, FileMode.Create)) {
                file.CopyTo(stream);
            }

            if(param.CorrectionType == FileCorrectionType.Upscale) {
                var multiplier = possibleUpscaleMultipliers
                               .Where(num => num >= param.UpscaleMultiplier.GetValueOrDefault())
                               .DefaultIfEmpty(possibleUpscaleMultipliers.Max())
                               .Min();

                var outputTxt = upscaleMethod(originalFilePath, upscaledFilePath, multiplier, param.UpscalerType, _config.TileSize);

                if(!System.IO.File.Exists(upscaledFilePath)) {
                    return StatusCode(500, $"Upscaled file does not exist | {outputTxt}");
                }
            }

            var fileToCompressPath = param.CorrectionType == FileCorrectionType.Upscale ? upscaledFilePath : originalFilePath;

            var mimeType = param.ToWebp ? SupportedMimeType.WEBP : SupportedMimeType.ORIGINAL;
            var compressOutput = _ip.CompressImage(fileToCompressPath, finalFilePath,
                param.Compression!.Quality, new Size(param.Compression.Width, param.Compression.Height),
                mimeType);

            System.IO.File.Delete(originalFilePath);
            System.IO.File.Delete(upscaledFilePath);

            if(!System.IO.File.Exists(finalFilePath)) {
                return StatusCode(500, $"Compressed file does not exist | {compressOutput}");
            }

            var bytes = System.IO.File.ReadAllBytes(finalFilePath);

            System.IO.File.Delete(finalFilePath);
            return File(bytes, "application/octet-stream", Path.GetFileName(finalFilePath));
        }
        catch(Exception ex) {
            _logger.Error(ex, $"UpscaleCompress | {paramJson}");

            return StatusCode(500, ex.Message);
        }
    }
}
