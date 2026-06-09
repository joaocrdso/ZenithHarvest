# 🌾 ZenithHarvest

API REST para gestão de sinistros em seguros agrícolas usando análise de vegetação (NDVI).

**Status:** ✅ Funcional e testado

---

## 🚀 Quick Start (7 minutos)

### 1. Pré-requisitos

- **.NET SDK 10.0** ([Download](https://dotnet.microsoft.com/download))
- **Docker Desktop** ([Download](https://www.docker.com/products/docker-desktop))
- **Git**

### 2. Clonar e restaurar

```bash
git clone <seu-repo>
cd zenith-harvest-dotnet
dotnet restore
```

### 3. Iniciar Oracle em Docker

```powershell
docker run -d -e ORACLE_PASSWORD=oracle123 -p 1521:1521 --name oracle-db gvenzl/oracle-free:latest
```

**Aguarde 3-5 minutos** até o banco estar pronto:

```powershell
docker logs -f oracle-db
```

Procure por: `DATABASE IS READY TO USE!` → Pressione **CTRL+C**

### 4. Criar usuário no banco

```powershell
docker exec -it oracle-db sqlplus sys/oracle123@localhost:1521/freepdb1 as sysdba
```

Cole isto dentro do SQL:

```sql
ALTER SESSION SET CONTAINER=FREEPDB1;
CREATE USER zenith_user IDENTIFIED BY zenith123;
GRANT CONNECT, RESOURCE, UNLIMITED TABLESPACE TO zenith_user;
COMMIT;
EXIT;
```

### 5. Configurar connection string

Abra `src/ZenithHarvest.Api/appsettings.json` e verifique:

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

### 6. Executar migrations

```powershell
dotnet ef database update --project src/ZenithHarvest.Infrastructure `
  --connection "Data Source=localhost:1521/freepdb1;User Id=zenith_user;Password=zenith123;"
```

**Esperado:**
```
Applying migration '20260606031856_InitialCreate'.
Done.
```

### 7. Rodar a aplicação

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

### 8. Acessar a API

Abra no navegador:
```
https://localhost:7177/scalar/v1
```

✅ **Pronto!** A API está funcionando com documentação interativa.

---

## 📚 Endpoints

### Auth
- **POST** `/api/Auth/login` - Fazer login e receber JWT

### Policies
- **GET** `/api/Policies/{insurerId}` - Listar apólices por seguradora
- **POST** `/api/Policies/claims` - Criar novo sinistro

### Health
- **GET** `/health` - Verificar saúde da API

---

## 🏗️ Arquitetura

```
API Layer
  ↓
Application Layer (Handlers, DTOs, Security)
  ↓
Domain Layer (Entities, Interfaces)
  ↓
Infrastructure Layer (DbContext, Repositories)
  ↓
Oracle Database
```

**Padrões:** Clean Architecture, CQRS, Repository Pattern, Dependency Injection

---

## 🧪 Testes

```bash
dotnet test tests/ZenithHarvest.Tests
```

Com cobertura:
```bash
dotnet test tests/ZenithHarvest.Tests --collect:"XPlat Code Coverage"
```

---

## 🆘 Troubleshooting

### ❌ ORA-12514: Serviço não encontrado

**Solução:** Aguarde o Oracle inicializar completamente (5-10 minutos)

```powershell
docker logs oracle-db | Select-String "DATABASE IS READY"
```

---

### ❌ ORA-01017: Usuário/senha inválidos

**Solução:** Recrie o usuário

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

### ❌ Migration falha com "Cannot connect to service"

**Solução:** Use connection string explícita

```powershell
dotnet ef database update --project src/ZenithHarvest.Infrastructure `
  --connection "Data Source=localhost:1521/freepdb1;User Id=zenith_user;Password=zenith123;"
```

---

### ⚠️ NuGet Warning (Microsoft.Extensions.Caching.Memory)

**Solução (opcional):**
```powershell
dotnet add package Microsoft.Extensions.Caching.Memory --version 9.0.0 --project src/ZenithHarvest.Infrastructure
```

---

## 📂 Estrutura de Pastas

```
zenith-harvest-dotnet/
├── src/
│   ├── ZenithHarvest.Api/              # ASP.NET Core (Controllers, Program.cs)
│   ├── ZenithHarvest.Application/      # Use Cases (Handlers, DTOs)
│   ├── ZenithHarvest.Domain/           # Business Logic (Entities, Interfaces)
│   └── ZenithHarvest.Infrastructure/   # Data Access (DbContext, Migrations)
│
├── tests/
│   └── ZenithHarvest.Tests/            # Unit & Integration Tests
│
└── README.md
```

---

## 🔑 Tecnologias

| Tecnologia | Versão | Uso |
|-----------|--------|-----|
| .NET | 10.0 | Runtime |
| ASP.NET Core | 10 | Web Framework |
| Entity Framework Core | 8 | ORM |
| Oracle Database | 21c Free | Banco de Dados |
| JWT | - | Autenticação |
| xUnit | - | Testes |

---

## ✅ Checklist de Funcionalidades

- ✅ Clean Architecture implementada
- ✅ Autenticação JWT funcional
- ✅ Testes automatizados (xUnit)
- ✅ Health Checks
- ✅ OpenAPI/Swagger documentado
- ✅ Migrations do EF Core
- ✅ Docker configurado
- ✅ Exception Handling global

---

## 📋 Exemplo de Requisição

### Login

```bash
curl -X POST https://localhost:7177/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"teste@seguros.com","senha":"senha123"}'
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Usar Token

```bash
curl -X GET https://localhost:7177/api/Policies/1 \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

---

## 📄 Licença

MIT

---

## 📝 Observações

- **Connection String:** Usa Oracle Free com PDB `FREEPDB1`
- **Senha padrão:** `zenith123`
- **Usuário padrão:** `zenith_user`
- **JWT Secret:** Alterar em produção
- **Base URL:** `https://localhost:7177`

---

**Última atualização:** Junho 2026 | **.NET 10.0** | **Oracle 21c Free**
