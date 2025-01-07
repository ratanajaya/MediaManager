using MediaInfo;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging.Abstractions;
using SharedLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharedLibrary;
#pragma warning disable CA1416 // Validate platform compatibility

//Very thin wrapper of System.IO
//Must not require any dependency
public class SystemIOAbstraction : ISystemIOAbstraction
{
    #region QUERY
    public string[] GetFiles(string path) => Directory.GetFiles(path);

    public string[] GetDirectories(string path) => Directory.GetDirectories(path);

    public string[] GetDirectories(string path, SearchOption option) => Directory.GetDirectories(path, "*.*", option);

    public bool IsFileExists(string path) => File.Exists(path);

    public bool IsDirectoryExist(string path) => Directory.Exists(path);

    public bool IsPathExist(string path) {
        return (Directory.Exists(path) || File.Exists(path));
    }

    public byte[] ReadFile(string path) => File.ReadAllBytes(path);

    public Bitmap ReadBitmap(string path) => new Bitmap(path);

    public Task<string> ReadText(string path) => File.ReadAllTextAsync(path);

    public FileInfo GetFileInfo(string path) => new FileInfo(path);

    public FileStream OpenRead(string path) => File.OpenRead(path);

    public List<string> GetSuitableFilePaths(string folderPath, string[] suitableFileFormats, int depth) 
        => Directory.EnumerateFiles(folderPath, "*.*", (depth > 0 ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            .Where(file => Array.IndexOf(suitableFileFormats, Path.GetExtension(file)) > -1)
            .ToList();

    //OPTIMIZED at 2022-07-02
    public List<string> GetSuitableFilePathsWithNaturalSort(string folderPath, string[] suitableFileFormats, int depth) {
        var filePaths = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(f => Array.IndexOf(suitableFileFormats, Path.GetExtension(f)) > -1)
            .ToList();

        var result = filePaths.OrderByAlphaNumeric(f => Path.GetFileName(f)).ToList();

        if(depth == 0) { return result; }

        var subDirs = Directory.GetDirectories(folderPath);//.OrderByAlphaNumeric(f => f);
        foreach(var dir in subDirs) {
            result.AddRange(GetSuitableFilePathsWithNaturalSort(dir, suitableFileFormats, depth - 1));
        }

        return result;
    }

    public Stream GetStream(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

    public DateTime GetLastWriteTime(string path) => File.GetLastWriteTime(path);
    #endregion

    #region COMMAND
    public Task WriteFile(string path, byte[] file) {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        return File.WriteAllBytesAsync(path, file);
    }

    public async Task WriteAllText(string path, string content) {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, content);
    }

    public void CreateEmptyFile(string path) => File.Create(path).Dispose();

    public void DeleteFile(string path) {
        if(File.Exists(path)) {
            File.Delete(path);
        }
    }

    public void MoveFile(string currentPath, string newPath) {
        Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
        File.Move(currentPath, newPath, true);
    }

    public void CopyFile(string currentPath, string newPath, bool overwrite = false) {
        File.Copy(currentPath, newPath, overwrite);
    }

    public void MoveDirectory(string currentPath, string newPath) => Directory.Move(currentPath, newPath);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public void DeleteDirectory(string path) {
        //SOLUTION: https://stackoverflow.com/questions/1701457/directory-delete-doesnt-work-access-denied-error-but-under-windows-explorer-it
        var dir = new DirectoryInfo(path);
        if(dir.Exists) {
            NormalizeAttributesRecursive(dir);
            dir.Delete(true);
        }
    }
    private void NormalizeAttributesRecursive(DirectoryInfo dir) {
        foreach(var subDir in dir.GetDirectories())
            NormalizeAttributesRecursive(subDir);
        foreach(var file in dir.GetFiles()) {
            file.Attributes = FileAttributes.Normal;
        }
        dir.Attributes = FileAttributes.Normal;
    }

    public void DeleteFileOrDirectory(string path) {
        if(!IsPathExist(path)) return;
        var targetAttributes = File.GetAttributes(path);

        if(targetAttributes.HasFlag(FileAttributes.Directory)) {
            DeleteDirectory(path);
        }
        else {
            DeleteFile(path);
        }
    }

    public T? DeserializeJsonSync<T>(string path) {
        if(!File.Exists(path)) {
            return default;
        }

        return JsonSerializer.Deserialize<T>(File.ReadAllBytes(path));
    }

    public async Task<T?> DeserializeJson<T>(string path) {
        if(!File.Exists(path)) {
            return default;
        }

        return JsonSerializer.Deserialize<T>(await File.ReadAllBytesAsync(path));
    }

    public async Task<T?> DeserializeJsonCamelCase<T>(string path) {
        byte[] fileBytes = await File.ReadAllBytesAsync(path);

        return JsonSerializer.Deserialize<T>(fileBytes, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public async Task<T?> DeserializeMsgpack<T>(string path) {
        if(!File.Exists(path)) {
            return default;
        }

        byte[] fileBytes = await File.ReadAllBytesAsync(path);
        return MessagePackSerializer.Deserialize<T>(fileBytes, ContractlessStandardResolver.Options);
    }

    public void SerializeToJson(string path, dynamic item) {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(item));
    }

    public void SerializeToMsgpack(string path, dynamic item) {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        byte[] fileBytes = MessagePackSerializer.Serialize(item, ContractlessStandardResolver.Options);
        File.WriteAllBytes(path, fileBytes);
    }

    public SixLabors.ImageSharp.Image LoadImage(string path) => SixLabors.ImageSharp.Image.Load(path);

    public MediaInfoWrapper LoadMediaInfo(string path) => new MediaInfoWrapper(path, NullLogger.Instance);
    #endregion
}