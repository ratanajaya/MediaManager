using CoreAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController : ControllerBase
{
    AuthService _authService;

    public AuthController(AuthService authService) {
        _authService = authService;
    }

    [HttpPost]
    public IActionResult Login(string password) {
        return Ok(_authService.Login(password));
    }
}