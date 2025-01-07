namespace SharedLibrary.Models;

public class QueryModel
{
    public required string Name { get; set; }
    public required string Query { get; set; }
    public int Group { get; set; }
}