using System.Collections.Generic;

namespace CoreAPI.AL.Models.Dashboard;

public class TablePaginationModel<T>
{
    public int TotalItem { get; set; }
    public int TotalPage { get; set; }
    public List<T> Records { get; set; }
}
