namespace ZenithHarvest.Application.DTOs;

public record LoginRequest(string Email, string Senha);
public record LoginResponse(string Token);

public record InsurerDto(int Id, string CNPJ, string Nome, string CodigoSUSEP);

public record PolicyDto(int Id, int InsurerId, string NumeroApolice, 
    DateTime VigenciaInicio, DateTime VigenciaFim, decimal Premio, string Status);

public record ClaimDto(int Id, int PolicyId, decimal NDVIAntes, decimal NDVIDepois, 
    decimal ValorSinistro, string Evento, string Status, DateTime DataOcorrencia);

public record CreateClaimRequest(int PolicyId, decimal NDVIAntes, decimal NDVIDepois, 
    decimal ValorSinistro, string Evento, DateTime DataOcorrencia);
