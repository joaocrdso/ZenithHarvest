namespace ZenithHarvest.Application.UseCases;

using DTOs;
using Security;
using ZenithHarvest.Domain.Interfaces;

public class AuthenticationHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthenticationHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> Handle(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !user.Ativo)
            throw new UnauthorizedAccessException("Email ou senha inválidos");

        if (!PasswordHasher.Verify(request.Senha, user.PasswordHash))
            throw new UnauthorizedAccessException("Email ou senha inválidos");

        var token = _jwtService.GenerateToken(user);
        return new LoginResponse(token);
    }
}
