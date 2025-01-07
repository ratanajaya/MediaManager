using CoreAPI.AL.Models.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary;
using SharedLibrary.Helpers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace CoreAPI.Services;

public class AuthService(
    IMemoryCache _cache,
    CoreApiConfig _config,
    CensorshipService _cs,
    AuthSetting _authSetting
    )
{
    const string _cacheKey = "PasswordWrongKey";

    public void DisableRouteOnPublicBuild() {
        if(_config.IsPublic) {
            throw new UnauthorizedAccessException("Data changing operations are disabled in public build. Thank you 👍");
        }

        if(_cs.IsCensorshipOn())
            throw new UnauthorizedAccessException("Data changing operations are disabled when censorship is on");
    }

    public string Login(string password) {
        int wrongPasswordCount = 0;
        _cache.TryGetValue(_cacheKey, out wrongPasswordCount);

        if(wrongPasswordCount >= 3) {
            throw new Exception("Application is locked out");
        }

        var hash = $"{password}{_authSetting.Salt}".QuickHash();

        if(!string.Equals(hash, _authSetting.Hash, System.StringComparison.OrdinalIgnoreCase)) {
            _cache.Set(_cacheKey, ++wrongPasswordCount);

            throw new Exception("Wrong Password");
        }

        _cache.Set(_cacheKey, 0);

        var loginExpirationDate = DateTime.Now.AddDays(_authSetting.TokenExpiration);

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authSetting.SecretKey));
        var token = new JwtSecurityToken(
            expires: loginExpirationDate,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenStr;
    }
}
