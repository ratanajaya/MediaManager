using System;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json;
using SharedLibrary.Models;
using SharedLibrary;
using BenchmarkDotNet.Running;
using ZBenchmark;


BenchmarkRunner.Run<FileSystemBenchmark2>();


//var param = new UpscaleCompressApiParam {
//    UpscaleMultiplier = 2,
//    UpscalerType = UpscalerType.Waifu2xCunet,
//    CorrectionType = FileCorrectionType.Upscale,
//    Compression = new CompressionCondition {
//        Width = 537,
//        Height = 670,
//        Quality = 90
//    },
//    ToJpeg = true,
//    Extension = ".png"
//};

//string paramJson = JsonConvert.SerializeObject(param);

//using(var client = new HttpClient()) {
//    using(var form = new MultipartFormDataContent()) {
//        form.Add(new StringContent(paramJson), "paramJson");

//        // Replace with your file path
//        var filePath = "D:\\TestFile1.png";
//        var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(filePath));
//        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
//        form.Add(fileContent, "file", "filename");

//        var response = await client.PostAsync("https://localhost:44367/Image/UpscaleCompress", form);

//        var responseStream = await response.Content.ReadAsStreamAsync();
//        var outputPath = "D:\\ResultFile.png";

//        using(var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
//            await responseStream.CopyToAsync(fileStream);
//        }
//    }
//}

Console.ReadLine();