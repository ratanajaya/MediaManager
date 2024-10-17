using CoreAPI.AL.DataAccess;
using CoreAPI.AL.Helpers;
using CoreAPI.AL.Models;
using CoreAPI.AL.Models.Config;
using CoreAPI.AL.Models.LogDb;
using Serilog;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreAPI.AL.Services;

public class ExtraInfoService
{
    ILogger _logger;
    ILogDbContext _logDb;
    AlbumInfoProvider _ai;
    CoreApiConfig _config;
    IDbContext _db;
    LibraryRepository _lib;

    public ExtraInfoService(ILogger logger, ILogDbContext logDb, AlbumInfoProvider ai, CoreApiConfig config, IDbContext db, LibraryRepository lib) {
        _logger = logger;
        _logDb = logDb;
        _ai = ai;
        _config = config;
        _db = db;
        _lib = lib;
    }

    public List<Comment> GetComments(string url) {
        try {
            return _logDb.GetComments(url);
        }
        catch(Exception e) {
            _logger.Error($"ExtraInfoServices.GetComments{Environment.NewLine}" +
                $"Params=[{url}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }

    public void UpsertSourceAndContent(SourceAndContentUpsertModel param) {
        var now = DateTime.Now;

        var albumVm = _db.AlbumVMs.Get(param.AlbumPath);

        var sourceAndContent = param.SourceAndContent;

        var existingSource = albumVm.Album.Sources.FirstOrDefault(a => a.Url == sourceAndContent.Source.Url);

        if(existingSource != null) {
            existingSource.Title = sourceAndContent.Source.Title;
            existingSource.SubTitle = sourceAndContent.Source.SubTitle;
        }
        else {
            albumVm.Album.Sources.Add(sourceAndContent.Source);
        }

        _lib.SaveAlbumMetadata(albumVm, CrudLog.Update);
        _db.SaveChanges();

        var scrapedComments = sourceAndContent.Comments;

        var existingComments = _logDb.GetComments(sourceAndContent.Source.Url);

        var commentToInsert = scrapedComments
            .Where(a => !existingComments.Any(b => b.Author == a.Author && b.PostedDate == a.PostedDate))
            .ToList();

        commentToInsert.ForEach(a => {
            a.Url = sourceAndContent.Source.Url;
            a.CreatedDate = now;
        });

        var commentToUpdate = new List<Comment>();

        existingComments.ForEach(a => {
            var scrappedComment = scrapedComments.FirstOrDefault(b => b.Author == a.Author && b.PostedDate == a.PostedDate);

            if(a.Content != scrappedComment?.Content || a.Score != scrappedComment.Score) {
                a.Content = scrappedComment?.Content;
                a.Score = scrappedComment?.Score;
                a.UpdatedDate = now;

                commentToUpdate.Add(a);
            }
        });

        if(commentToInsert.Any())
            _logDb.InsertComments(commentToInsert);
        if(commentToUpdate.Any())
            _logDb.UpdateComments(commentToUpdate);
    }

    public void DeleteSource(SourceDeleteModel param) {
        var albumVm = _db.AlbumVMs.Get(param.AlbumPath);

        albumVm.Album.Sources = albumVm.Album.Sources.Where(a => a.Url != param.Url).ToList();
        _lib.SaveAlbumMetadata(albumVm, CrudLog.Update);
        _db.SaveChanges();

        _logDb.DeleteComments(param.Url);
    }

    public void UpdateSource(SourceUpdateModel param) {
        var albumVm = _db.AlbumVMs.Get(param.AlbumPath);

        var toUpdateSource = albumVm.Album.Sources.First(a => a.Url == param.Source.Url);

        toUpdateSource.Title = param.Source.Title;
        toUpdateSource.SubTitle = param.Source.SubTitle;

        _lib.SaveAlbumMetadata(albumVm, CrudLog.Update);
        _db.SaveChanges();
    }

    #region Postponed
    public async Task ScrapeComment(string url) {
        try {
            var now = DateTime.Now;

            #region Postponed / Cancelled Selenium Experiment
            //var service = FirefoxDriverService.CreateDefaultService("C:\\Users\\fakew\\Downloads\\geckodriver");
            //service.FirefoxBinaryPath = "C:\\Program Files\\Mozilla Firefox\\firefox.exe";

            //var driver = new FirefoxDriver(service);

            //var options = new FirefoxOptions();
            //options.BinaryLocation = _config.PortableBrowserPath;
            //var driver = new Chr(options);

            //var driverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "geckodriver.exe");

            ////Give the path of the geckodriver.exe    
            //FirefoxDriverService service = FirefoxDriverService.CreateDefaultService(driverPath);

            ////Give the path of the Firefox Browser        
            ////service.FirefoxBinaryPath = _config.PortableBrowserPath;

            //IWebDriver driver = new FirefoxDriver(service);
            //driver.Navigate().GoToUrl("https://www.google.com");

            //driver.Navigate().GoToUrl("https://www.selenium.dev/selenium/web/web-form.html");

            //driver.Quit();
            #endregion

            var scrapedComments = await (new Func<Task<List<Comment>>>(async () => {
                if(url.StartsWith("N:")) {
                    var id = url.Split(':').Last();
                    var apiUrl = string.Format(_ai.NUrl, id);

                    throw new NotImplementedException();
                }
                else if(url.StartsWith("E:")) {
                    throw new NotImplementedException();
                }
                else
                    throw new Exception("Url invalid");
            }))();

            var existingComments = _logDb.GetComments(url);

            var commentToInsert = scrapedComments
                .Where(a => !existingComments.Any(b => b.Author == a.Author && b.PostedDate == a.PostedDate))
                .ToList();

            var commentToUpdate = new List<Comment>();

            existingComments.ForEach(a => {
                var scrappedComment = scrapedComments.FirstOrDefault(b => b.Author == a.Author && b.PostedDate == a.PostedDate);

                if(a.Content != scrappedComment?.Content || a.Score != scrappedComment.Score) {
                    a.Content = scrappedComment?.Content;
                    a.Score = scrappedComment?.Score;
                    a.UpdatedDate = now;

                    commentToUpdate.Add(a);
                }
            });

            if(commentToInsert.Any())
                _logDb.InsertComments(commentToInsert);
            if(commentToUpdate.Any())
                _logDb.UpdateComments(commentToUpdate);
        }
        catch(Exception e) {
            _logger.Error($"ExtraInfoServices.ScrapeComment{Environment.NewLine}" +
                $"Params=[{url}]{Environment.NewLine}" +
                $"{e}");

            throw;
        }
    }
    #endregion
}
