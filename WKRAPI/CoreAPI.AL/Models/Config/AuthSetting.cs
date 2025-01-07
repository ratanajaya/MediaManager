namespace CoreAPI.AL.Models.Config;

public class AuthSetting
{
    public required string Salt { get; set; }
    public required string Hash { get; set; }
    public required string SecretKey { get; set; }
    public int TokenExpiration { get; set; }
}
