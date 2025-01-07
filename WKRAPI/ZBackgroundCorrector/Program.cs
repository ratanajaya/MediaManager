using CoreAPI;
using CoreAPI.AL.Models.Sc;
using CoreAPI.Models;
using Serilog.Events;
using Serilog;
using SharedLibrary.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;
using System.Web;
using Microsoft.Extensions.Configuration;

class Program
{
    static string _newLine = System.Environment.NewLine;

    static string _mainApiUrl = "";

    static string _login() => $"{_mainApiUrl}/Auth/Login";
    static string _getAcm() => $"{_mainApiUrl}/Main/GetAlbumCardModels";
    static string _hscanCorrectablePaths() => $"{_mainApiUrl}/Operation/HScanCorrectiblePaths";
    static string _getCorrectablePages() => $"{_mainApiUrl}/Operation/GetCorrectablePages";
    static string _correctPages() => $"{_mainApiUrl}/Operation/CorrectPages";
    static string _scGetCorrectablePaths() => $"{_mainApiUrl}/Operation/ScGetCorrectablePaths";

    static async Task Main(string[] args) {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("bcAppSettings.json");

        var configuration = configurationBuilder.Build();

        _mainApiUrl = configuration["MainApiUrl"]!;

        Log.Logger = new LoggerConfiguration()
              .Enrich.FromLogContext()
              .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
              .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "L_.Log"), rollingInterval: RollingInterval.Day)
              .WriteTo.Console()
              .CreateLogger();
        Log.Information("~START~");
        Console.WriteLine("Password:");
        var authToken = await Login(Console.ReadLine()!);
        Console.WriteLine($"AuthToken: {authToken}");


        Console.WriteLine("UpscaleTarget: (a. 1280 | b. 1600)");
        var selectedTarget = Console.ReadLine();
        var upscaleTarget = selectedTarget == "a" ? 1280 : selectedTarget == "b" ? 1600 : throw new Exception();

        Console.WriteLine("Thread: ");
        var thread = Math.Clamp(int.Parse(Console.ReadLine()!), 1, 4);
        Console.WriteLine();
        Console.WriteLine("a. H Correction");
        Console.WriteLine("b. SC Correction");
        Console.Write($"{_newLine}Proceed? (a,b) ");

        var res = Console.ReadLine();

        var cts = new CancellationTokenSource();

        if (res == "a") {
            Console.WriteLine("Query: ");
            var query = Console.ReadLine();
            if(string.IsNullOrEmpty(query))
                return;

            try {
                var ongoingProcessTask = MainHCorrection(authToken, query, thread, upscaleTarget, cts.Token);
                var listenForKeyPressTask = Task.Run(() => { ListenForKeyPress(cts); });

                await Task.WhenAny(listenForKeyPressTask, ongoingProcessTask);

                // If 'c' is pressed, ensure OngoingProcess has a chance to complete
                if (cts.IsCancellationRequested) {
                    await ongoingProcessTask; // Wait for it to complete
                }
            }
            catch(Exception ex) {
                Log.Error($"MainHCorrection | {ex.Message}");
            }
        }
        if(res == "b") {
            try {
                var ongoingProcessTask = MainSCCorrection(authToken, thread, upscaleTarget, cts.Token);
                var listenForKeyPressTask = Task.Run(() => { ListenForKeyPress(cts); });

                await Task.WhenAny(listenForKeyPressTask, ongoingProcessTask);

                // If 'c' is pressed, ensure OngoingProcess has a chance to complete
                if(cts.IsCancellationRequested) {
                    await ongoingProcessTask; // Wait for it to complete
                }
            }
            catch(Exception ex) {
                Log.Error($"MainSCCorrection | {ex.Message}");
            }
        }

        Log.Information("~END~");
        Console.ReadLine();
    }

    #region Local Methods
    static void ListenForKeyPress(CancellationTokenSource cts) {
        while (true) {
            var key = Console.ReadKey(intercept: true).KeyChar;

            if (key == 'c' || key == 'C') {
                Log.Information("'c' key pressed. The app will terminate on the next loop.");
                cts.Cancel();
                break;
            }
        }
    }

    #endregion

    #region Backend Caller Methods
    static async Task<string> Login(string password) {
        using(HttpClient client = new HttpClient()) {
            client.Timeout = Timeout.InfiniteTimeSpan;

            HttpResponseMessage response = await client.PostAsync($"{_login()}?password={password}", null);

            if(response.IsSuccessStatusCode) {
                var token = await response.Content.ReadAsStringAsync();
                return token;
            }
            else {
                var errMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Login | {response.StatusCode} | {errMsg}");
            }
        }
    }

    static async Task MainHCorrection(string authToken, string query, int thread, int upscaleTarget, CancellationToken token) {
        Log.Information($"Start MainHCorrection | {query}");

        try {
            var albumCardModelToProcessList = await GetAlbumCardModelWithNoCorrectionRecord(authToken, query);

            Log.Information($"AlbumCardModels | ToProcess: {albumCardModelToProcessList.Count}");

            bool exitMainHCorrectionApp = false;

            // Divide the albumCardModels into batches of 30 Paths each
            int batchSize = 30;
            for(int i = 0; i < albumCardModelToProcessList.Count; i += batchSize) {
                if(exitMainHCorrectionApp) {
                    Log.Information($"Exiting outer loop...");
                    break;
                }

                List<string> batchPaths = albumCardModelToProcessList
                    .Skip(i)
                    .Take(batchSize)
                    .Select(model => model.Path)
                    .ToList();

                Log.Information($"Scanning for correctible albums in batch of {batchPaths.Count}");
                Console.WriteLine("");

                HScanCorrectiblePathParam param = new HScanCorrectiblePathParam {
                    Paths = batchPaths,
                    Thread = thread,
                    UpscaleTarget = upscaleTarget
                };

                var pathCorrectionModels = await PostHScanCorrectiblePaths(authToken, param);

                foreach(var pcm in pathCorrectionModels) {
                    if(token.IsCancellationRequested) {
                        Log.Information($"Exiting inner loop...");
                        exitMainHCorrectionApp = true;
                        break;
                    }

                    Log.Information($"Pgs: {pcm.CorrectablePageCount} | {Path.GetFileName(pcm.LibRelPath)}");

                    var fileCorrectionModels = await GetCorrectablePagesAsync(authToken, 0, pcm.LibRelPath, true, upscaleTarget);

                    Log.Information($"Ret: {fileCorrectionModels.Count} | Starting correction...");

                    var correctionSw = new Stopwatch();
                    correctionSw.Start();

                    var report = await PostCorrectPages(authToken, new CorrectPageParam {
                        LibRelPath = pcm.LibRelPath,
                        FileToCorrectList = fileCorrectionModels,
                        Thread = thread,
                        Type = 0,
                        UpscalerType = SharedLibrary.UpscalerType.Waifu2xCunet,
                        ToJpeg = true
                    });

                    correctionSw.Stop();

                    if(report.Any(a => !a.Success)) {
                        var firstErrorFile = report.First(a => !a.Success);
                        Log.Error($"Error: {pcm.LibRelPath} | {firstErrorFile.AlRelPath} | {firstErrorFile.Message} | {correctionSw.Elapsed.TotalSeconds} s");
                    }
                    else {
                        double totalSeconds = correctionSw.Elapsed.TotalSeconds;

                        double avg = totalSeconds / fileCorrectionModels.Count;

                        Log.Information($"Fin: {fileCorrectionModels.Count} | {correctionSw.Elapsed.TotalSeconds.ToString("0.##")} s | {avg.ToString("0.##")} s");
                    }
                }
            }            
        }
        catch(Exception ex) {
            Log.Error($"MainHCorrection | {ex.Message}");
        }
    }

    static async Task MainSCCorrection(string authToken, int thread, int upscaleTarget, CancellationToken token) {
        try {
            var correctablePaths = await ScGetCorrectablePaths(authToken);
            var pathToProcessList = correctablePaths
                .Where(a => a.CorrectablePageCount > 0 && !a.LibRelPath.Contains("Test", StringComparison.OrdinalIgnoreCase))
                .ToList();

            Log.Information($"SC Paths | Total: {correctablePaths.Count} | ToProcess: {pathToProcessList.Count}");

            foreach(var pathToProcess in pathToProcessList) {
                if(token.IsCancellationRequested) {
                    Log.Information($"Exiting loop...");
                    break;
                }

                Log.Information($"Pth: {pathToProcess.LibRelPath}");

                var fileCorrectionModels = await GetCorrectablePagesAsync(authToken, 1, pathToProcess.LibRelPath, false, upscaleTarget);

                Log.Information($"Ret: {fileCorrectionModels.Count} | Starting correction...");

                var correctionSw = new Stopwatch();
                correctionSw.Start();

                var report = await PostCorrectPages(authToken, new CorrectPageParam {
                    LibRelPath = pathToProcess.LibRelPath,
                    FileToCorrectList = fileCorrectionModels,
                    Thread = thread,
                    Type = 1,
                    UpscalerType = SharedLibrary.UpscalerType.Waifu2xCunet,
                    ToJpeg = false
                });

                correctionSw.Stop();

                if(report.Any(a => !a.Success)) {
                    var firstErrorFile = report.First(a => !a.Success);
                    Log.Error($"Error: {pathToProcess.LibRelPath} | {firstErrorFile.AlRelPath} | {firstErrorFile.Message} | {correctionSw.Elapsed.TotalSeconds} s");
                }
                else {
                    double totalSeconds = correctionSw.Elapsed.TotalSeconds;

                    double avg = totalSeconds / fileCorrectionModels.Count;

                    Log.Information($"Fin: {fileCorrectionModels.Count} | {correctionSw.Elapsed.TotalSeconds.ToString("0.##")} s | {avg.ToString("0.##")} s");
                }
            }
        }
        catch(Exception ex) {
            Log.Error($"MainSCCorrection | {ex.Message}");
        }
    }

    static async Task<List<AlbumCardModel>> GetAlbumCardModelWithNoCorrectionRecord(string authToken, string query) {
        using(HttpClient client = new HttpClient()) {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authToken);
            client.Timeout = Timeout.InfiniteTimeSpan;
            HttpResponseMessage response = await client.GetAsync($"{_getAcm()}?query={HttpUtility.UrlEncode(query)}&excludeCorrected=true");

            if(response.IsSuccessStatusCode) {
                List<AlbumCardModel> albumCardModels = await response.Content.ReadAsAsync<List<AlbumCardModel>>();
                return albumCardModels;
            }
            else {
                var errMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"GetAlbumCardModelsAsync | {response.StatusCode} | {errMsg}");
            }
        }
    }

    static async Task<List<PathCorrectionModel>> PostHScanCorrectiblePaths(string authToken, HScanCorrectiblePathParam param) {
        using(HttpClient client = new HttpClient()) {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authToken);
            client.Timeout = Timeout.InfiniteTimeSpan;
            HttpResponseMessage response = await client.PostAsJsonAsync(_hscanCorrectablePaths(), param);

            if(response.IsSuccessStatusCode) {
                List<PathCorrectionModel> pathCorrectionModels = await response.Content.ReadAsAsync<List<PathCorrectionModel>>();
                return pathCorrectionModels;
            }
            else {
                var errMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"PostHScanCorrectiblePaths | {response.StatusCode} | {errMsg}");
            }
        }
    }

    static async Task<List<FileCorrectionModel>> GetCorrectablePagesAsync(string authToken, int type, string libRelPath, bool clampToTarget, int upscaleTarget) {
        using(HttpClient client = new HttpClient()) {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authToken);
            client.Timeout = Timeout.InfiniteTimeSpan;
            string fullUrl = $"{_getCorrectablePages()}?type={type}&path={HttpUtility.UrlEncode(libRelPath)}&thread=2&upscaleTarget={upscaleTarget}&clampToTarget={clampToTarget.ToString().ToLower()}";
            HttpResponseMessage response = await client.GetAsync(fullUrl);

            if(response.IsSuccessStatusCode) {
                List<FileCorrectionModel> fileCorrectionModels = await response.Content.ReadAsAsync<List<FileCorrectionModel>>();
                return fileCorrectionModels;
            }
            else {
                var errMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"GetCorrectablePagesAsync | {response.StatusCode} | {errMsg}");
            }
        }
    }

    static async Task<List<FileCorrectionReportModel>> PostCorrectPages(string authToken, CorrectPageParam param) {
        using(HttpClient client = new HttpClient()) {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authToken);
            client.Timeout = Timeout.InfiniteTimeSpan;
            HttpResponseMessage response = await client.PostAsJsonAsync(_correctPages(), param);

            if(response.IsSuccessStatusCode) {
                List<FileCorrectionReportModel> pathCorrectionModels = await response.Content.ReadAsAsync<List<FileCorrectionReportModel>>();
                return pathCorrectionModels;
            }
            else {
                var errMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"PostCorrectPages | {response.StatusCode} | {errMsg}");
            }
        }
    }

    static async Task<List<PathCorrectionModel>> ScGetCorrectablePaths(string authToken) {
        using(HttpClient client = new HttpClient()) {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authToken);
            client.Timeout = Timeout.InfiniteTimeSpan;
            HttpResponseMessage response = await client.GetAsync($"{_scGetCorrectablePaths()}");

            if(response.IsSuccessStatusCode) {
                var pathCorrectionModels = await response.Content.ReadAsAsync<List<PathCorrectionModel>>();
                return pathCorrectionModels;
            }
            else {
                var errMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"ScGetCorrectablePaths | {response.StatusCode} | {errMsg}");
            }
        }
    }
    #endregion
}
