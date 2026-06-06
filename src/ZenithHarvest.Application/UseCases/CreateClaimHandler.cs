namespace ZenithHarvest.Application.UseCases;

using DTOs;
using ZenithHarvest.Domain.Interfaces;

public record CreateClaimCommand(int PolicyId, decimal NDVIAntes, decimal NDVIDepois, 
    decimal ValorSinistro, string Evento, DateTime DataOcorrencia);

public class CreateClaimHandler
{
    private readonly IClaimRepository _claimRepository;
    private readonly IPolicyRepository _policyRepository;

    public CreateClaimHandler(IClaimRepository claimRepository, IPolicyRepository policyRepository)
    {
        _claimRepository = claimRepository;
        _policyRepository = policyRepository;
    }

    public async Task<ClaimDto> Handle(CreateClaimCommand command)
    {
        var policy = await _policyRepository.GetByIdAsync(command.PolicyId);
        if (policy == null)
            throw new KeyNotFoundException($"Apólice {command.PolicyId} não encontrada");

        var claim = new ZenithHarvest.Domain.Entities.Claim
        {
            PolicyId = command.PolicyId,
            NDVIAntes = command.NDVIAntes,
            NDVIDepois = command.NDVIDepois,
            ValorSinistro = command.ValorSinistro,
            Evento = command.Evento,
            DataOcorrencia = command.DataOcorrencia,
            Status = "Pendente"
        };

        await _claimRepository.AddAsync(claim);

        return new ClaimDto(claim.Id, claim.PolicyId, claim.NDVIAntes, claim.NDVIDepois,
            claim.ValorSinistro, claim.Evento, claim.Status, claim.DataOcorrencia);
    }
}
