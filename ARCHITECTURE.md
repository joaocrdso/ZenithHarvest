# 🏗️ Arquitetura & Design - ZenithHarvest

## 📖 Visão Geral

ZenithHarvest implementa **Clean Architecture** seguindo princípios SOLID e padrões de design consolidados. A arquitetura está organizada em 4 camadas independentes.

---

## 🏛️ Clean Architecture

### 4 Camadas Principais

```
┌─────────────────────────────────────────┐
│  Presenters, Controllers, Middleware     │  ◄── API Layer
├─────────────────────────────────────────┤
│  Use Cases, Application Services         │  ◄── Application Layer
├─────────────────────────────────────────┤
│  Entities, Business Rules                │  ◄── Domain Layer
├─────────────────────────────────────────┤
│  Frameworks, Databases, External Tools   │  ◄── Infrastructure Layer
└─────────────────────────────────────────┘
```

### Princípio de Dependência

**As dependências devem apontar para o CENTRO (Domain)**

```
Infrastructure ──► Application ──► Domain ◄─── API
                       │               ▲
                       └───────────────┘
                          (interfaces)
```

---

## 📁 Estrutura de Arquivos

### **API Layer** (`src/ZenithHarvest.Api/`)

**Responsabilidade:** Receber requisições HTTP e rotear para handlers

```
ZenithHarvest.Api/
├── Controllers/
│   ├── AuthController.cs        # Endpoints de autenticação
│   └── PoliciesController.cs     # Endpoints de apólices
├── Middleware/
│   └── ExceptionHandlerMiddleware.cs  # Tratamento global de erros
├── Program.cs                   # Configuração de DI
├── appsettings.json             # Configurações (sem secrets)
└── appsettings.Development.json # Dev config (secrets locais)
```

**Controllers Implementados:**

#### AuthController
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
}
```
- **POST `/api/auth/login`** → Autentica usuário, retorna JWT

#### PoliciesController
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // Todas rotas requerem autenticação
public class PoliciesController : ControllerBase
{
    [HttpGet("{insurerId}")]
    public async Task<ActionResult<IEnumerable<PolicyDto>>> GetByInsurer(int insurerId)

    [HttpPost("claims")]
    public async Task<ActionResult<ClaimDto>> CreateClaim([FromBody] CreateClaimRequest request)
}
```
- **GET `/api/policies/{insurerId}`** → Lista apólices
- **POST `/api/policies/claims`** → Cria sinistro

---

### **Application Layer** (`src/ZenithHarvest.Application/`)

**Responsabilidade:** Implementar casos de uso e orquestração

```
ZenithHarvest.Application/
├── UseCases/
│   ├── GetPoliciesByInsurerHandler.cs    # Query Handler
│   ├── CreateClaimHandler.cs             # Command Handler
│   ├── AuthenticationHandler.cs          # Login Handler
│   └── Queries/Commands.cs               # Request/Response Objects
├── DTOs/
│   └── Dtos.cs                           # Data Transfer Objects
└── Security/
    ├── IJwtService.cs                    # Interface JWT
    ├── JwtService.cs                     # Implementação JWT
    ├── PasswordHasher.cs                 # Hash de senhas
    └── IPasswordHasher.cs                # Interface (opcional)
```

#### Padrão: CQRS (Command Query Responsibility Segregation)

**Commands** (Modificam estado):
```csharp
public record CreateClaimCommand(
    int PolicyId,
    decimal NDVIAntes,
    decimal NDVIDepois,
    decimal ValorSinistro,
    string Evento,
    DateTime DataOcorrencia);
```

**Queries** (Apenas leem estado):
```csharp
public record GetPoliciesByInsurerQuery(int InsurerId);
```

**Handlers** (Executam lógica):
```csharp
public class CreateClaimHandler
{
    private readonly IClaimRepository _claimRepo;
    private readonly IPolicyRepository _policyRepo;

    public async Task<ClaimDto> Handle(CreateClaimCommand command)
    {
        // Lógica de negócio
        var policy = await _policyRepo.GetByIdAsync(command.PolicyId)
            ?? throw new KeyNotFoundException("Apólice não encontrada");

        var claim = new Claim
        {
            PolicyId = command.PolicyId,
            NDVIAntes = command.NDVIAntes,
            // ...
        };

        await _claimRepo.AddAsync(claim);
        return MapToClaim(claim);
    }
}
```

