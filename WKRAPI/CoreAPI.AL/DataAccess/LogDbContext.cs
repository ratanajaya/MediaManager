using CoreAPI.AL.Models;
using CoreAPI.AL.Models.Config;
using CoreAPI.AL.Models.Dashboard;
using CoreAPI.AL.Models.LogDb;
using SharedLibrary;
using SharedLibrary.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Qh = CoreAPI.AL.Helpers.QueryHelpers;

namespace CoreAPI.AL.DataAccess;

public interface ILogDbContext
{
    void DeleteAlbumCorrection(string libRelPath);
    void DeleteComments(string url);
    int DeleteCrudLogs(List<long> ids);
    List<AlbumCorrection> GetAlbumCorrections();
    List<Comment> GetComments(string url);
    CorrectionLog GetCorrectionLog(string path);
    List<CorrectionLog> GetCorrectionLogs(List<string> paths);
    List<CrudLog> GetDeleteLogs(string? query);
    DateTime? GetLastCorrectionTime(string path);
    TablePaginationModel<CrudLog> GetLogs(int page, int row, string? operation, string? freeText, DateTime? startDate, DateTime? endDate);
    void InsertAlbumCorrection(AlbumCorrection ac);
    void InsertComments(List<Comment> comments);
    void InsertCrudLog(string operation, AlbumVM avm);
    void Devtool_OneTimeOperation();
    void UpdateComments(List<Comment> comments);
    void UpdateCorrectionLog(string path, DateTime? correctionDate, int correctablePageCount);
}

public class LogDbContext(
     CoreApiConfig _config
    ) : ILogDbContext
{
    public List<CrudLog> GetDeleteLogs(string? query) {
        var querySegments = Qh.GetQuerySegments(query);
        var now = DateTime.Now;

        var deleteLogs = GetLogs(0, 0, CrudLog.Delete, null, null, null);
        var deleteLogsByQuery = deleteLogs.Records
            //Realistically, a.AlbumJson will never be null for delete logs but always be null for everything else
            .Where(a => Qh.MatchAllQueries(JsonSerializer.Deserialize<Album>(a.AlbumJson!)!, querySegments, [], [], now))
            .ToList();

        return deleteLogsByQuery;
    }

    public TablePaginationModel<CrudLog> GetLogs(int page, int row, string? operation, string? freeText, DateTime? startDate, DateTime? endDate) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var lowerFreeText = !string.IsNullOrEmpty(freeText) ? freeText.ToLower() : null;

            var qWhere = new Func<string>(() => {
                var wFt = string.IsNullOrEmpty(lowerFreeText) ? "" :
                    $" AND LOWER(AlbumFullTitle) LIKE '%{lowerFreeText}%'";
                var wOp = string.IsNullOrEmpty(operation) ? "" :
                    $" AND Operation == '{operation}'";
                var wSd = !startDate.HasValue ? "" :
                    $" AND CreateDate >= {startDate.Value.Ticks}";
                var wEd = !endDate.HasValue ? "" :
                    $" AND CreateDate <= {endDate.Value.Ticks}";

                return $"WHERE 0=0{wFt}{wOp}{wSd}{wEd}";
            })();

            var total = db.ExecuteScalar<int>($"SELECT COUNT(*) FROM CrudLog {qWhere}");
            var totalPage = row > 0 ? ((total - 1) / row) + 1 : 1;

            var qOrder = $"ORDER BY CreateDate DESC";

            var qLimit = (page > 0 && row > 0) ? $"LIMIT {row} OFFSET {(page - 1) * row}" : "";

            var records = db.Query<CrudLog>($"SELECT * FROM CrudLog {qWhere} {qOrder} {qLimit}");

            return new TablePaginationModel<CrudLog> {
                Records = records,
                TotalItem = total,
                TotalPage = totalPage
            };
        }
    }

    public void InsertCrudLog(string operation, AlbumVM avm) {
        var now = DateTime.Now;
        var anchorDate = now.Subtract(CrudLog.MergeThreshold);
        var updateFristRead = new string[] { CrudLog.Update, CrudLog.FirstRead };

        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var sameTypeOperations = updateFristRead.Contains(operation) ? updateFristRead : new string[] { operation };

            var recentLogOfSameType = db.Table<CrudLog>()
                .FirstOrDefault(a => a.AlbumPath == avm.Path && sameTypeOperations.Contains(a.Operation)
                    && (a.UpdateDate > anchorDate || a.CreateDate > anchorDate));

            if(recentLogOfSameType != null) {
                if(operation == CrudLog.FirstRead)
                    recentLogOfSameType.Operation = operation;

                recentLogOfSameType.UpdateDate = now;

                db.Update(recentLogOfSameType);
            }
            else {
                db.Insert(new CrudLog(operation, avm.Path, avm.Album, now));
            }
        }
    }

    public int DeleteCrudLogs(List<long> ids) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            return db.Table<CrudLog>()
                .Where(a => ids.Contains(a.Id))
                .Delete();
        }
    }

    public DateTime? GetLastCorrectionTime(string path) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var entity = db.Table<CorrectionLog>().FirstOrDefault(a => a.Path == path);
            return entity?.LastCorrectionDate;
        }
    }

    public CorrectionLog GetCorrectionLog(string path) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var entity = db.Table<CorrectionLog>().FirstOrDefault(a => a.Path == path);
            return entity;
        }
    }

    public List<CorrectionLog> GetCorrectionLogs(List<string> paths) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var entities = db.Table<CorrectionLog>().Where(a => paths.Contains(a.Path!)).ToList();
            return entities;
        }
    }

    public void UpdateCorrectionLog(string path, DateTime? correctionDate, int correctablePageCount) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var cl = db.Table<CorrectionLog>().FirstOrDefault(a => a.Path == path);

            if(cl != null) {
                if(correctionDate != null)
                    cl.LastCorrectionDate = correctionDate.Value;

                cl.CorrectablePageCount = correctablePageCount;
                db.Update(cl);
            }
            else {
                db.Insert(new CorrectionLog {
                    Path = path,
                    LastCorrectionDate = correctionDate.GetValueOrDefault(),
                    CorrectablePageCount = correctablePageCount
                });
            }
        }
    }

    public List<Comment> GetComments(string url) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            var result = db.Table<Comment>()
                .Where(a => a.Url == url)
                .OrderBy(a => a.PostedDate)
                .ToList();

            return result;
        }
    }

    public void InsertComments(List<Comment> comments) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            db.InsertAll(comments);
        }
    }

    public void UpdateComments(List<Comment> comments) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            db.UpdateAll(comments);
        }
    }

    public void DeleteComments(string url) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            db.Table<Comment>()
                .Where(a => a.Url == url)
                .Delete();
        }
    }

    public List<AlbumCorrection> GetAlbumCorrections() {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) { 
            return db.Table<AlbumCorrection>().ToList();
        }
    }

    public void InsertAlbumCorrection(AlbumCorrection ac) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            db.Insert(ac);
        }
    }

    public void DeleteAlbumCorrection(string libRelPath) {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            db.Table<AlbumCorrection>()
                .Where(a => a.LibRelPath == libRelPath)
                .Delete();
        }
    }

    public void Devtool_OneTimeOperation() {
        using(var db = new SQLiteConnection(_config.FullLogDbPath)) {
            
        }
    }
}
