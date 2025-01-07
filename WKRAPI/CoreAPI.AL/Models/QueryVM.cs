namespace CoreAPI.AL.Models;

public class QueryVM
{
    public required string Name { get; set; }
    public int Tier { get; set; }
    public required string Query { get; set; }
}