#### DTOs (Data Transfer Objects)

```csharp
// Request DTOs
public record LoginRequest(string Email, string Senha);
public record CreateClaimRequest(
    int PolicyId, 
    decimal NDVIAntes, 
    decimal NDVIDepois, 
    decimal ValorSinistro, 
    string Evento, 
    DateTime DataOcorrencia);

// Response DTOs
public record LoginResponse(string Token);
public record ClaimDto(
    int Id, 
    int PolicyId, 
    decimal NDVIAntes, 
    decimal NDVIDepois, 
    decimal ValorSinistro, 
    string Evento, 
    string Status, 
    DateTime DataOcorrencia);
```

**Benefício:** Desacoplamento entre modelo interno (Domain) e externo (API)

---

### **Domain Layer** (`src/ZenithHarvest.Domain/`)

**Responsabilidade:** Definir regras de negócio puros (sem dependências externas)

```
ZenithHarvest.Domain/
├── Entities/
│   ├── User.cs          # Usuário (email, role, senha)
│   ├── Insurer.cs       # Seguradora (CNPJ, SUSEP)
│   ├── Policy.cs        # Apólice (número, vigência, prêmio)
│   └── Claim.cs         # Sinistro (NDVI, valor, status)
└── Interfaces/
    └── Repositories.cs  # Contracts de Repositórios
```

#### Entidades (Domain Models)

```csharp
public class Insurer
{
    public int Id { get; set; }
    public string CNPJ { get; set; }
    public string Nome { get; set; }
    public string CodigoSUSEP { get; set; }
    
    // Relacionamentos
    public ICollection<Policy> Policies { get; set; }
    public ICollection<User> Users { get; set; }
}

public class Policy
{
    public int Id { get; set; }
    public int InsurerId { get; set; }
    public string NumeroApolice { get; set; }
    public DateTime VigenciaInicio { get; set; }
    public DateTime VigenciaFim { get; set; }
    public decimal Premio { get; set; }
    public string Status { get; set; } // "Ativa", "Cancelada", "Expirada"
    
    // Relacionamentos
    public Insurer Insurer { get; set; }
    public ICollection<Claim> Claims { get; set; }
}

public class Claim
{
    public int Id { get; set; }
    public int PolicyId { get; set; }
    public decimal NDVIAntes { get; set; }  // Índice de vegetação antes
    public decimal NDVIDepois { get; set; } // Índice de vegetação depois
    public decimal ValorSinistro { get; set; }
    public string Evento { get; set; }      // "Seca", "Chuva Excessiva", etc
    public string Status { get; set; }      // "Pendente", "Aprovado", "Recusado", "Pago"
    
    // Relacionamento
    public Policy Policy { get; set; }
}
```

#### Repository Interfaces

```csharp
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// Especializadas
public interface IPolicyRepository : IRepository<Policy>
{
    Task<IEnumerable<Policy>> GetByInsurerIdAsync(int insurerId);
}

public interface IClaimRepository : IRepository<Claim>
{
    Task<IEnumerable<Claim>> GetByPolicyIdAsync(int policyId);
}
```

**Princípio DIP:** Controllers e Handlers dependem de interfaces, não de implementações.

---

### **Infrastructure Layer** (`src/ZenithHarvest.Infrastructure/`)

**Responsabilidade:** Implementar acesso a dados, bancos, APIs externas

```
ZenithHarvest.Infrastructure/
├── Persistence/
│   ├── ZenithContext.cs              # DbContext (EF Core)
│   ├── ZenithContextFactory.cs       # Factory para Migrations
│   └── Migrations/
│       ├── 20260606031856_InitialCreate.cs
│       └── ZenithContextModelSnapshot.cs
└── Repositories/
    ├── BaseRepository.cs             # Implementação genérica
    ├── PolicyRepository.cs
    ├── ClaimRepository.cs
    ├── UserRepository.cs
    └── InsurerRepository.cs
```

