using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using CoreAPI.AL.Services;
using CoreAPI.CustomMiddlewares;
using Microsoft.AspNetCore.Http;
using CoreAPI.AL.Models.Dto;

namespace CoreAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SseController : ControllerBase
{
    AuthorizationService _auth;
    LibraryRepository _library;

    public SseController(AuthorizationService auth, LibraryRepository library) {
        _auth = auth;
        _library = library;
    }

    async Task WriteResponse(EventStreamData data) {
        #region Important Lesson from static
        //NEVER fuck around with static class configurations
        //They may cause weird behavior somewhere else in the application
        //JsonSerializer.SetDefaultResolver(StandardResolver.CamelCase);
        //var OLDdataJson = JsonSerializer.ToJsonString(data);
        #endregion

        var dataJson = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        await Response.WriteAsync($"data: {dataJson}\r\r");
        Response.Body.Flush();
    }

    [HttpGet]
    public async Task TestSse() {
        _auth.DisableRouteOnPublicBuild();

        Response.Headers.Add("Content-Type", "text/event-stream");
        try {
            await WriteResponse(new EventStreamData {
                IsError = false,
                MaxStep = 10,
                CurrentStep = 0,
                Message = "0 - start"
            });

            await Task.Delay(1000);

            for(int i = 1; i <= 10; i++) {
                await WriteResponse(new EventStreamData {
                    IsError = false,
                    MaxStep = 10,
                    CurrentStep = i,
                    Message = $"{i} - step"
                });

                await Task.Delay(1000);
            }
        }
        catch(Exception e) {
            await WriteResponse(new EventStreamData {
                IsError = true,
                MaxStep = 0,
                CurrentStep = 0,
                Message = e.Message
            });
        }
    }

    #region Library Command
    [HttpGet]
    public async Task ReloadDatabase() {
        _auth.DisableRouteOnPublicBuild();

        Response.Headers.Add("Content-Type", "text/event-stream");
        try {
            await _library.ReloadDatabase(WriteResponse);
        }
        catch(Exception e) {
            await WriteResponse(new EventStreamData {
                IsError = true,
                MaxStep = 0,
                CurrentStep = 0,
                Message = e.Message
            });
        }
    }

    [HttpGet]
    public async Task RescanDatabase() {
        _auth.DisableRouteOnPublicBuild();

        Response.Headers.Add("Content-Type", "text/event-stream");
        try {
            await _library.RescanDatabase(WriteResponse);
        }
        catch(Exception e) {
            await WriteResponse(new EventStreamData {
                IsError = true,
                MaxStep = 0,
                CurrentStep = 0,
                Message = e.Message
            });
        }
    }
    #endregion
}
