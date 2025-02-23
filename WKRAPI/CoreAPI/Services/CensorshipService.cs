using CoreAPI.AL;
using CoreAPI.AL.DataAccess;
using CoreAPI.AL.Models;
using CoreAPI.AL.Models.Config;
using CoreAPI.AL.Models.Dashboard;
using SharedLibrary.Helpers;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Qh = CoreAPI.AL.Helpers.QueryHelpers;

namespace CoreAPI.Services;

public class CensorshipService(
    IFlagDbContext _flagDb,
    CoreApiConfig _config
    )
{
    #region Public Db Methods
    public bool IsAccessible() {
        return _flagDb.IsAccessible();
    }

    public bool IsCensorshipOn() {
        return _flagDb.IsCensorshipOn();
    }

    public void UpdateCensorshipStatus(bool value) {
        _flagDb.UpdateCensorshipStatus(value);
    }
    #endregion

    #region Public Censorship Methods
    public AlbumInfoVm ConCensorAlbumInfoVm(AlbumInfoVm source) {
        if(!IsCensorshipOn()) return source;

        return new AlbumInfoVm {
            SuitableImageFormats = source.SuitableImageFormats,
            SuitableVideoFormats = source.SuitableVideoFormats,
            Languages = source.Languages,
            Orientations = source.Orientations,
            Categories = DiskCensorArray(source.Categories)
        };
    }

    public List<QueryVM> ConCensorQueryVms(List<QueryVM> source) {
        if(!IsCensorshipOn()) return source;

        var result = source.Select(a => new QueryVM {
            Tier = a.Tier,
            Name = a.Name.DiskCensor(),
            Query = CensorQuery(a.Query)
        }).ToList();
        return result;
    }

    public List<AlbumCardGroup> ConCensorAlbumCardGroups(List<AlbumCardGroup> source) {
        if(!IsCensorshipOn()) return source;

        var result = source.Select(a => new AlbumCardGroup {
            Name = a.Name,
            AlbumCms = CensorAlbumCardModels(a.AlbumCms, CensorQuery)
        }).ToList();

        return result;
    }

    public List<AlbumCardModel> ConCensorAlbumCardModels(List<AlbumCardModel> source) {
        if(!IsCensorshipOn()) return source;

        return CensorAlbumCardModels(source, PrimitiveHelper.DiskCensor);
    }

    public AlbumVM ConCensorAlbumVM(AlbumVM source) {
        if(!IsCensorshipOn()) return source;

        return new AlbumVM {
            Album = CensorAlbum(source.Album),
            Path = source.Path.DiskCensor(),
            CoverInfo = CensorFileInfo(source.CoverInfo),
            LastPageIndex = source.LastPageIndex,
            LastPageAlRelPath = source.LastPageAlRelPath?.DiskCensor(),
            PageCount = source.PageCount
        };
    }

    [Obsolete]
    public AlbumPageInfo ConCensorAlbumPageInfo(AlbumPageInfo source) {
        if(!IsCensorshipOn()) return source;

        var result = new AlbumPageInfo {
            Orientation = source.Orientation,
            FileInfos = source.FileInfos.Select(a => new FileInfoModel {
                CreateDate = a.CreateDate,
                UpdateDate = a.UpdateDate,
                Size = a.Size,
                Name = _config.DefaultThumbnailName,
                Extension = Path.GetExtension(_config.DefaultThumbnailName),
                LibRelPath = "PathNotUsed"
            }).ToArray()
        };

        return result;
    }

    public AlbumFsNodeInfo ConCensorAlbumFsNodeInfo(AlbumFsNodeInfo source) {
        if(!IsCensorshipOn()) return source;

        List<FsNode> CensorNewFsNodeRecursive(List<FsNode> source) {
            if(source.Count == 0) return source;

            return source;//TODO
        }

        var result = new AlbumFsNodeInfo {
            Title = source.Title.DiskCensor(),
            Orientation = source.Orientation,
            FsNodes = CensorNewFsNodeRecursive(source.FsNodes)
        };

        return result;
    }

    public string ConDecensorPath(string source) {
        if(!IsCensorshipOn()) return source;

        return source.DiskDecensor();
    }

    public string ConDecensorQuery(string source) {
        if(!IsCensorshipOn()) return source;

        return DecensorQuery(source);
    }

    public string ConDecensorLibRelMediaPath(string source) {
        if(!IsCensorshipOn()) return source;

        return _config.LibRelDefaultThumbnailPath;
    }

    public TierFractionModel ConCensorTierFractionModel(TierFractionModel source) {
        if(!IsCensorshipOn()) return source;

        return DecensorQueryFractionModel(source);
    }

    public List<TierFractionModel> ConCensorTierFractionModels(List<TierFractionModel> source) {
        if(!IsCensorshipOn()) return source;

        return source.Select(a => DecensorQueryFractionModel(a)).ToList();
    }

    public List<LogDashboardModel> ConCensorLogDashboardModels(List<LogDashboardModel> source) {
        if(!IsCensorshipOn()) return source;

        var result = source.Select(a => new LogDashboardModel {
            Album = a.Album != null ? CensorAlbum(a.Album) : null,
            AlbumFullTitle = a.AlbumFullTitle.DiskCensor(),
            Id = a.Id,
            Operation = a.Operation,
            CreationTime = a.CreationTime
        }).ToList();

        return result;
    }

    public ForceGraphData ConCensorTagForceGraphData(ForceGraphData source) {
        if(!IsCensorshipOn()) return source;

        var result = new ForceGraphData {
            Links = source.Links.Select(a => {
                var newLink = ObjectCloner.Clone(a);

                newLink.Source = newLink.Source.DiskCensor();
                newLink.Target = newLink.Target.DiskCensor();
                return newLink;
            }).ToList(),
            Nodes = source.Nodes.Select(a => {
                var newNode = ObjectCloner.Clone(a);

                newNode.Id = newNode.Id.DiskCensor();
                return newNode;
            }).ToList(),
        };

        return result;
    }
    #endregion

    #region Private Censorship Methods
    private string[] DiskCensorArray(string[] source) {
        return source.Select(a => a.DiskCensor()).ToArray();
    }
    private List<string> DiskCensorList(List<string> source) {
        return source.Select(a => a.DiskCensor()).ToList();
    }

    private string CensorQuery(string source) {
        var segments = Qh.GetQuerySegments(source);
        segments.ForEach(a => {
            a.Val = a.Val.DiskCensor();
        });

        return Qh.CombineQuerySegments(segments);
    }

    private string DecensorQuery(string source) {
        var segments = Qh.GetQuerySegments(source);
        segments.ForEach(a => {
            a.Val = a.Val.DiskDecensor();
        });

        return Qh.CombineQuerySegments(segments);
    }

    private FileInfoModel? CensorFileInfo(FileInfoModel? source) {
        if(source == null) return null;

        return new FileInfoModel { 
            Name = _config.DefaultThumbnailName,
            LibRelPath = _config.FullDefaultThumbnailPath,
            CreateDate = source.CreateDate,
            Extension = source.Extension,
            Size = source.Size,
            UpdateDate = source.UpdateDate,
        };
    }

    private Album CensorAlbum(Album source) {
        return new Album {
            Artists = DiskCensorList(source.Artists),
            Category = source.Category.DiskCensor(),
            Povs = DiskCensorList(source.Povs),
            Focuses = DiskCensorList(source.Focuses),
            Others = DiskCensorList(source.Others),
            Rares = DiskCensorList(source.Rares),
            Qualities = DiskCensorList(source.Qualities),
            Title = source.Title.DiskCensor(),
            ChapterTier = source.ChapterTier,
            EntryDate = source.EntryDate,
            IsRead = source.IsRead,
            IsWip = source.IsRead,
            Languages = source.Languages,
            Note = source.Note,
            Orientation = source.Orientation,
            Tier = source.Tier,
        };
    }

    private List<AlbumCardModel> CensorAlbumCardModels(List<AlbumCardModel> source, Func<string, string> pathCensorer) {
        return source.Select(a => new AlbumCardModel {
            ArtistDisplay = a.ArtistDisplay?.DiskCensor(),
            Title = a.Title.DiskCensor(),
            Path = pathCensorer(a.Path),
            CoverInfo = CensorFileInfo(a.CoverInfo),
            IsRead = a.IsRead,
            IsWip = a.IsWip,
            Languages = a.Languages,
            LastPageIndex = a.LastPageIndex,
            Note = a.Note,
            PageCount = a.PageCount,
            Tier = a.Tier
        }).ToList();
    }

    private TierFractionModel DecensorQueryFractionModel(TierFractionModel source) {
        return new TierFractionModel {
            Name = source.Name.DiskCensor(),
            Query = CensorQuery(source.Query),
            T0 = source.T0,
            T1 = source.T1,
            T2 = source.T2,
            T3 = source.T3,
            Ts = source.Ts,
            Tn = source.Tn,
        };
    }
    #endregion
}