#### DbContext (Entity Framework Core)

```csharp
public class ZenithContext : DbContext
{
    public ZenithContext(DbContextOptions<ZenithContext> options) 
        : base(options) { }

    public DbSet<Insurer> Insurers { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mapeamento Fluent API
        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(p => p.Insurer)
                .WithMany(i => i.Policies)
                .HasForeignKey(p => p.InsurerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

#### Repositories

```csharp
public class PolicyRepository : IPolicyRepository
{
    private readonly ZenithContext _context;

    public PolicyRepository(ZenithContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Policy>> GetByInsurerIdAsync(int insurerId)
    {
        return await _context.Policies
            .Where(p => p.InsurerId == insurerId)
            .ToListAsync();
    }
}
```

---

## ✅ SOLID Principles

### 1. **S** - Single Responsibility Principle

Cada classe tem UMA razão para mudar:

```csharp
// ✅ BOM: Cada classe tem uma responsabilidade
class JwtService { /* Gera tokens */ }
class PasswordHasher { /* Valida senhas */ }
class CreateClaimHandler { /* Cria sinistros */ }

// ❌ RUIM: Uma classe com múltiplas responsabilidades
class UtilityService 
{ 
    public string GenerateJwt() { }
    public string HashPassword() { }
    public CreateClaimResponse CreateClaim() { }
}
```

---

### 2. **O** - Open/Closed Principle

Aberto para EXTENSÃO, fechado para MODIFICAÇÃO:

```csharp
// ✅ BOM: Posso adicionar novo repositório sem alterar Handler
public interface IClaimRepository
{
    Task AddAsync(Claim claim);
}

public class CreateClaimHandler
{
    public CreateClaimHandler(IClaimRepository repo) { }
}

// Novo repositório MongoDB? Apenas crie nova implementação!
public class ClaimRepositoryMongo : IClaimRepository { }

// ❌ RUIM: Preciso modificar Handler a cada novo repositório
public class CreateClaimHandler
{
    public void CreateClaimOracleDirect() { }
    public void CreateClaimMongo() { }
    public void CreateClaimSqlServer() { }
}
```

---

### 3. **L** - Liskov Substitution Principle

Implementações devem ser substituíveis sem quebrar:

```csharp
// ✅ BOM: ClaimRepository substitui IPolicyRepository
IPolicyRepository repo = new PolicyRepository(context);
// ou
IPolicyRepository repo = new PolicyRepositoryMongo();
// Ambas funcionam igual!

// ❌ RUIM: Comportamento diferente
class PolicyRepository : IPolicyRepository
{
    public async Task<Policy> GetByIdAsync(int id)
    {
        // Retorna null sem aviso
        return null;
    }
}
```

---

### 4. **I** - Interface Segregation Principle

Interfaces pequenas e específicas:

```csharp
// ✅ BOM: Interfaces segregadas
public interface IJwtService { string GenerateToken(User user); }
public interface IPasswordHasher { string Hash(string pwd); }

// ❌ RUIM: Interface gigante
public interface ISecurityService
{
    string GenerateToken(User user);
    string Hash(string password);
    bool VerifyPassword(string pwd, string hash);
    void InvalidateToken(string token);
    // ... mais 20 métodos
}
```

---

### 5. **D** - Dependency Inversion Principle

Dependa de ABSTRAÇÕES, não de implementações:

```csharp
// ✅ BOM: Depende de interface
public class CreateClaimHandler
{
    private readonly IClaimRepository _repo;
    
