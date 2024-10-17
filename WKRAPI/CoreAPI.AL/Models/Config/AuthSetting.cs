namespace CoreAPI.AL.Models.Config;

public class AuthSetting
{
    public string Salt { get; set; }
    public string Hash { get; set; }
    public string SecretKey { get; set; }
    public int TokenExpiration { get; set; }
}
