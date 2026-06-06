namespace ZenithHarvest.Tests.UseCases;

using Microsoft.EntityFrameworkCore;
using Xunit;
using ZenithHarvest.Application.UseCases;
using ZenithHarvest.Domain.Entities;
using ZenithHarvest.Infrastructure.Persistence;
using ZenithHarvest.Infrastructure.Repositories;

public class CreateClaimHandlerTests
{
    private static ZenithContext BuildInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ZenithContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ZenithContext(options);
    }

    [Fact]
    public async Task Handle_WithValidPolicy_CreatesClaim()
    {
        // Arrange
        var context = BuildInMemoryContext();
        var insurer = new Insurer { Id = 1, CNPJ = "12.345.678/0001-00", Nome = "Mapfre", CodigoSUSEP = "012347" };
        var policy = new Policy
        {
            Id = 1,
            InsurerId = 1,
            NumeroApolice = "AP-2026-002",
            VigenciaInicio = DateTime.Now,
            VigenciaFim = DateTime.Now.AddYears(1),
            Premio = 2000m,
            Status = "Ativa"
        };

        context.Insurers.Add(insurer);
        context.Policies.Add(policy);
        await context.SaveChangesAsync();

        var claimRepository = new ClaimRepository(context);
        var policyRepository = new PolicyRepository(context);
        var handler = new CreateClaimHandler(claimRepository, policyRepository);

        var command = new CreateClaimCommand(
            PolicyId: 1,
            NDVIAntes: 0.75m,
            NDVIDepois: 0.31m,
            ValorSinistro: 5000m,
            Evento: "Seca",
            DataOcorrencia: DateTime.Now);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Pendente", result.Status);
        Assert.Equal(0.75m, result.NDVIAntes);
        Assert.Equal(0.31m, result.NDVIDepois);
        Assert.Equal(5000m, result.ValorSinistro);
    }

    [Fact]
    public async Task Handle_WithInvalidPolicyId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = BuildInMemoryContext();
        var claimRepository = new ClaimRepository(context);
        var policyRepository = new PolicyRepository(context);
        var handler = new CreateClaimHandler(claimRepository, policyRepository);

        var command = new CreateClaimCommand(999, 0.75m, 0.31m, 5000m, "Seca", DateTime.Now);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(command));
    }

    [Fact]
    public async Task Handle_CreatedClaimPersistsInDatabase()
    {
        // Arrange
        var context = BuildInMemoryContext();
        var insurer = new Insurer { Id = 1, CNPJ = "98.765.432/0001-11", Nome = "TestInsurer", CodigoSUSEP = "999999" };
        var policy = new Policy
        {
            Id = 1,
            InsurerId = 1,
            NumeroApolice = "AP-TEST-001",
            VigenciaInicio = DateTime.Now,
            VigenciaFim = DateTime.Now.AddYears(1),
            Premio = 1500m,
            Status = "Ativa"
        };

        context.Insurers.Add(insurer);
        context.Policies.Add(policy);
        await context.SaveChangesAsync();

        var claimRepository = new ClaimRepository(context);
        var policyRepository = new PolicyRepository(context);
        var handler = new CreateClaimHandler(claimRepository, policyRepository);

        var command = new CreateClaimCommand(1, 0.80m, 0.25m, 7500m, "Chuva Excessiva", DateTime.Now);

        // Act
        var result = await handler.Handle(command);

        // Assert
        var dbClaim = await context.Claims.FindAsync(result.Id);
        Assert.NotNull(dbClaim);
        Assert.Equal("Chuva Excessiva", dbClaim.Evento);
    }
}
