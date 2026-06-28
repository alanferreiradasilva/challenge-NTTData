# Challenge NTT Data — Order Management API

Backend de um sistema de gestão de pedidos para e-commerce, construído com **.NET 10**, **Clean Architecture**, **CQRS com MediatR** e **Docker**.

---

## Stack

| Camada     | Tecnologias                                                                 |
| ---------- | --------------------------------------------------------------------------- |
| API        | ASP.NET Core Controllers, JWT Bearer, Scalar UI (OpenAPI), Serilog           |
| Application| MediatR (CQRS), FluentValidation, Pipeline Behaviors (Validation + Logging) |
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

### Logs com Serilog

Acompanhe os logs da API em tempo real:

```bash
docker compose logs api -f
```

Cada request/response é logado automaticamente com tempo de execução:

```
[23:14:24 INF] Handling CancelOrderCommand { OrderId: "2b9b2a04-..." }
[23:14:25 INF] Handled CancelOrderCommand in 803ms
```

Para sair do modo follow, pressione `Ctrl+C`.

---

## Endpoints

| Método | Rota                      | Auth | Descrição                        |
| ------ | ------------------------- | ---- | -------------------------------- |
| POST   | `/auth/login`             | ❌   | Retorna JWT token                |
| POST   | `/api/orders`             | ✅   | Cria novo pedido                 |
| GET    | `/api/orders`             | ✅   | Lista pedidos (paginado)         |
| GET    | `/api/orders/{id}`        | ✅   | Busca pedido por ID              |
| PATCH  | `/api/orders/{id}/cancel` | ✅   | Cancela pedido (se `Pending`)    |

## Consumo das APIs

### Autenticação

**POST** `/auth/login`

Request:
```json
{
  "email": "dev@martech.com",
  "password": "Senha@123"
}
```

Response `200 OK`:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2026-06-28T14:00:00Z"
}
```

Response `401 Unauthorized`:
*(sem body)*

---

### Pedidos

> Todos os endpoints de pedidos exigem o header `Authorization: Bearer {token}`.

#### Criar Pedido

**POST** `/api/orders`

Request:
```json
{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "items": [
    {
      "productName": "Notebook",
      "quantity": 2,
      "unitPrice": 25.50
    }
  ]
}
```

Response `201 Created`:
```json
{
  "id": "a1b2c3d4-...-..."
}
```

Response `400 Bad Request` (exemplo — items vazio):
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "Items": ["Order must have at least one item."]
  }
}
```

#### Listar Pedidos

**GET** `/api/orders?page=1&pageSize=10`

Response `200 OK`:
```json
{
  "items": [
    {
      "id": "a1b2c3d4-...-...",
      "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "status": "Pending",
      "createdAt": "2026-06-28T12:00:00Z",
      "totalAmount": 51.00,
      "items": [
        {
          "id": "b2c3d4e5-...-...",
          "productName": "Notebook",
          "quantity": 2,
          "unitPrice": 25.50
        }
      ]
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

#### Obter Pedido por ID

**GET** `/api/orders/{id}`

Response `200 OK`:
```json
{
  "id": "a1b2c3d4-...-...",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Pending",
  "createdAt": "2026-06-28T12:00:00Z",
  "totalAmount": 51.00,
  "items": [
    {
      "id": "b2c3d4e5-...-...",
      "productName": "Notebook",
      "quantity": 2,
      "unitPrice": 25.50
    }
  ]
}
```

Response `404 Not Found`:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Order with id a1b2c3d4-...-... not found."
}
```

#### Cancelar Pedido

**PATCH** `/api/orders/{id}/cancel`

Response `204 No Content`:
*(sem body)*

Response `409 Conflict` (pedido já não está `Pending`):
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Only orders with Pending status can be cancelled."
}
```

---

### Exemplos com cURL

```bash
# 1. Login
TOKEN=$(curl -s -X POST http://localhost:5217/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"dev@martech.com","password":"Senha@123"}' \
  | jq -r '.token')

# 2. Criar pedido
curl -s -X POST http://localhost:5217/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "items": [{"productName": "Notebook", "quantity": 2, "unitPrice": 25.50}]
  }'

# 3. Listar pedidos
curl -s "http://localhost:5217/api/orders?page=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"

# 4. Buscar pedido por ID (substitua {id} pelo retornado no passo 2)
curl -s http://localhost:5217/api/orders/{id} \
  -H "Authorization: Bearer $TOKEN"

# 5. Cancelar pedido
curl -s -X PATCH http://localhost:5217/api/orders/{id}/cancel \
  -H "Authorization: Bearer $TOKEN"
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

## Análise com SonarQube

O SonarQube está configurado no `docker-compose.yml` (container `sonarqube` com banco H2 embutido, porta `9000`).

### Iniciar o SonarQube

```bash
docker compose up -d sonarqube
```

Aguardar até o log mostrar `SonarQube is operational`:

```bash
docker compose logs sonarqube --tail 5
```

Acessar http://localhost:9000, logar com `admin` / `admin` e trocar a senha.

### Gerar token de análise

1. Avatar (canto superior direito) → **My Account** → **Security**
2. Em **Generate Tokens**, digite um nome (ex: `challenge-token`) e clique **Generate**
3. Copie o token gerado

### Executar análise

```bash
# Instalar o scanner (uma vez)
dotnet tool install --global dotnet-sonarscanner

# Iniciar análise
dotnet sonarscanner begin /k:"challenge-nttdata" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="SEU_TOKEN"

# Compilar
dotnet build src/Challenge.API/Challenge.API.csproj

# Finalizar e enviar resultados
dotnet sonarscanner end /d:sonar.login="SEU_TOKEN"
```

Resultados em http://localhost:9000/dashboard?id=challenge-nttdata.

---

## Decisões Técnicas

- **Records para DTOs/Commands/Queries**: Imutabilidade, `with` expressions, value equality para asserts em testes.
- **Serilog + LoggingBehavior (pipeline)**: Logging estruturado de todos os requests/responses com tempo de execução via pipeline do MediatR. Console sink configurado com nível `Information` por padrão, `Warning` para Microsoft.AspNetCore.
- **Global Exception Handler Middleware**: Tratamento centralizado de `ValidationException` (400), `KeyNotFoundException` (404), `InvalidOperationException` (409), demais (500).
- **EnsureCreated()**: Criação automática do banco na inicialização — simples e direto para desenvolvimento.
- **Scalar UI**: Interface moderna e recomendada pela Microsoft para .NET 10, substituindo SwaggerUI.
