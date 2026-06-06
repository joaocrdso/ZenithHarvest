namespace ZenithHarvest.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenithHarvest.Application.DTOs;
using ZenithHarvest.Application.UseCases;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthenticationHandler _authHandler;

    public AuthController(AuthenticationHandler authHandler)
    {
        _authHandler = authHandler;
    }

    /// <summary>
    /// Autenticar usuário e obter token JWT
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authHandler.Handle(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}
