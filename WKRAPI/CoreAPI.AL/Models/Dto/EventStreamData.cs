namespace CoreAPI.AL.Models.Dto;

public class EventStreamData
{
    public bool IsError { get; set; }
    public int MaxStep { get; set; }
    public int CurrentStep { get; set; }
    public string Message { get; set; }

    public EventStreamData(int maxStep, int currentStep, string message, bool isError = false)
    {
        MaxStep = maxStep;
        CurrentStep = currentStep;
        Message = message;
        IsError = isError;
    }
    public EventStreamData()
    {

    }
}