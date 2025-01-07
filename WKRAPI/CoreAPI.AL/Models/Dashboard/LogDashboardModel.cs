using SharedLibrary.Models;
using System;

namespace CoreAPI.AL.Models.Dashboard;

public class LogDashboardModel
{
    public required string Id { get; set; }
    public required string AlbumFullTitle { get; set; }
    public required string Operation { get; set; }
    public DateTime CreationTime { get; set; }

    public Album? Album { get; set; }
}
