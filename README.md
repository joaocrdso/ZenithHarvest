# 🌾 ZenithHarvest

API REST para gestão de sinistros em seguros agrícolas usando índices de vegetação (NDVI).

---

## 🚀 Quick Start (5 minutos)

### 1️⃣ Clonar e restaurar

```bash
git clone seu-repo-aqui
cd zenith-harvest-dotnet
dotnet restore
```

### 2️⃣ Oracle em Docker

```powershell
docker run -d -e ORACLE_PASSWORD=oracle123 -p 1521:1521 --name oracle-db gvenzl/oracle-free:latest
```

**Aguarde 3-5 minutos** até ver nos logs:
```powershell
docker logs -f oracle-db
```

Procure por: `DATABASE IS READY TO USE!` → Pressione **CTRL+C**

### 3️⃣ Criar usuário no Oracle

```powershell
docker exec -it oracle-db sqlplus sys/oracle123@localhost:1521/xe as sysdba
```

Cole tudo isto:
```sql
ALTER SESSION SET CONTAINER=FREEPDB1;
CREATE USER zenith_user IDENTIFIED BY zenith123;
GRANT CONNECT, RESOURCE TO zenith_user;
GRANT UNLIMITED TABLESPACE TO zenith_user;
EXIT;
```

### 4️⃣ Configurar banco de dados

Edite `src/ZenithHarvest.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost:1521/xe;User Id=zenith_user;Password=zenith123;"
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

### 5️⃣ Criar tabelas

```powershell
dotnet ef database update --project src/ZenithHarvest.Infrastructure
```

### 6️⃣ Rodar a aplicação

```powershell
dotnet run --project src/ZenithHarvest.Api --launch-profile https
```

Abra no navegador:
```
https://localhost:7177/scalar/v1
```

✅ **Pronto!**

---

## 📋 Pré-requisitos

- **.NET SDK 10.0** ([Baixar](https://dotnet.microsoft.com/download))
- **Docker** ([Baixar](https://www.docker.com/products/docker-desktop))
- **Git**

---

## 🏗️ Arquitetura

```
API Layer (Controllers, JWT)
    ↓
Application Layer (Handlers, DTOs)
    ↓
Domain Layer (Entities, Interfaces)
    ↓
Infrastructure Layer (DbContext, Repositories)
    ↓
Oracle Database
```

---

## 🔐 Autenticação

Todos os endpoints (exceto `/health`) usam JWT.

### Login
```bash
curl -X POST http://localhost:5032/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"teste@seguros.com","senha":"senha123"}'
```

Resposta:
```json
{"token":"eyJhbGc..."}
```

### Usar token
```bash
curl -X GET http://localhost:5032/api/policies/1 \
  -H "Authorization: Bearer SEU_TOKEN"
```

---

## 📚 Endpoints Principais

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/auth/login` | Fazer login |
| GET | `/api/policies/{insurerId}` | Listar apólices |
| POST | `/api/policies/claims` | Criar sinistro |
| GET | `/health` | Health check |

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

**Causa:** Oracle ainda inicializando

**Solução:** Aguarde mais e verifique logs:
```powershell
docker logs oracle-db | Select-String "DATABASE IS READY"
```

---

### ❌ ORA-01017: Usuário/senha inválidos

**Causa:** Usuário não foi criado

**Solução:** Conecte como SYS e crie novamente:
```powershell
docker exec -it oracle-db sqlplus sys/oracle123@localhost:1521/xe as sysdba
```

```sql
ALTER SESSION SET CONTAINER=FREEPDB1;
DROP USER zenith_user CASCADE;
CREATE USER zenith_user IDENTIFIED BY zenith123;
GRANT CONNECT, RESOURCE TO zenith_user;
GRANT UNLIMITED TABLESPACE TO zenith_user;
EXIT;
```

---

### ❌ NuGet Warning sobre Microsoft.Extensions.Caching.Memory

**Solução:**
```powershell
dotnet add package Microsoft.Extensions.Caching.Memory --version 9.0.0 --project src/ZenithHarvest.Infrastructure
```

---

### ❌ Erro na migration: "Required user does not exists"

**Solução:** Execute com connection string explícita:
```powershell
dotnet ef database update --project src/ZenithHarvest.Infrastructure `
  --connection "Data Source=localhost:1521/xe;User Id=zenith_user;Password=zenith123;"
```

---

## 📂 Estrutura de Pastas

```
zenith-harvest-dotnet/
├── src/
│   ├── ZenithHarvest.Api/              # Controllers, Program.cs
│   ├── ZenithHarvest.Application/      # Handlers, DTOs, Security
│   ├── ZenithHarvest.Domain/           # Entities, Interfaces
│   └── ZenithHarvest.Infrastructure/   # DbContext, Repositories, Migrations
│
├── tests/
│   └── ZenithHarvest.Tests/            # xUnit tests
│
└── README.md
```

---

## 🔑 Tecnologias

- .NET 10.0
- ASP.NET Core 10
- Entity Framework Core 8
- Oracle Database
- JWT (JSON Web Tokens)
- xUnit (testes)
- Clean Architecture

---

## 📖 Clean Architecture

- **Domain**: Regras de negócio (Entities, Interfaces)
- **Application**: Casos de uso (Handlers, DTOs, Security)
- **Infrastructure**: Acesso a dados (DbContext, Repositories)
- **Api**: Endpoints HTTP (Controllers, Middleware)

---

## 📄 Licença

MIT

---

## 📞 Suporte

Encontrou problema? Abra uma issue no repositório.

---

**Última atualização**: Junho 2026 | **.NET 10.0** | **Oracle 21c Free**