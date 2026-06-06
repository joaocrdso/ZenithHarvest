namespace ZenithHarvest.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenithHarvest.Application.DTOs;
using ZenithHarvest.Application.UseCases;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PoliciesController : ControllerBase
{
    private readonly GetPoliciesByInsurerHandler _getPoliciesHandler;
    private readonly CreateClaimHandler _createClaimHandler;

    public PoliciesController(GetPoliciesByInsurerHandler getPoliciesHandler, CreateClaimHandler createClaimHandler)
    {
        _getPoliciesHandler = getPoliciesHandler;
        _createClaimHandler = createClaimHandler;
    }

    /// <summary>
    /// Listar apólices por seguradora
    /// </summary>
    [HttpGet("{insurerId}")]
    public async Task<ActionResult<IEnumerable<PolicyDto>>> GetByInsurer(int insurerId)
    {
        try
        {
            var result = await _getPoliciesHandler.Handle(new GetPoliciesByInsurerQuery(insurerId));
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Criar novo sinistro
    /// </summary>
    [HttpPost("claims")]
    public async Task<ActionResult<ClaimDto>> CreateClaim([FromBody] CreateClaimRequest request)
    {
        try
        {
            var command = new CreateClaimCommand(
                request.PolicyId,
                request.NDVIAntes,
                request.NDVIDepois,
                request.ValorSinistro,
                request.Evento,
                request.DataOcorrencia);

            var result = await _createClaimHandler.Handle(command);
            return CreatedAtAction(nameof(CreateClaim), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
