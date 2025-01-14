﻿using CoreAPI.AL.DataAccess;
using CoreAPI.AL.Models.Config;
using CoreAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SharedLibrary;
using System;
using System.IO;

namespace CoreAPI.Controllers;

[ApiController]
[Route("")]
public class TestController(
    CoreApiConfig _config,
    ILogger _logger,
    CensorshipService _cs,
    ISystemIOAbstraction _io,
    ILogDbContext _logDb
    ) : ControllerBase
{
    [HttpGet]
    public IActionResult GetJson() {
        var version = new Func<string>(() => {
            try {
                DateTime dllWriteTime = _io.GetLastWriteTime(Path.Combine(Directory.GetCurrentDirectory(), "CoreAPI.dll"));

                return dllWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch(Exception e) {
                return e.Message;
            }
        })();

        var logDbStatus = new Func<(bool, bool?, string?)>(() => {
            try {
                var isAccessible = _cs.IsAccessible();

                var useCensorship = _cs.IsCensorshipOn();

                return (isAccessible, useCensorship, null);
            }
            catch(Exception e) {
                return (false, null, e.Message);
            }
        })();
        

        return Ok(new {
            Version = version,
            BuildType = _config.BuildType,
            LibraryPath = _config.LibraryPath,
            LogDbStatus = new { 
               Accessible = logDbStatus.Item1,
               UseCensorship = logDbStatus.Item2,
               Message = logDbStatus.Item3
            }
        });
    }

    [HttpGet("Wololo")]
    public IActionResult Wololo() {
        _logger.Warning("Wololo triggered");

        return Ok(new {
            Message = "Wololo"
        });
    }

    [HttpPost]
    public IActionResult TempRoute() {
        _logDb.Devtool_OneTimeOperation();
        return Ok();
    }
}