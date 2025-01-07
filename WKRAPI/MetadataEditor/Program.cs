using SharedLibrary;
using System;
using System.Windows.Forms;
using SimpleInjector;
using System.Configuration;
using CoreAPI.AL.Services;
using Serilog;
using System.IO;
using MetadataEditor.Models;
using MetadataEditor.Services;
using Microsoft.Extensions.Caching.Memory;
using CoreAPI.AL.DataAccess;
using System.Runtime.Versioning;
using System.Text.Json;
using SharedLibrary.Models;

namespace MetadataEditor;

[SupportedOSPlatform("windows")]
static class Program
{
    static readonly Container container;

    static Program() {
        container = new Container();
    }

    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        container.Register<AppLogic>(Lifestyle.Singleton);
        container.Register<ISystemIOAbstraction, SystemIOAbstraction>(Lifestyle.Singleton);
        container.Register<FormMain>(Lifestyle.Singleton);
        var configuration = new MetadataEditorConfig {
            BrowsePath = ConfigurationManager.AppSettings["BrowsePath"]!,
            Args = args
        };
        container.RegisterInstance(configuration);
        var ai = JsonSerializer.Deserialize<AlbumInfoProvider>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "albumInfo.json")))!;
        container.RegisterInstance(ai);

        #region Services from API project
        container.Register<ExtraInfoService>(Lifestyle.Singleton);
        container.Register<LibraryRepository>(Lifestyle.Singleton);
        container.Register<FileRepository>(Lifestyle.Singleton);
        container.Register<MediaProcessor>(Lifestyle.Singleton);
        container.Register<IDbContext, JsonDbContext>(Lifestyle.Singleton);
        container.Register<ILogDbContext, LogDbContext>(Lifestyle.Singleton);
        container.Register<IFlagDbContext, FlagDbContext>(Lifestyle.Singleton);
        container.Register<ImageProcessor>(Lifestyle.Singleton);

        var apiConf = new CoreAPI.AL.Models.Config.CoreApiConfig {
            LibraryPath = ConfigurationManager.AppSettings["LibraryPath"]!,
            TempPath = ConfigurationManager.AppSettings["TempPath"]!,
            BuildType = "Private",
            AppType = "MetadataEditor",
            ProcessorApiUrl = ConfigurationManager.AppSettings["ProcessorApiUrl"]!,
            Version = "Not Set",
            PortableBrowserPath = "Not Set",
            ScLibraryPath = "Not Set"
        };
        container.RegisterInstance(apiConf);
        var logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serilog/log-.txt"), rollingInterval: RollingInterval.Day)
            .CreateLogger();
        container.RegisterInstance<ILogger>(logger);
        #endregion

        container.RegisterInstance<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));

        container.Verify();

        Application.Run(container.GetInstance<FormMain>());
    }
}