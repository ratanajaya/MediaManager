namespace CoreAPI.AL.Models.Config;

public class GoogleCred
{
    public required string Type { get; set; }
    public required string ProjectId { get; set; }
    public required string PrivateKeyId { get; set; }
    public required string PrivateKey { get; set; }
    public required string ClientEmail { get; set; }
}
