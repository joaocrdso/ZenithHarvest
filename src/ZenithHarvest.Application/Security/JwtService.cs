namespace ZenithHarvest.Application.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ZenithHarvest.Domain.Entities;

public interface IJwtService
{
    string GenerateToken(User user);
}

public class JwtService : IJwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly int _expirationMinutes;

    public JwtService(string secret, string issuer = "ZenithHarvest", int expirationMinutes = 60)
    {
        _secret = secret;
        _issuer = issuer;
        _expirationMinutes = expirationMinutes;
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new System.Security.Claims.Claim(ClaimTypes.Email, user.Email),
            new System.Security.Claims.Claim(ClaimTypes.Role, user.Role),
            new System.Security.Claims.Claim("InsurerId", user.InsurerId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
