namespace ZenithHarvest.Tests.UseCases;

using Microsoft.EntityFrameworkCore;
using Xunit;
using ZenithHarvest.Application.UseCases;
using ZenithHarvest.Domain.Entities;
using ZenithHarvest.Infrastructure.Persistence;
using ZenithHarvest.Infrastructure.Repositories;

public class GetPoliciesByInsurerHandlerTests
{
    private static ZenithContext BuildInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ZenithContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ZenithContext(options);
    }

    [Fact]
    public async Task Handle_WithValidInsurerId_ReturnsPolicies()
    {
        // Arrange
        var context = BuildInMemoryContext();
        var insurer = new Insurer { Id = 1, CNPJ = "12.345.678/0001-00", Nome = "Brasilseg", CodigoSUSEP = "012345" };
        var policy = new Policy 
        { 
            Id = 1, 
            InsurerId = 1, 
            NumeroApolice = "AP-2026-001", 
            VigenciaInicio = DateTime.Now,
            VigenciaFim = DateTime.Now.AddYears(1),
            Premio = 1500m,
            Status = "Ativa"
        };

        context.Insurers.Add(insurer);
        context.Policies.Add(policy);
        await context.SaveChangesAsync();

        var repository = new PolicyRepository(context);
        var handler = new GetPoliciesByInsurerHandler(repository);

        // Act
        var result = await handler.Handle(new GetPoliciesByInsurerQuery(1));

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("AP-2026-001", result.First().NumeroApolice);
    }

    [Fact]
    public async Task Handle_WithInvalidInsurerId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = BuildInMemoryContext();
        var repository = new PolicyRepository(context);
        var handler = new GetPoliciesByInsurerHandler(repository);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new GetPoliciesByInsurerQuery(999)));
    }

    [Fact]
    public async Task Handle_WithMultiplePolicies_ReturnsAll()
    {
        // Arrange
        var context = BuildInMemoryContext();
        var insurer = new Insurer { Id = 1, CNPJ = "12.345.678/0001-00", Nome = "Porto", CodigoSUSEP = "012346" };
        
        context.Insurers.Add(insurer);
        
        for (int i = 1; i <= 3; i++)
        {
            context.Policies.Add(new Policy 
            { 
                Id = i, 
                InsurerId = 1, 
                NumeroApolice = $"AP-{i}", 
                VigenciaInicio = DateTime.Now,
                VigenciaFim = DateTime.Now.AddYears(1),
                Premio = 1000m * i,
                Status = "Ativa"
            });
        }
        
        await context.SaveChangesAsync();

        var repository = new PolicyRepository(context);
        var handler = new GetPoliciesByInsurerHandler(repository);

        // Act
        var result = await handler.Handle(new GetPoliciesByInsurerQuery(1));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }
}
