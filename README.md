# 🌾 ZenithHarvest - Plataforma de Gestão de Sinistros Agrícolas

[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)
[![Status](https://img.shields.io/badge/Status-Active-success)](https://github.com/joaocrdso/ZenithHarvest)

## 📋 Visão Geral

**ZenithHarvest** é uma plataforma backend para gestão de sinistros em seguros agrícolas, utilizando índices de vegetação (NDVI) para análise de perdas em lavouras. A aplicação implementa Clean Architecture com ASP.NET Core 10, Entity Framework Core, Oracle Database e autenticação JWT.

### 🎯 Objetivo Principal
Fornecer uma API REST robusta para cadastro, processamento e análise de sinistros agrícolas com base em dados de vegetação, permitindo que seguradoras automatizem a validação de perdas.

### 🔑 Tecnologias Principais
- **Runtime**: .NET 10.0
- **Framework**: ASP.NET Core 10
- **ORM**: Entity Framework Core 8
- **Banco de Dados**: Oracle Database
- **Autenticação**: JWT (JSON Web Tokens)
- **Testes**: xUnit + InMemory Database
- **API Documentation**: OpenAPI + Scalar UI

---

## 🏗️ Arquitetura

### Diagrama de Camadas

```
┌─────────────────────────────────────────────────┐
│         API Layer (ASP.NET Core)                │
│  - Controllers (Auth, Policies)                 │
│  - Middleware (Exception Handler)               │
│  - Program.cs (DI Configuration)                │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────┐
│    Application Layer (Use Cases & Services)     │
│  - Handlers (CreateClaim, GetPolicies)          │
│  - DTOs (Data Transfer Objects)                 │
│  - Security (JWT, Password Hashing)             │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────┐
│        Domain Layer (Business Logic)            │
│  - Entities (User, Policy, Claim, Insurer)      │
│  - Interfaces (Repository Contracts)            │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────┐
│  Infrastructure Layer (Data Access & Tools)     │
│  - DbContext (ZenithContext)                    │
│  - Repositories (Policy, Claim, User, Insurer)  │
│  - Migrations (EF Core Versioning)              │
└─────────────────────────────────────────────────┘
```

### Relacionamentos de Entidades

```
┌──────────────┐
│   Insurer    │ (Seguradora)
├──────────────┤
│ ID (PK)      │
│ CNPJ         │
│ Nome         │
│ CodigoSUSEP  │
└────┬─────────┘
     │ 1:N
     ├─────────────────┬──────────────────┐
     │                 │                  │
     ▼ 1:N             ▼ 1:N              ▼ 1:N
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│   Policy     │  │     User     │  │   (unused)   │
├──────────────┤  ├──────────────┤  └──────────────┘
│ ID (PK)      │  │ ID (PK)      │
│ InsurerId    │  │ InsurerId    │
│ NumeroApolice│  │ Email        │
│ VigenciaInicio  │ PasswordHash │
│ VigenciaFim  │  │ Role         │
│ Premio       │  │ Ativo        │
│ Status       │  └──────────────┘
└────┬─────────┘
     │ 1:N
     ▼
┌──────────────┐
│    Claim     │ (Sinistro)
├──────────────┤
│ ID (PK)      │
│ PolicyId (FK)│
│ NDVIAntes    │
│ NDVIDepois   │
│ ValorSinistro│
│ Evento       │
│ Status       │
│ DataOcorrencia
└──────────────┘
```

---

## 🚀 Setup & Instalação

### Pré-requisitos
- **.NET SDK 10.0 (Obrigatório - o SDK 9.0 ou inferior NÃO compila este projeto)**: [Baixar](https://dotnet.microsoft.com/download)
- **Visual Studio 2022+** ou **VS Code** com a extensão C# Dev Kit
- **Oracle Database 19c+** ou **Oracle Express Edition**
- **Git**

### Passo 1: Clonar o Repositório
```bash
git clone https://github.com/joaocrdso/ZenithHarvest.git
cd ZenithHarvest
```

### Passo 2: Restaurar Dependências
```bash
dotnet restore
```

### Passo 3: Configurar Banco de Dados

#### Opção A: Oracle Real
Edite `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Oracle": "User Id=SYSTEM;Password=YourPassword;Data Source=127.0.0.1:1521/xe"
  }
}
```

#### Opção B: Oracle Docker (Recomendado para Dev)
```bash
docker run -d -e ORACLE_PASSWORD=oracle123 -e APP_USER=zenith_user -e APP_USER_PASSWORD=zenith123 -p 1521:1521 --name oracle-db gvenzl/oracle-xe:latest
```

Depois configure em `appsettings.Development.json` (use a mesma senha definida em APP_USER_PASSWORD):
```json
{
  "ConnectionStrings": {
    "Oracle": "User Id=zenith_user;Password=zenith123;Data Source=localhost:1521/XEPDB1;"
  }
}
```

### Passo 4: Executar Migrations
```bash
dotnet ef database update --project src/ZenithHarvest.Infrastructure
```
*Nota:* ao configurar o banco pela Opção B (Docker), o comando acima pode falhar com `ORA-01017`, porque o EF não usa Development nos comandos de design-time. Nesse caso, passe a connection string explicitamente:

```bash
dotnet ef database update --project src/ZenithHarvest.Infrastructure --connection "User Id=zenith_user;Password=zenith123;Data Source=localhost:1521/XEPDB1"
```
### Passo 5: Executar a Aplicação
```bash
dotnet run --project src/ZenithHarvest.Api --launch-profile https
```

**Saída esperada:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7177
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5032
```

---

## 🧪 Testes Automatizados

### Executar Todos os Testes
```bash
dotnet test tests/ZenithHarvest.Tests
```

### Executar com Cobertura
```bash
dotnet test tests/ZenithHarvest.Tests --collect:"XPlat Code Coverage"
```

### Testes Inclusos

| Teste | Tipo | Descrição |
|-------|------|-----------|
| `PasswordHasherTests` | Unitário | Verificar hash e validação de senhas |
| `CreateClaimHandlerTests` | Integração | Criar sinistro com banco InMemory |
| `GetPoliciesByInsurerHandlerTests` | Integração | Listar apólices por seguradora |

Detalhes em [test-instructions.md](test-instructions.md)

---

## 📚 API Documentation

### Swagger/OpenAPI
```
https://localhost:7177/scalar/v1
```

Acesse para visualizar todos os endpoints, modelos de dados e testar requisições interativamente.

---

## 🔐 Autenticação

### Fluxo JWT

1. **Login**: POST `/api/auth/login` → Recebe token JWT
2. **Usar Token**: Adicione header `Authorization: Bearer {token}` em requisições protegidas

### Criação de Usuário de Teste
```sql
INSERT INTO Users (InsurerId, Email, PasswordHash, Role, Ativo, DataCriacao)
VALUES (1, 'teste@seguros.com', 'HASH_SHA256_AQUI', 'Analista', 1, SYSDATE);
```

---

## 🧬 Exemplos de Requisições

### 1. Login (Obter Token JWT)
```bash
curl -X POST http://localhost:5032/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "teste@seguros.com",
    "senha": "senha123"
  }'
```

**Resposta (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

---

### 2. Listar Apólices por Seguradora
```bash
curl -X GET http://localhost:5032/api/policies/1 \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

**Resposta (200 OK):**
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

---

### 3. Criar Sinistro (Claim)
```bash
curl -X POST http://localhost:5032/api/policies/claims \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_TOKEN_JWT" \
  -d '{
    "policyId": 1,
    "ndviAntes": 0.80,
    "ndviDepois": 0.31,
    "valorSinistro": 5000.00,
    "evento": "Seca",
    "dataOcorrencia": "2025-06-15"
  }'
```

**Resposta (201 Created):**
```json
{
  "id": 1,
  "policyId": 1,
  "ndviAntes": 0.80,
  "ndviDepois": 0.31,
  "valorSinistro": 5000.00,
  "evento": "Seca",
  "status": "Pendente",
  "dataOcorrencia": "2025-06-15T00:00:00Z"
}
```

---

### 4. Health Check (Sem Autenticação)
```bash
curl -X GET http://localhost:5032/health
```

**Resposta (200 OK):**
```json
{
  "status": "Healthy",
  "checks": {
    "oracle-db": "Healthy"
  }
}
```

---

## 📂 Estrutura de Pastas

```
zenith-harvest-dotnet/
├── src/
│   ├── ZenithHarvest.Api/              # ASP.NET Core Web API
│   │   ├── Controllers/                 # HTTP Endpoints
│   │   ├── Middleware/                  # Exception Handler
│   │   ├── Program.cs                   # DI Configuration
│   │   └── appsettings*.json            # Configuration
│   │
│   ├── ZenithHarvest.Application/       # Business Logic & Use Cases
│   │   ├── UseCases/                    # Handlers (CQRS Pattern)
│   │   ├── DTOs/                        # Data Transfer Objects
│   │   └── Security/                    # JWT & Password Hashing
│   │
│   ├── ZenithHarvest.Domain/            # Core Business Rules
│   │   ├── Entities/                    # Domain Models
│   │   └── Interfaces/                  # Repository Contracts
│   │
│   └── ZenithHarvest.Infrastructure/    # Data Access & Tools
│       ├── Persistence/                 # DbContext & Migrations
│       └── Repositories/                # Repository Implementations
│
├── tests/
│   └── ZenithHarvest.Tests/             # xUnit Test Suite
│       ├── *HandlerTests.cs
│       └── *SecurityTests.cs
│
├── .gitignore                            # Git Configuration
├── README.md                             # This File
├── ARCHITECTURE.md                       # Technical Details
└── test-instructions.md                  # Test Guide
```

---

## 🔍 SOLID Principles

Este projeto implementa todos os 5 princípios SOLID:

- **S**RP (Single Responsibility): Cada classe tem uma responsabilidade única
  - `CreateClaimHandler` → Cria sinistros
  - `JwtService` → Gerencia tokens JWT
  - `PasswordHasher` → Hasha senhas

- **O**CP (Open/Closed): Código aberto para extensão, fechado para modificação
  - Interfaces de repositório permitem novas implementações sem alterar código existente

- **L**SP (Liskov Substitution): Implementações substituem interfaces sem quebrar comportamento
  - Qualquer `IRepository` pode substituir outra

- **I**SP (Interface Segregation): Interfaces pequenas e específicas
  - `IJwtService` vs `IPasswordHasher` (não uma interface gigante)

- **D**IP (Dependency Inversion): Dependa de abstrações, não de implementações
  - Injeção de `IClaimRepository` em vez de `ClaimRepository`

Veja [ARCHITECTURE.md](ARCHITECTURE.md) para detalhes.

---

## 🎓 Diferencial & Inovação

- **NDVI (Normalized Difference Vegetation Index)**: Uso de índices de vegetação para análise automática de perdas agrícolas
- **Clean Architecture**: Separação clara de responsabilidades facilitando manutenção e testes
- **CQRS Pattern**: Handlers separados para cada operação (GetPolicies, CreateClaim)
- **Testabilidade**: 100% das camadas cobertas com testes

---

## 📝 Padrões Implementados

| Padrão | Uso |
|--------|-----|
| **Repository Pattern** | Abstração de acesso a dados |
| **CQRS** | Handlers para operações |
| **Dependency Injection** | IoC Container no Program.cs |
| **Middleware Pattern** | Exception Handling Global |
| **DTO Pattern** | Transferência de dados segura |

---

## 🤝 Contribuindo

1. Fork o repositório
2. Crie uma branch: `git checkout -b feature/sua-feature`
3. Commit: `git commit -m "feat: adiciona sua feature"`
4. Push: `git push origin feature/sua-feature`
5. Abra um Pull Request

---

## 📄 Licença

Este projeto está sob a licença MIT. Veja [LICENSE](LICENSE) para detalhes.

---

## 👨‍💻 Autor

**João Cardoso**
- GitHub: [@joaocrdso](https://github.com/joaocrdso)
- Email: joao@zenithmicrotech.com

---

## 🐛 Issues & Suporte

Encontrou um bug? [Abra uma issue](https://github.com/joaocrdso/ZenithHarvest/issues)

---

## 📊 Status do Projeto

- ✅ Clean Architecture implementada
- ✅ Autenticação JWT funcional
- ✅ Testes automatizados
- ✅ Health Checks configurados
- 🔄 Integração com MongoDB (planejado)
- 🔄 Webhooks para notificações (planejado)
- 🔄 Relatórios em PDF (planejado)

---

**Última atualização**: Junho 2026 | **.NET 10.0**
