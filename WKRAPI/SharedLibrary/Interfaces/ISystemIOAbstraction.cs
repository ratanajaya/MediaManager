using MediaInfo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace SharedLibrary;

public interface ISystemIOAbstraction
{
    void CreateDirectory(string path);
    void DeleteDirectory(string path);
    void DeleteFile(string path);
    void DeleteFileOrDirectory(string path);
    T? DeserializeJsonSync<T>(string path);
    Task<T?> DeserializeJson<T>(string path);
    Task<T?> DeserializeMsgpack<T>(string path);
    string[] GetDirectories(string path);
    string[] GetDirectories(string path, SearchOption option);
    string[] GetFiles(string path);
    List<string> GetSuitableFilePaths(string folderPath, string[] suitableFileFormats, int depth);
    List<string> GetSuitableFilePathsWithNaturalSort(string folderPath, string[] suitableFileFormats, int depth);
    bool IsFileExists(string path);
    bool IsDirectoryExist(string path);
    bool IsPathExist(string path);
    void MoveFile(string currentPath, string newPath);
    void MoveDirectory(string currentPath, string newPath);
    Bitmap ReadBitmap(string path);
    byte[] ReadFile(string path);
    Task<string> ReadText(string path);
    void SerializeToJson(string path, dynamic item);
    void SerializeToMsgpack(string path, dynamic item);
    Task WriteAllText(string path, string content);
    Task WriteFile(string path, byte[] file);
    void CreateEmptyFile(string path);
    FileInfo GetFileInfo(string path);
    FileStream OpenRead(string path);
    SixLabors.ImageSharp.Image LoadImage(string path);
    MediaInfoWrapper LoadMediaInfo(string path);
    Stream GetStream(string path);
    void CopyFile(string currentPath, string newPath, bool overwrite = false);
    DateTime GetLastWriteTime(string path);
    Task<T?> DeserializeJsonCamelCase<T>(string path);
}
