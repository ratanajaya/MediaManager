using CoreAPI.AL.DataAccess;
using CoreAPI.AL.Models.Config;
using CoreAPI.AL.Models.Dashboard;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace CoreAPI.AL.Services;

public class DashboardService
{
    AlbumInfoProvider _ai;
    ILogDbContext _logDb;
    LibraryRepository _library;
    CoreApiConfig _config;

    public DashboardService(AlbumInfoProvider ai, LibraryRepository library, ILogDbContext logDb, CoreApiConfig config) {
        _ai = ai;
        _library = library;
        _logDb = logDb;
        _config = config;
    }

    public ForceGraphData GetTagForceGraphData() {
        var albums = _library.GetAlbumVMs(0, 0, null);

        var combinedTags = _ai.Others.Concat(_ai.Rares).Concat(_ai.Qualities)
            .Concat(_ai.Povs.Select(a => "p:" + a))
            .Concat(_ai.Focuses.Select(a => "f:" + a))
            .ToArray();

        var albumTagsDict = combinedTags.ToDictionary(a => a,
                a => albums
                    .Where(b => {
                        if(a.StartsWith("p:"))
                            return b.Album.Povs.Contains(a[2..]);
                        if(a.StartsWith("f:"))
                            return b.Album.Focuses.Contains(a[2..]);

                        return b.Album.Others.Contains(a) ||
                            b.Album.Rares.Contains(a) ||
                            b.Album.Qualities.Contains(a);
                    })
                    .Select(b => b.Album)
                    .ToList()
            );

        var nodes = new List<ForceGraphNode>();
        var links = new List<ForceGraphLink>();

        for(int i=0; i < combinedTags.Length; i++) {
            var sourceTag = combinedTags[i];
            var sourceAlbums = albumTagsDict[sourceTag];

            nodes.Add(new() { 
                Id = sourceTag, 
                Count = sourceAlbums.Count 
            });

            if(i == combinedTags.Length - 1)
                break;

            for(int j=i+1; j < combinedTags.Length; j++) {
                var targetTag = combinedTags[j];
                var targetAlbums = albumTagsDict[targetTag];

                links.Add(new() {
                    Source = sourceTag,
                    Target = targetTag,
                    SourceCount = sourceAlbums.Count,
                    TargetCount = targetAlbums.Count,
                    LinkCount = sourceAlbums.Where(a => {
                        if(targetTag.StartsWith("p:"))
                            return a.Povs.Contains(targetTag[2..]);
                        if(targetTag.StartsWith("f:"))
                            return a.Focuses.Contains(targetTag[2..]);

                        return a.Others.Contains(targetTag) ||
                            a.Rares.Contains(targetTag) ||
                            a.Qualities.Contains(targetTag);
                    }).Count()
                });
            }
        }

        var linkWithValue = links.Where(a => a.Value > 0).ToList();

        return new ForceGraphData {
            Nodes = nodes,
            Links = linkWithValue,
        };
    }

    //Pre-existing
    public TierFractionModel GetTierFractionFromQuery(string query, string name) {
        var albums = _library.GetAlbumVMs(0, 0, query);

        //Optimized code
        int[] tc = new int[6];
        foreach(var album in albums) {
            if(!album.Album.IsRead)
                tc[5]++;
            else if(album.Album.Tier < 3)
                tc[album.Album.Tier]++;
            else if(album.Album.Note != "🌟")
                tc[3]++;
            else
                tc[4]++;
        }
        return new TierFractionModel {
            Name = name,
            Query = query,
            T0 = tc[0],
            T1 = tc[1],
            T2 = tc[2],
            T3 = tc[3],
            Ts = tc[4],
            Tn = tc[5],
        };
    }

    public List<TierFractionModel> GetGenreTierFractions() {
        return _ai.GenreQueries.Where(a => a.Group != 0)
            .Select(a => GetTierFractionFromQuery(a.Query, a.Name))
            .ToList();
    }

    public TablePaginationModel<LogDashboardModel> GetLogs(int page, int row, string operation, string freeText, DateTime? startDate, DateTime? endDate) {
        var data = _logDb.GetLogs(page, row, operation, freeText, startDate, endDate);

        return new TablePaginationModel<LogDashboardModel> {
            TotalPage = data.TotalPage,
            TotalItem = data.TotalItem,
            Records = data.Records.Select(a => new LogDashboardModel {
                Id = a.Id.ToString(),
                AlbumFullTitle = a.AlbumFullTitle,
                CreationTime = a.CreateDate,
                Operation = a.Operation
            }).ToList()
        };
    }

    public List<LogDashboardModel> GetDeleteLogs(string query, bool? includeAlbum) {
        var logs = _logDb.GetDeleteLogs(query);

        return logs.Select(a => new LogDashboardModel {
            Id = a.Id.ToString(),
            AlbumFullTitle = a.AlbumFullTitle,
            CreationTime = a.CreateDate,
            Operation = a.Operation,
            Album = includeAlbum.GetValueOrDefault() ? JsonSerializer.Deserialize<Album>(a.AlbumJson) : null
        }).ToList();
    }
}
