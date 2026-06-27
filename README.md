# Challenge NTT Data — Order Management API

Backend de um sistema de gestão de pedidos para e-commerce, construído com **.NET 10**, **Clean Architecture**, **CQRS com MediatR** e **Docker**.

---

## Stack

| Camada     | Tecnologias                                                                 |
| ---------- | --------------------------------------------------------------------------- |
| API        | ASP.NET Core Controllers, JWT Bearer, Scalar UI (OpenAPI)                   |
| Application| MediatR (CQRS), FluentValidation, Pipeline Behaviors                        |
| Domain     | Entidades ricas com regras de negócio (TotalAmount, Cancelamento)           |
| Infrastructure | Entity Framework Core + SQLite, Repositórios, AuthService (JWT)         |
| Tests      | xUnit, FluentAssertions, Moq, Microsoft.AspNetCore.Mvc.Testing              |
| Infra      | Docker (multi-stage), docker-compose com volume SQLite                      |

---

## Arquitetura

### Por que Controllers (e não Minimal APIs)?

| Critério          | Controllers | Minimal APIs |
| ----------------- | ----------- | ------------ |
| Organização por recurso | `[Route("api/orders")]` agrupa tudo | Rotas espalhadas em `Program.cs` ou extensões |
| Atributos nativos | `[Authorize]`, `[FromBody]`, `[FromRoute]` inline | Requer `[AsParameters]` ou filtros manuais |
| Escalabilidade    | Separação natural em arquivos conforme o domínio cresce | Tendem a virar arquivos gigantes |
| Ação thin         | Delega ao MediatR — controller tem 1-3 linhas por ação | Mesmo padrão, mas sem a organização de grupo |

Para um sistema de e-commerce que cresce em recursos (Orders, Products, Customers, Payments), Controllers oferecem melhor organização e são mais familiares em projetos enterprise.

### Clean Architecture

```
src/
├── Challenge.Domain          → Entidades, Enums, Interfaces de repositório
├── Challenge.Application     → Commands, Queries, Handlers, Validators, DTOs
├── Challenge.Infrastructure  → EF Core, Repositórios, AuthService (JWT)
├── Challenge.API             → Controllers, Middleware, Extensions, Program.cs
tests/
└── Challenge.Tests
    ├── Unit/                 → Testes unitários dos handlers (Moq)
    └── Integration/          → Testes de integração (WebApplicationFactory)
```

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (opcional, para execução via Docker)

---

## Como Rodar Localmente

```bash
# Restaurar dependências
dotnet restore

# Executar a API (http://localhost:5217)
dotnet run --project src/Challenge.API --launch-profile http
```

A API inicia em `http://localhost:5217`. Em ambiente **Development**:
- Scalar UI: http://localhost:5217/scalar/v1
- OpenAPI JSON: http://localhost:5217/openapi/v1.json

### Credenciais de Login

| Email              | Senha     |
| ------------------ | --------- |
| dev@martech.com    | Senha@123 |

---

## Como Rodar com Docker

```bash
# Construir e iniciar o container
docker compose up --build

# A API estará disponível em http://localhost:5217
```

O banco SQLite é persistido em um volume Docker (`challenge_data`).

---

## Endpoints

| Método | Rota                      | Auth | Descrição                        |
| ------ | ------------------------- | ---- | -------------------------------- |
| POST   | `/auth/login`             | ❌   | Retorna JWT token                |
| POST   | `/api/orders`             | ✅   | Cria novo pedido                 |
| GET    | `/api/orders`             | ✅   | Lista pedidos (paginado)         |
| GET    | `/api/orders/{id}`        | ✅   | Busca pedido por ID              |
| PATCH  | `/api/orders/{id}/cancel` | ✅   | Cancela pedido (se `Pending`)    |

### Exemplos

```bash
# Login
curl -s -X POST http://localhost:5217/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"dev@martech.com","password":"Senha@123"}'

# Criar pedido (substitua {token} pelo JWT recebido)
curl -s -X POST http://localhost:5217/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{"customerId":"3fa85f64-5717-4562-b3fc-2c963f66afa6","items":[{"productName":"Notebook","quantity":2,"unitPrice":25.50}]}'
```

---

## Testes

```bash
# Todos os testes (unitários + integração)
dotnet test tests/Challenge.Tests
```

- **11 testes unitários**: handlers (Login, CreateOrder, CancelOrder, GetOrders, GetOrderById)
- **11 testes de integração**: fluxos completos com WebApplicationFactory + SQLite in-memory

---

## Decisões Técnicas

- **Records para DTOs/Commands/Queries**: Imutabilidade, `with` expressions, value equality para asserts em testes.
- **ValidationBehavior (pipeline)**: Validação automática de todos os commands/queries antes do handler — sem try-catch espalhados.
- **Global Exception Handler Middleware**: Tratamento centralizado de `ValidationException` (400), `KeyNotFoundException` (404), `InvalidOperationException` (409), demais (500).
- **EnsureCreated()**: Criação automática do banco na inicialização — simples e direto para desenvolvimento.
- **Scalar UI**: Interface moderna e recomendada pela Microsoft para .NET 10, substituindo SwaggerUI.
