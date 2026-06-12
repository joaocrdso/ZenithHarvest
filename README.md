# 🌾 ZenithHarvest

Sistema para gestão de sinistros agrícolas utilizando indicadores de vegetação (NDVI), permitindo que seguradoras realizem análises mais rápidas e precisas sobre perdas causadas por eventos climáticos.

## Integrantes:

João dos Santos Cardoso de Jesus - RM560400<br>

Davi Praxedes Santos Silva - RM560719<br>

Kauê Vinicius Samartino da Silva - RM559317<br>

Alexis Ronaldo Quirijota Rondo - RM560384<br>

---

## 📹 Vídeos Obrigatórios

Demonstração:https://youtu.be/rERNOWs4jnc

Pitch:

---

## 🎯 Objetivo

O agronegócio está sujeito a diversos riscos climáticos que podem causar prejuízos significativos aos produtores rurais. O processo tradicional de análise de sinistros costuma ser lento e dependente de vistorias presenciais.

A **ZenithHarvest** propõe uma solução baseada em tecnologia para auxiliar seguradoras na análise de sinistros agrícolas por meio da integração de informações de vegetação obtidas por satélites, permitindo maior agilidade na tomada de decisão.

---

## 📋 Índice

1. [Quick Start](#-quick-start)
2. [Arquitetura](#-arquitetura-da-solução)
3. [Desenvolvimento](#-desenvolvimento)
4. [Endpoints](#-endpoints)
5. [Testes](#-testes)
6. [Troubleshooting](#-troubleshooting)

---

## 🚀 Quick Start (8 minutos)

### Pré-requisitos

- **.NET SDK 10.0**
- **Docker Desktop**
- **Git**

### 1. Clonar o projeto

```bash
git clone https://github.com/joaocrdso/ZenithHarvest.git
cd zenith-harvest-dotnet
dotnet restore
```

### 2. Subir o Oracle

```powershell
docker run -d -e ORACLE_PASSWORD=oracle123 -p 1521:1521 --name oracle-db gvenzl/oracle-free:latest
```

Aguarde até aparecer `DATABASE IS READY TO USE!`:

```powershell
docker logs -f oracle-db
```

Quando aparecer a mensagem, pressione **CTRL+C**.

### 3. Criar usuário da aplicação

```powershell
docker exec -it oracle-db sqlplus sys/oracle123@localhost:1521/freepdb1 as sysdba
```

**Dentro do SQL, execute:**

```sql
ALTER SESSION SET CONTAINER=FREEPDB1;
CREATE USER zenith_user IDENTIFIED BY zenith123;
GRANT CONNECT, RESOURCE, UNLIMITED TABLESPACE TO zenith_user;
COMMIT;
EXIT;
```

### 4. Configurar Connection String

Edite `src/ZenithHarvest.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost:1521/freepdb1;User Id=zenith_user;Password=zenith123;"
  },
  "Jwt": {
    "Secret": "PLACEHOLDER_256_BIT_SECRET_KEY_CHANGE_IN_PRODUCTION_MIN_32_CHARS"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 5. Aplicar migrations

```powershell
dotnet ef database update --project src/ZenithHarvest.Infrastructure `
  --connection "Data Source=localhost:1521/freepdb1;User Id=zenith_user;Password=zenith123;"
```

**Esperado:**
```
Applying migration '20260606031856_InitialCreate'.
Done.
```

### 6. Verificar tabelas criadas

```powershell
docker exec -it oracle-db sqlplus zenith_user/zenith123@localhost:1521/freepdb1
```

```sql
SELECT table_name FROM user_tables;
```

**Esperado:**
```
__EFMigrationsHistory
Insurers
Policies
Users
Claims
```

### 7. Criar dados de teste

**Seguradora:**

```sql
INSERT INTO "Insurers"
("Id","CNPJ","Nome","CodigoSUSEP","DataCriacao")
VALUES
(1,'12.345.678/0001-99','Zenith Seguros','123456',SYSTIMESTAMP);
COMMIT;
```

**Usuário (email: admin@zenith.com, senha: 123456):**

```sql
INSERT INTO "Users"
("Id","InsurerId","Email","PasswordHash","Role","Ativo","DataCriacao")
VALUES
(1,1,'admin@zenith.com',
'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=',
'Admin',1,SYSTIMESTAMP);
COMMIT;
```

**Apólice:**

```sql
INSERT INTO "Policies"
("Id","InsurerId","NumeroApolice","VigenciaInicio","VigenciaFim","Premio","Status","DataCriacao")
VALUES
(1,1,'AP-2026-001',TRUNC(SYSTIMESTAMP),TRUNC(SYSTIMESTAMP)+365,1500.00,'Ativa',SYSTIMESTAMP);
COMMIT;
EXIT;
```

### 8. Executar a API

```powershell
dotnet run --project src/ZenithHarvest.Api --launch-profile https
```

**Esperado:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7177
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5032
```

### 9. Acessar a documentação

Abra no navegador:

```
https://localhost:7177/scalar/v1
```

✅ **Pronto!** A API está funcionando com documentação interativa.

---

## 🏗️ Arquitetura da Solução

A aplicação foi desenvolvida seguindo os princípios de **Clean Architecture**, garantindo separação de responsabilidades, facilidade de manutenção e escalabilidade.

### Diagrama de Camadas

```
┌──────────────────────────────────────────────┐
│  API Layer (Controllers, JWT, Middleware)    │
│  - AuthController                            │
│  - PoliciesController                        │
│  - ExceptionHandlerMiddleware                │
└──────────────────┬───────────────────────────┘
                   │ (Dependency Injection)
┌──────────────────▼───────────────────────────┐
│  Application Layer (Use Cases, DTOs)         │
│  - CreateClaimHandler                        │
│  - GetPoliciesByInsurerHandler               │
│  - JwtService, PasswordHasher                │
└──────────────────┬───────────────────────────┘
                   │ (Interfaces)
┌──────────────────▼───────────────────────────┐
│  Domain Layer (Entities, Business Rules)     │
│  - User, Insurer, Policy, Claim              │
│  - IRepository, IJwtService                  │
└──────────────────┬───────────────────────────┘
                   │ (EF Core)
┌──────────────────▼───────────────────────────┐
│  Infrastructure Layer (Database, ORM)        │
│  - ZenithContext (DbContext)                 │
│  - PolicyRepository, ClaimRepository         │
│  - Migrations                                │
└──────────────────────────────────────────────┘
                   │
                   ▼
          Oracle Database 21c
```

### Diagrama de Relacionamentos

```
┌────────────────┐
│    Insurer     │
├────────────────┤
│ Id (PK)        │
│ CNPJ           │
│ Nome           │
│ CodigoSUSEP    │
└────────┬───────┘
         │ 1:N
         ├──────────────┬──────────────┐
         │              │              │
    1:N  ▼              ▼ 1:N          ▼ 1:N
┌──────────────┐  ┌──────────────┐
│   Policy     │  │     User     │
├──────────────┤  ├──────────────┤
│ Id (PK)      │  │ Id (PK)      │
│ InsurerId(FK)│  │ InsurerId(FK)│
│ NumeroApolice│  │ Email        │
│ VigenciaInicio│ │ PasswordHash │
│ VigenciaFim  │  │ Role         │
│ Premio       │  │ Ativo        │
│ Status       │  └──────────────┘
└────────┬─────┘
         │ 1:N
         ▼
┌──────────────────┐
│     Claim        │
├──────────────────┤
│ Id (PK)          │
│ PolicyId (FK)    │
│ NDVIAntes        │
│ NDVIDepois       │
│ ValorSinistro    │
│ Evento           │
│ Status           │
│ DataOcorrencia   │
└──────────────────┘
```

### Fluxo da Aplicação

```
CLIENT REQUEST
    │
    │ HTTP Request
    │
    ▼
┌──────────────────────────────────┐
│  API Layer (Controllers)         │
│  - Valida autenticação           │
│  - Mapeia request para command   │
└──────────────┬───────────────────┘
               │
               ▼
┌──────────────────────────────────┐
│ Application Layer (Use Cases)    │
│  - Executa lógica de negócio    │
│  - Valida regras                 │
└──────────────┬───────────────────┘
               │
               ▼
┌──────────────────────────────────┐
│ Infrastructure Layer (Repos)     │
│  - Acessa dados (EF Core)        │
│  - Interage com banco            │
└──────────────┬───────────────────┘
               │
               ▼
        ORACLE DATABASE
        (Persistência)
               │
               ▼
        Response (DTO)
               │
               ▼
        HTTP Response
```

---

## 🛠️ Desenvolvimento

### Camadas

#### API
Responsável por:
- Exposição dos endpoints REST
- Autenticação
- Configuração da aplicação
- Documentação OpenAPI

#### Application
Responsável por:
- Casos de uso
- Commands e Queries (CQRS)
- DTOs
- Regras de aplicação

#### Domain
Responsável por:
- Entidades
- Interfaces
- Regras de negócio

#### Infrastructure
Responsável por:
- Persistência de dados
- Entity Framework Core
- Repositórios
- Oracle Database

### Padrões Utilizados

✅ **Clean Architecture** - Separação clara de responsabilidades  
✅ **CQRS** - Commands (escrita) e Queries (leitura) separadas  
✅ **Repository Pattern** - Abstração de acesso a dados  
✅ **Dependency Injection** - Gerenciamento de dependências  
✅ **SOLID Principles** - Código limpo e manutenível  

### Stack Tecnológico

| Componente | Tecnologia | Versão |
|-----------|-----------|--------|
| Runtime | .NET | 10.0 |
| Framework Web | ASP.NET Core | 10 |
| ORM | Entity Framework Core | 8 |
| Banco de Dados | Oracle Database | 21c Free |
| Autenticação | JWT | - |
| Testes | xUnit + Moq | - |
| API Docs | OpenAPI + Scalar UI | - |

### Estrutura de Pastas

```
zenith-harvest-dotnet/
├── src/
│   ├── ZenithHarvest.Api/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   └── PoliciesController.cs
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlerMiddleware.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── ZenithHarvest.Application/
│   │   ├── UseCases/
│   │   ├── DTOs/
│   │   └── Security/
│   │
│   ├── ZenithHarvest.Domain/
│   │   ├── Entities/
│   │   └── Interfaces/
│   │
│   └── ZenithHarvest.Infrastructure/
│       ├── Persistence/
│       ├── Repositories/
│       └── Migrations/
│
├── tests/
│   └── ZenithHarvest.Tests/
│
└── README.md
```

---

## 📚 Endpoints

### Autenticação

#### Login

**Request:**
```bash
curl -X POST https://localhost:7177/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@zenith.com",
    "senha": "123456"
  }'
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJhZG1pbkB6ZW5pdGguY29tIiwicm9sZSI6IkFkbWluIiwiZXhwIjoxNzI0Nzc4MDAwfQ.SIGNATURE"
}
```

---

### Apólices

#### Listar Apólices por Seguradora

**Request:**
```bash
curl -X GET https://localhost:7177/api/policies/1 \
  -H "Authorization: Bearer eyJhbGc..."
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "insurerId": 1,
    "numeroApolice": "AP-2026-001",
    "vigenciaInicio": "2025-01-01T00:00:00Z",
    "vigenciaFim": "2026-01-01T00:00:00Z",
    "premio": 1500.00,
    "status": "Ativa"
  }
]
```

#### Criar Sinistro

**Request:**
```bash
curl -X POST https://localhost:7177/api/policies/claims \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGc..." \
  -d '{
    "policyId": 1,
    "ndviAntes": 0.85,
    "ndviDepois": 0.35,
    "valorSinistro": 8500.00,
    "evento": "Seca",
    "dataOcorrencia": "2025-06-15"
  }'
```

**Response (201 Created):**
```json
{
  "id": 1,
  "policyId": 1,
  "ndviAntes": 0.85,
  "ndviDepois": 0.35,
  "valorSinistro": 8500.00,
  "evento": "Seca",
  "status": "Pendente",
  "dataOcorrencia": "2025-06-15T00:00:00Z"
}
```

---

### Health Check

**Request:**
```bash
curl -X GET https://localhost:7177/health
```

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "checks": {
    "oracle-db": "Healthy"
  }
}
```

---

## 🧪 Testes

### Executar Testes

```powershell
dotnet test tests/ZenithHarvest.Tests
```

**Esperado:**
```
Test Run Successful.
Total tests: 12
  Passed: 12
  Failed: 0
```

### Cobertura de Testes

```powershell
dotnet test tests/ZenithHarvest.Tests --collect:"XPlat Code Coverage"
```

### Testes Implementados

✅ **PasswordHasherTests** - Validação de hash de senhas  
✅ **CreateClaimHandlerTests** - Testes de criação de sinistros  
✅ **GetPoliciesByInsurerHandlerTests** - Testes de listagem de apólices

### Exemplo de Teste Unitário

```csharp
[Fact]
public async Task Handle_WithValidPolicy_CreatesClaim()
{
    // Arrange
    var mockPolicyRepo = new Mock<IPolicyRepository>();
    var mockClaimRepo = new Mock<IClaimRepository>();
    
    var policy = new Policy 
    { 
        Id = 1, 
        InsurerId = 1, 
        Status = "Ativa" 
    };
    
    mockPolicyRepo
        .Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(policy);

    var handler = new CreateClaimHandler(mockPolicyRepo.Object, mockClaimRepo.Object);
    
    var command = new CreateClaimCommand(
        PolicyId: 1,
        NDVIAntes: 0.85m,
        NDVIDepois: 0.35m,
        ValorSinistro: 8500m,
        Evento: "Seca",
        DataOcorrencia: DateTime.Now
    );

    // Act
    var result = await handler.Handle(command);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.PolicyId);
    Assert.Equal("Pendente", result.Status);
    
    // Verifica se repositório foi chamado
    mockClaimRepo.Verify(
        r => r.AddAsync(It.IsAny<Claim>()), 
        Times.Once
    );
}
```

---

## 🆘 Troubleshooting

### ❌ ORA-12514: Serviço não encontrado

**Verifique:**
- Serviço `freepdb1` (não `XEPDB1`)
- Connection String
- Oracle totalmente inicializado (5-10 minutos)

**Solução:**
```powershell
docker logs oracle-db | Select-String "DATABASE IS READY"
```

Se não aparecer, recrie o container:
```powershell
docker stop oracle-db && docker rm oracle-db
docker run -d -e ORACLE_PASSWORD=oracle123 -p 1521:1521 --name oracle-db gvenzl/oracle-free:latest
docker logs -f oracle-db
```

---

### ❌ ORA-01017: Usuário/senha inválidos

**Solução:**
```powershell
docker exec -it oracle-db sqlplus sys/oracle123@localhost:1521/freepdb1 as sysdba
```

```sql
ALTER SESSION SET CONTAINER=FREEPDB1;
DROP USER zenith_user CASCADE;
CREATE USER zenith_user IDENTIFIED BY zenith123;
GRANT CONNECT, RESOURCE, UNLIMITED TABLESPACE TO zenith_user;
COMMIT;
EXIT;
```

---

### ❌ ORA-01920: Nome de usuário já existe

**Solução:**
```sql
DROP USER zenith_user CASCADE;
```

Depois recrie o usuário.

---

### ❌ ORA-02291: Erro de chave estrangeira

**Causa:** Falta de registro em tabela pai (ex: Insurers)

**Solução:** Crie primeiro a Seguradora antes de criar Usuários/Apólices

---

### ❌ Migration falha

**Solução:** Use connection string explícita

```powershell
dotnet ef database update --project src/ZenithHarvest.Infrastructure `
  --connection "Data Source=localhost:1521/freepdb1;User Id=zenith_user;Password=zenith123;"
```

---

### ⚠️ NuGet Warning (Microsoft.Extensions.Caching.Memory)

**Solução:**
```powershell
dotnet add package Microsoft.Extensions.Caching.Memory --version 9.0.0 `
  --project src/ZenithHarvest.Infrastructure
```

---

## 🔐 Credenciais de Teste

**Email:** `admin@zenith.com`  
**Senha:** `123456`

---

## 🔐 Segurança

O sistema utiliza:

- ✅ JWT Authentication
- ✅ Autorização baseada em token
- ✅ Tratamento global de exceções
- ✅ Validação de entrada de dados
- ✅ Health Checks

---

## ✅ Funcionalidades Implementadas

- ✅ Clean Architecture
- ✅ CQRS
- ✅ Repository Pattern
- ✅ Oracle Database
- ✅ Entity Framework Core
- ✅ JWT Authentication
- ✅ OpenAPI/Scalar
- ✅ Health Checks
- ✅ Migrations
- ✅ Testes Automatizados
- ✅ Tratamento Global de Exceções

---

## 📄 Licença

Projeto desenvolvido para fins acadêmicos.

---

**Status:** ✅ Pronto para Avaliação | **Última atualização:** Junho 2026
