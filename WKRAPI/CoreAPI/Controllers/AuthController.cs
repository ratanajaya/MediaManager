using CoreAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController(
    AuthService _authService
    ) : ControllerBase
{
    [HttpPost]
    public IActionResult Login(string password) {
        return Ok(_authService.Login(password));
    }
}