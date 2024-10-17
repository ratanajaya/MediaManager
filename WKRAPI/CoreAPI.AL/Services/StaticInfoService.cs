using SharedLibrary;
using SharedLibrary.Models;
using System.Collections.Generic;

namespace CoreAPI.AL.Services;

public class StaticInfoService
{
    AlbumInfoProvider _ai;

    public StaticInfoService(AlbumInfoProvider ai) {
        _ai = ai;
    }

    public AlbumInfoVm GetAlbumInfoVm() {
        return new AlbumInfoVm {
            Povs = _ai.Povs,
            Focuses = _ai.Focuses,
            Others = _ai.Others,
            Rares = _ai.Rares,
            Qualities = _ai.Qualities,

            Characters = _ai.Characters,
            Categories = _ai.Categories,
            Orientations = _ai.Orientations,
            Languages = _ai.Languages,
            SuitableImageFormats = _ai.SuitableImageFormats,
            SuitableVideoFormats = _ai.SuitableVideoFormats
        };
    }

    public List<KeyValuePair<int, string>> GetUpscalers() {
        return new List<KeyValuePair<int, string>> {
            new((int)UpscalerType.Waifu2xCunet, "Waifu2x Cunet"),
            new((int)UpscalerType.Waifu2xAnime, "Waifu2x Anime"),

            new((int)UpscalerType.SrganD2fkJpeg, "SRGAN D2FK Jpeg"),

            new((int)UpscalerType.RealesrganX4plus, "RealESRGAN X4 Plus"),
            new((int)UpscalerType.RealesrganX4plusAnime, "RealESRGAN X4 Plus Anime"),
        };
    }
}
