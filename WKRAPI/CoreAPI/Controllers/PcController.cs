using CoreAPI.AL.Models.Config;
using CoreAPI.AL.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoreAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class PcController(
    IPcService _pc,
    CoreApiConfig _config
    ) : ControllerBase 
{
    [HttpPost]
    public IActionResult Sleep() {
        _pc.Sleep();
        return Ok();
    }

    [HttpPost]
    public IActionResult Hibernate() {
        _pc.Hibernate();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> IpCheck() {
        using(var client = new HttpClient()) {
            client.Timeout = TimeSpan.FromSeconds(5);
            try {
                var response = await client.GetAsync($"{_config.ProcessorApiUrl}/Pc/Check");

                var messsage = await response.Content.ReadAsStringAsync();

                return Ok(new Response {
                    Success = response.IsSuccessStatusCode,
                    Message = messsage
                });
            }
            catch(Exception e) {
                return Ok(new Response {
                    Success = false,
                    Message = e.Message
                });
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> IpSleep() {
        using(var client = new HttpClient()) {
            var response = await client.PostAsync($"{_config.ProcessorApiUrl}/Pc/Sleep", null);

            if(!response.IsSuccessStatusCode)
                return BadRequest(await response.Content.ReadAsStringAsync());

            return Ok();
        }
    }

    [HttpPost]
    public async Task<IActionResult> IpShutdown() {
        using(var client = new HttpClient()) {
            var response = await client.PostAsync($"{_config.ProcessorApiUrl}/Pc/Shutdown", null);

            if(!response.IsSuccessStatusCode)
                return BadRequest(await response.Content.ReadAsStringAsync());

            return Ok();
        }
    }
}
