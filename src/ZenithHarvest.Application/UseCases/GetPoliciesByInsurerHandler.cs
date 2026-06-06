namespace ZenithHarvest.Application.UseCases;

using DTOs;
using ZenithHarvest.Domain.Interfaces;

// Query
public record GetPoliciesByInsurerQuery(int InsurerId);

// Handler — implementa SRP: responsável só pela execução da lógica
public class GetPoliciesByInsurerHandler
{
    private readonly IPolicyRepository _policyRepository;

    public GetPoliciesByInsurerHandler(IPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<IEnumerable<PolicyDto>> Handle(GetPoliciesByInsurerQuery query)
    {
        var policies = await _policyRepository.GetByInsurerIdAsync(query.InsurerId);
        
        if (!policies.Any())
            throw new KeyNotFoundException($"Nenhuma apólice encontrada para seguradora {query.InsurerId}");

        return policies.Select(p => new PolicyDto(
            p.Id, p.InsurerId, p.NumeroApolice, p.VigenciaInicio, p.VigenciaFim, p.Premio, p.Status));
    }
}
