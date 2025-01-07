using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using CoreAPI.AL.Models;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ZBenchmark;

[InProcess]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class FileSystemBenchmark
{
    static AlbumInfoProvider _ai = new AlbumInfoProvider {
        EUrl = "",
        NUrl = "",
    };
    static SystemIOAbstraction _io = new SystemIOAbstraction();

    static string source = "Z:\\_Temp\\[Anon] Test Webp";

    [Benchmark]
    public void GetFilePaths() {
        var result = _io.GetSuitableFilePaths(source, _ai.SuitableFileFormats, 1);

        var page100 = result[100];
    }

    [Benchmark]
    public void GetFilePathsWithSort() {
        var result = _io.GetSuitableFilePathsWithNaturalSort(source, _ai.SuitableFileFormats, 1);

        var page100 = result[100];
    }

    [Benchmark]
    public async Task GetFilePathsFromJson() {
        var jsonPath = Path.Combine(source, "_files.json");
        bool exist = File.Exists(jsonPath);

        if(!exist) {
            throw new Exception("Wololo");
        }

        var alRelFiles = await _io.DeserializeJson<List<string>>(jsonPath);

        var page100 = Path.Combine(source, alRelFiles[100]);
    }


    [Benchmark]
    public void GetAlbumPageInfos() {
        var allFilePaths = _io.GetSuitableFilePathsWithNaturalSort(source, _ai.SuitableFileFormats, 1);

        var result = allFilePaths.Select(f => new FileInfoModel {
            Name = Path.GetFileName(f),
            LibRelPath = Path.GetRelativePath(source, f),
            Extension = Path.GetExtension(f)
        }).ToList();
    }

    [Benchmark]
    public void GetPageInfo() {
        var allFilePaths = _io.GetSuitableFilePathsWithNaturalSort(source, _ai.SuitableFileFormats, 1);

        if(allFilePaths.Count == 0) {
            return;
        }
        var targetFullPath = allFilePaths[100];
        var fileInfo = new FileInfo(targetFullPath);

        var targetLibRelPath = Path.GetRelativePath(source, targetFullPath);

        var result = new FileInfoModel {
            Name = Path.GetFileName(targetFullPath),
            Size = fileInfo.Length,
            LibRelPath = targetLibRelPath,
            Extension = Path.GetExtension(targetFullPath)
        };
    }

    [Benchmark]
    public void GetPageInfoNoFI() {
        var allFilePaths = _io.GetSuitableFilePathsWithNaturalSort(source, _ai.SuitableFileFormats, 1);

        if(allFilePaths.Count == 0) {
            return;
        }
        var targetFullPath = allFilePaths[100];
        //var fileInfo = new FileInfo(targetFullPath);

        var targetLibRelPath = Path.GetRelativePath(source, targetFullPath);

        var result = new FileInfoModel {
            Name = Path.GetFileName(targetFullPath),
            //Size = fileInfo.Length,
            LibRelPath = targetLibRelPath,
            Extension = Path.GetExtension(targetFullPath)
        };
    }
}