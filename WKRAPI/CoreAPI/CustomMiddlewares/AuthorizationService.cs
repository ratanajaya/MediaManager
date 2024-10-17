using CoreAPI.AL.Models.Config;
using CoreAPI.Services;
using System;

namespace CoreAPI.CustomMiddlewares;

public class AuthorizationService
{
    CoreApiConfig _config;
    CensorshipService _cs;

    public AuthorizationService(CoreApiConfig config, CensorshipService cs) {
        _config = config;
        _cs = cs;
    }

    public void DisableRouteOnPublicBuild() {
        if(_config.IsPublic){
            throw new UnauthorizedAccessException("Data changing operations are disabled in public build. Thank you 👍");
        }

        if(_cs.IsCensorshipOn())
            throw new UnauthorizedAccessException("Data changing operations are disabled when censorship is on");
    }
}