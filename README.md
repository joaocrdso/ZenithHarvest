# 🌾 ZenithHarvest

API REST para gestão de sinistros agrícolas utilizando .NET, Oracle Database e autenticação JWT.

**Status:** ✅ Funcional

---

# 🚀 Quick Start

## Pré-requisitos

- .NET SDK 10
- Docker Desktop
- Git

## 1. Clonar o projeto

```bash
git clone <seu-repositorio>
cd zenith-harvest-dotnet
dotnet restore
```

## 2. Subir o Oracle

```powershell
    ddocker run -d -e ORACLE_PASSWORD=oracle123 -p 1521:1521 --name oracle-db gvenzl/oracle-free:latest
```

Aguarde até aparecer:

```text
DATABASE IS READY TO USE!
```

Verificação:

```powershell
docker logs -f oracle-db
```

## 3. Criar usuário da aplicação

```powershell
docker exec -it oracle-db sqlplus sys/oracle123@localhost:1521/freepdb1 as sysdba
```

```sql
ALTER SESSION SET CONTAINER=FREEPDB1;

CREATE USER zenith_user IDENTIFIED BY zenith123;

GRANT CONNECT, RESOURCE, UNLIMITED TABLESPACE TO zenith_user;

COMMIT;
```

## 4. Configurar Connection String

appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost:1521/freepdb1;User Id=zenith_user;Password=zenith123;"
  }
}
```

## 5. Corrigir o DesignTime DbContext Factory

O arquivo `ZenithContextFactory.cs` deve utilizar o mesmo serviço Oracle da aplicação:

```csharp
optionsBuilder.UseOracle(
    "Data Source=localhost:1521/freepdb1;User Id=zenith_user;Password=zenith123;");
```

Não utilize:

```text
XEPDB1
```

O container Oracle Free expõe o serviço:

```text
freepdb1
```

## 6. Aplicar migrations

```powershell
dotnet ef database update --project src/ZenithHarvest.Infrastructure --connection "Data Source=localhost:1521/freepdb1;User Id=zenith_user;Password=zenith123;"
```

## 7. Conectar ao banco com o usuário correto

```powershell
docker exec -it oracle-db sqlplus zenith_user/zenith123@freepdb1
```


Verificar tabelas:

```sql
SELECT table_name FROM user_tables;
```

Esperado:

```text
__EFMigrationsHistory
Insurers
Policies
Users
Claims
```

## 7. Criar dados de teste

### Seguradora

```sql
INSERT INTO "Insurers"
("Id","CNPJ","Nome","CodigoSUSEP","DataCriacao")
VALUES
(1,'12.345.678/0001-99','Zenith Seguros','123456',SYSTIMESTAMP);

COMMIT;
```

### Usuário

Senha utilizada:

```text
123456
```

Hash correspondente:

```text
jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=
```

```sql
INSERT INTO "Users"
("Id","InsurerId","Email","PasswordHash","Role","Ativo","DataCriacao")
VALUES
(1,1,'admin@zenith.com',
'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=',
'Admin',1,SYSTIMESTAMP);

COMMIT;
```

## 8. Executar a API

```powershell
dotnet run --project src/ZenithHarvest.Api --launch-profile https
```

Documentação:

```text
https://localhost:7177/scalar/v1
```

---

# 🔐 Login de Teste

```json
{
  "email": "admin@zenith.com",
  "senha": "123456"
}
```

---

# 📚 Endpoints

## Auth

- POST `/api/Auth/login`

## Policies

- GET `/api/Policies/{insurerId}`
- POST `/api/Policies/claims`

## Health

- GET `/health`

---

# 🛠 Troubleshooting

## ORA-12514

Verifique:

- Serviço `freepdb1`
- Connection String
- `ZenithContextFactory`
- Oracle totalmente inicializado

Teste:

```bash
sqlplus zenith_user/zenith123@freepdb1
```

Se conectar, o banco está funcionando.

## ORA-02291

Erro de chave estrangeira.

Crie primeiro um registro em:

```text
Insurers
```

antes de inserir usuários.

---

# 🏗 Arquitetura

- API
- Application
- Domain
- Infrastructure
- Oracle Database

Padrões utilizados:

- Clean Architecture
- Repository Pattern
- Dependency Injection
- CQRS

---

# 🔑 Tecnologias

- .NET 10
- ASP.NET Core
- Entity Framework Core
- Oracle Database Free (26ai)
- JWT
- xUnit

---

# 📄 Licença

MIT
