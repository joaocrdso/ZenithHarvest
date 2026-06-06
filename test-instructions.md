# 🧪 Instruções de Testes - ZenithHarvest

## 📋 Visão Geral

O projeto ZenithHarvest utiliza **xUnit** como framework de testes e segue o padrão **AAA (Arrange, Act, Assert)** para garantir clareza e manutenibilidade.

**Localização dos testes:** `tests/ZenithHarvest.Tests/`

---

## 🚀 Como Executar Testes

### 1. Executar Todos os Testes
```bash
dotnet test tests/ZenithHarvest.Tests
```

**Saída esperada:**
```
Test Run Successful.
Total tests: 8
     Passed: 8
     Failed: 0
```

---

### 2. Executar Testes de um Arquivo Específico
```bash
dotnet test tests/ZenithHarvest.Tests --filter "PasswordHasherTests"
```

---

### 3. Executar com Cobertura de Código
```bash
dotnet test tests/ZenithHarvest.Tests \
  --collect:"XPlat Code Coverage" \
  --logger "trx"
```

Resultado em: `TestResults/` com relatório `.trx`

---

### 4. Executar em Modo Verbose
```bash
dotnet test tests/ZenithHarvest.Tests --verbosity detailed
```

---

## 📚 Suite de Testes

### **PasswordHasherTests** (3 testes)
**Tipo:** Unitário (Domain.Security)  
**Localização:** `tests/ZenithHarvest.Tests/PasswordHasherTests.cs`

#### Teste 1: `Hash_GeneratesConsistentHashForSamePassword`
```csharp
[Fact]
public void Hash_GeneratesConsistentHashForSamePassword()
{
    // Arrange
    var password = "SenhaSegura@123";

    // Act
    var hash1 = PasswordHasher.Hash(password);
    var hash2 = PasswordHasher.Hash(password);

    // Assert
    Assert.Equal(hash1, hash2); // SHA256 é determinístico
}
```
✅ **Valida:** O hash de uma senha é sempre igual (determinístico)

---

#### Teste 2: `Verify_ReturnsTrueForCorrectPassword`
```csharp
[Fact]
public void Verify_ReturnsTrueForCorrectPassword()
{
    // Arrange
    var password = "SenhaSegura@123";
    var hash = PasswordHasher.Hash(password);

    // Act
    var result = PasswordHasher.Verify(password, hash);

    // Assert
    Assert.True(result);
}
```
✅ **Valida:** Verificação com senha correta retorna `true`

---

#### Teste 3: `Verify_ReturnsFalseForIncorrectPassword`
```csharp
[Fact]
public void Verify_ReturnsFalseForIncorrectPassword()
{
    // Arrange
    var password = "SenhaSegura@123";
    var wrongPassword = "OutraSenha@456";
    var hash = PasswordHasher.Hash(password);

    // Act
    var result = PasswordHasher.Verify(wrongPassword, hash);

    // Assert
    Assert.False(result);
}
```
✅ **Valida:** Verificação com senha incorreta retorna `false`

---

### **CreateClaimHandlerTests** (3 testes)
**Tipo:** Integração (Application.UseCases)  
**Localização:** `tests/ZenithHarvest.Tests/CreateClaimHandlerTests.cs`  
**Padrão:** Arrange com InMemory DB, Act com Handler, Assert em resultado

#### Teste 1: `Handle_WithValidPolicy_CreatesClaim`
```csharp
[Fact]
public async Task Handle_WithValidPolicy_CreatesClaim()
{
    // Arrange
    var context = BuildInMemoryContext();
    var insurer = new Insurer 
    { 
        Id = 1, 
        CNPJ = "12.345.678/0001-00", 
        Nome = "Mapfre", 
        CodigoSUSEP = "012347" 
    };
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
    Assert.Equal(1, result.PolicyId);
    Assert.Equal("Pendente", result.Status);
}
```
✅ **Valida:** Sinistro é criado com status "Pendente"

---

#### Teste 2: `Handle_WithInvalidPolicy_ThrowsKeyNotFoundException`
```csharp
[Fact]
public async Task Handle_WithInvalidPolicy_ThrowsKeyNotFoundException()
{
    // Arrange
    var context = BuildInMemoryContext();
    var claimRepository = new ClaimRepository(context);
    var policyRepository = new PolicyRepository(context);
    var handler = new CreateClaimHandler(claimRepository, policyRepository);

    var command = new CreateClaimCommand(
        PolicyId: 999, // Não existe
        NDVIAntes: 0.75m,
        NDVIDepois: 0.31m,
        ValorSinistro: 5000m,
        Evento: "Seca",
        DataOcorrencia: DateTime.Now);

    // Act & Assert
    await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command));
}
```
✅ **Valida:** Sinistro com apólice inexistente lança exceção

---