    public CreateClaimHandler(IClaimRepository repo)
    {
        _repo = repo; // Pode ser qualquer implementação!
    }
}

// ❌ RUIM: Depende de implementação concreta
public class CreateClaimHandler
{
    private readonly ClaimRepository _repo = new ClaimRepository();
    // Acoplado a uma implementação específica
}
```

**Implementação no Program.cs:**
```csharp
builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Trocar implementação é apenas uma linha!
```

---

## 🔄 Fluxo de Requisição

### Exemplo: Criar Sinistro

```
1. HTTP POST /api/policies/claims
    ├─ Payload: CreateClaimRequest
    └─ Header: Authorization: Bearer {jwt}
        │
2. PoliciesController.CreateClaim()
    ├─ Valida autenticação [Authorize]
    └─ Cria CreateClaimCommand
        │
3. CreateClaimHandler.Handle(command)
    ├─ Busca Policy em IClaimRepository (DIP)
    ├─ Valida regra de negócio
    ├─ Cria entidade Claim
    └─ Persiste via repository
        │
4. Converte para ClaimDto
    └─ Retorna 201 Created
```

---

## 🔐 Autenticação & Autorização

### Fluxo JWT

```
1. LOGIN
   Email + Senha → Hash → Verifica em DB
                           ├─ ✅ Encontrou
                           │  └─ Gera JWT com Claims
                           │     (user.id, user.email, user.role)
                           └─ ❌ Não encontrou → Unauthorized

2. REQUEST AUTENTICADO
   [Authorization: Bearer eyJ...]
       │
       └─ Middleware valida
          ├─ Extrai payload JWT
          ├─ Verifica assinatura (secret key)
          ├─ Valida expiração
          └─ Injeta User em HttpContext

3. [Authorize] VALIDA ACESSO
   Se token válido → Permite requisição
   Se inválido/expirado → 401 Unauthorized
```

### Claims no JWT

```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.Role) // "Analista", "Gerente"
};

var token = new JwtSecurityToken(
    issuer: "ZenithHarvest",
    claims: claims,
    expires: DateTime.UtcNow.AddMinutes(60),
    signingCredentials: credentials
);
```

---

## 🧪 Testabilidade

### Por que essa arquitetura é testável?

1. **Separação de Responsabilidades**: Cada classe faz UMA coisa
2. **Injeção de Dependência**: Posso mockar dependências
3. **Interfaces**: Posso criar implementações fake
4. **Sem Side Effects**: Lógica pura é testável

### Exemplo: Teste com Mock

```csharp
[Fact]
public async Task Handle_WithValidPolicy_CreatesClaim()
{
    // Arrange
    var mockRepo = new Mock<IClaimRepository>();
    mockRepo.Setup(r => r.AddAsync(It.IsAny<Claim>()))
        .Returns(Task.CompletedTask);

    var handler = new CreateClaimHandler(mockRepo.Object);

    // Act
    var result = await handler.Handle(command);

    // Assert
    mockRepo.Verify(r => r.AddAsync(It.IsAny<Claim>()), Times.Once);
}
```

---

## 📊 Mapeamento ORM (Entity Framework Core)

### Fluent API Configuration

```csharp
modelBuilder.Entity<Policy>(entity =>
{
    // Chave primária
    entity.HasKey(e => e.Id);
    
    // Propriedades
    entity.Property(e => e.NumeroApolice)
        .HasMaxLength(50)
        .IsRequired();
    
    entity.Property(e => e.Premio)
        .HasPrecision(12, 2); // 12 dígitos, 2 casas decimais
    
    // Relacionamento 1:N
    entity.HasOne(p => p.Insurer)
        .WithMany(i => i.Policies)
        .HasForeignKey(p => p.InsurerId)
        .OnDelete(DeleteBehavior.Cascade); // Apaga apólices se seguradora deletada
    
    // Index (performance)
    entity.HasIndex(e => e.InsurerId);
});
```

---

## 🚀 Deployment & Escalabilidade

### Considerações Arquiteturais

| Aspecto | Decisão | Razão |
|--------|---------|-------|
| **API Stateless** | Sem sessão em memória | Escalável horizontalmente |
| **JWT em vez de Session** | Token auto-contido | Não precisa de sticky sessions |
| **Repositories** | Pattern de abstração | Trocar BD sem mudar negócio |
| **DTOs** | Separa Domain de API | Versioning de API mais fácil |
| **Async/Await** | Operações I/O assíncronas | Melhor throughput |

---

## 📚 Recursos

- [Clean Architecture por Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [JWT Explained](https://jwt.io/introduction)

---

**Última atualização:** Junho 2026
