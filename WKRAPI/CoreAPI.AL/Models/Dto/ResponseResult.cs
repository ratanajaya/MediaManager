namespace CoreAPI.AL.Models.Dto;

public class Response 
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class ResponseResult<T> : Response
{
    public required T Result { get; set; }
}