### **GetPoliciesByInsurerHandlerTests** (2 testes)
**Tipo:** Integração (Application.UseCases)  
**Localização:** `tests/ZenithHarvest.Tests/GetPoliciesByInsurerHandlerTests.cs`

#### Teste 1: `Handle_WithValidInsurerId_ReturnsPolicies`
```csharp
[Fact]
public async Task Handle_WithValidInsurerId_ReturnsPolicies()
{
    // Arrange
    var context = BuildInMemoryContext();
    var insurer = new Insurer 
    { 
        Id = 1, 
        CNPJ = "12.345.678/0001-00", 
        Nome = "Brasilseg", 
        CodigoSUSEP = "012345" 
    };
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
```
✅ **Valida:** Retorna apólice de uma seguradora específica

---

#### Teste 2: `Handle_WithInvalidInsurerId_ReturnsEmpty`
```csharp
[Fact]
public async Task Handle_WithInvalidInsurerId_ReturnsEmpty()
{
    // Arrange
    var context = BuildInMemoryContext();
    var repository = new PolicyRepository(context);
    var handler = new GetPoliciesByInsurerHandler(repository);

    // Act
    var result = await handler.Handle(new GetPoliciesByInsurerQuery(999));

    // Assert
    Assert.Empty(result);
}
```
✅ **Valida:** Seguradora inexistente retorna lista vazia

---

## 🎯 Padrão AAA

Todos os testes seguem o padrão **Arrange, Act, Assert**:

```csharp
[Fact]
public async Task ExemploTest()
{
    // 1. ARRANGE - Preparar dados e dependências
    var dados = CriarDados();
    var servico = new Servico(dados);

    // 2. ACT - Executar a ação
    var resultado = await servico.ExecutarAcao();

    // 3. ASSERT - Validar resultado
    Assert.NotNull(resultado);
}
```

---

## 📊 Cobertura de Testes

| Camada | Testes | Cobertura |
|--------|--------|-----------|
| **Domain (Security)** | 3 (PasswordHasher) | PasswordHasher.cs |
| **Application (UseCases)** | 5 (CreateClaim, GetPolicies) | 100% |
| **Infrastructure (Repositories)** | Integração com InMemory | DbContext validado |

---

## 🔧 Dependency Injection em Testes

### Exemplo: Criando DbContext InMemory
```csharp
private static ZenithContext BuildInMemoryContext()
{
    var options = new DbContextOptionsBuilder<ZenithContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    return new ZenithContext(options);
}
```

**Benefícios:**
- ✅ Rápido (sem I/O real)
- ✅ Isolado (cada teste tem BD próprio)
- ✅ Reproduzível (sem estado compartilhado)

---

## 🚨 Estrutura de Test File

```csharp
namespace ZenithHarvest.Tests.UseCases;

using Xunit;
using Microsoft.EntityFrameworkCore;
using ZenithHarvest.Infrastructure.Persistence;
using ZenithHarvest.Application.UseCases;

public class NomeDoHandlerTests
{
    // Helper: Criar DbContext InMemory
    private static ZenithContext BuildInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ZenithContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ZenithContext(options);
    }

    // Teste 1
    [Fact]
    public async Task Cenario_Condicao_ResultadoEsperado() { }

    // Teste 2
    [Fact]
    public async Task Cenario_OutraCondicao_ResultadoEsperado() { }
}
```

---

## 🎓 Adicionando Novos Testes

### 1. Nomear o Arquivo
```
NomeHandlerTests.cs (para Application)
NomeServiceTests.cs (para Domain/Application)
```

### 2. Criar Classe e Usar `[Fact]`
```csharp
public class NovoHandlerTests
{
    [Fact]
    public async Task MeuNovoTeste()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

### 3. Executar
```bash
dotnet test tests/ZenithHarvest.Tests --filter "NovoHandlerTests"
```

---

## 🐛 Troubleshooting

### ❌ "Test discovery failed"
**Solução:** Adicione atributo `[Fact]` ou `[Theory]`

### ❌ "DbContext already disposed"
**Solução:** Use `Guid.NewGuid().ToString()` para cada teste

### ❌ "Async method without await"
**Solução:** Use `async Task` e `await` em operações assíncronas

---

## ✅ Checklist para Novos Testes

- [ ] Classe nomeada `*Tests`
- [ ] Método nomeado `Cenario_Condicao_ResultadoEsperado`
- [ ] Atributo `[Fact]` ou `[Theory]`
- [ ] Padrão AAA implementado
- [ ] DbContext InMemory se necessário (sem Oracle real)
- [ ] Assertions específicas
- [ ] Executa com `dotnet test`

---

## 📖 Recursos Adicionais

- [xUnit Documentation](https://xunit.net/)
- [Entity Framework In-Memory Provider](https://docs.microsoft.com/en-us/ef/core/providers/in-memory/)
- [Testing Best Practices](https://github.com/joaocrdso/ZenithHarvest/wiki/Testing-Guide)

---

**Última atualização:** Junho 2026
