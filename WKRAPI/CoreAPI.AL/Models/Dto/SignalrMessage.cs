namespace CoreAPI.AL.Models.Dto;

public class SignalrMessage<T>
{
    public SignalrMessageType MessageType { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public enum SignalrMessageType
{
    Progress = 1,
    Complete = 2,
    Error = 3,
}