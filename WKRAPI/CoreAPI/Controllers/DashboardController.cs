using CoreAPI.AL.Services;
using CoreAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CoreAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class DashboardController(
    CensorshipService _cs,
    DashboardService _dash
    ) : ControllerBase
{
    [HttpGet]
    public IActionResult GetQueryTierFractions(string query) {
        var decensoredQuery =_cs.ConDecensorQuery(query);

        var data = _dash.GetTierFractionFromQuery(decensoredQuery, decensoredQuery);

        return Ok(_cs.ConCensorTierFractionModel(data));
    }

    [HttpGet]
    public IActionResult GetGenreTierFractions() {
        var data = _dash.GetGenreTierFractions();

        return Ok(_cs.ConCensorTierFractionModels(data));
    }

    [HttpGet]
    public IActionResult GetLogs(int page, int row, string? operation, string? freeText, DateTime? startDate, DateTime? endDate) {
        var result = _dash.GetLogs(page, row, operation, freeText, startDate, endDate);

        result.Records = _cs.ConCensorLogDashboardModels(result.Records);
        return Ok(result);
    }

    [HttpGet]
    public IActionResult GetDeleteLogs(string? query, bool? includeAlbum) {
        var data = _dash.GetDeleteLogs(query, includeAlbum);

        return Ok(_cs.ConCensorLogDashboardModels(data));
    }

    [HttpGet]
    public IActionResult GetTagForceGraphData() {
        var data = _dash.GetTagForceGraphData();

        return Ok(_cs.ConCensorTagForceGraphData(data));
    }
}