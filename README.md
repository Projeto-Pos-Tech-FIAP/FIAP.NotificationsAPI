# FIAP.NotificationsAPI

Microservico responsavel por simular o envio de notificacoes por e-mail da plataforma FIAP Cloud Games.

A API disponibiliza endpoints HTTP para disparo manual de notificacoes e tambem possui consumidores Kafka para processar eventos de criacao de usuario e pagamento processado.

## Funcionalidades

- Envio simulado de e-mail de boas-vindas para usuarios cadastrados.
- Envio simulado de e-mail de pagamento aprovado.
- Consumo de eventos Kafka dos topicos `user-created` e `payment-processed`.
- Exposicao de documentacao Swagger em ambiente de desenvolvimento.
- Health check em `/health`.
- Testes unitarios para o servico de notificacao.

## Tecnologias

- .NET 8
- ASP.NET Core Web API
- Swashbuckle / Swagger
- Apache Kafka
- Confluent.Kafka
- xUnit
- Docker Compose

## Estrutura do projeto

```text
FIAP.NotificationsAPI
|-- docker-compose.yml
|-- FIAP.NotificationsAPI.slnx
|-- README.md
`-- src
    |-- FIAP.NotificationsAPI.Api
    |-- FIAP.NotificationsAPI.Application
    |-- FIAP.NotificationsAPI.Domain
    |-- FIAP.NotificationsAPI.Infrastructure
    `-- FIAP.NotificationsAPI.Tests
```

## Requisitos

- .NET SDK 8.0 ou superior.
- Docker Desktop, para executar Kafka localmente.
- Porta `5221` livre para execucao HTTP da API.
- Porta `7109` livre para execucao HTTPS da API.
- Porta `9092` livre para Kafka.
- Porta `8085` livre para Kafka UI.

## Configuracao

As configuracoes principais ficam em `src/FIAP.NotificationsAPI.Api/appsettings.json`.

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "notifications-api-group",
    "UserCreatedTopic": "user-created",
    "PaymentProcessedTopic": "payment-processed"
  }
}
```

## Como executar

Suba o Kafka e o Kafka UI:

```bash
docker compose up -d
```

Rode a API:

```bash
dotnet run --project src/FIAP.NotificationsAPI.Api/FIAP.NotificationsAPI.Api.csproj
```

Acesse:

- API HTTP: `http://localhost:5221`
- API HTTPS: `https://localhost:7109`
- Swagger: `http://localhost:5221/swagger`
- Health check: `http://localhost:5221/health`
- Kafka UI: `http://localhost:8085`

## Compilacao

```bash
dotnet build FIAP.NotificationsAPI.slnx
```

Se a compilacao falhar informando que arquivos `.dll` ou `.exe` estao em uso, encerre a instancia da API que estiver rodando e execute o build novamente.

## Testes

```bash
dotnet test FIAP.NotificationsAPI.slnx
```

## Endpoints HTTP

### Enviar e-mail de boas-vindas

`POST /api/notifications/welcome`

Request:

```json
{
  "email": "player@fiap.com.br",
  "name": "Player One"
}
```

Response:

```json
{
  "success": true,
  "status": "Sent",
  "message": "Welcome email sent successfully."
}
```

### Enviar e-mail de pagamento processado

`POST /api/notifications/payment-processed`

Request:

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "gameId": 1,
  "correlationId": "a446a667-2d23-4c22-9ea1-414deb0ea1f3",
  "status": 1
}
```

Response:

```json
{
  "success": true,
  "status": "Sent",
  "message": "Payment processed email sent successfully."
}
```

## Eventos Kafka

### Topico `user-created`

Quando uma mensagem valida e recebida neste topico, a API simula o envio de um e-mail de boas-vindas.

Payload esperado:

```json
{
  "name": "Player One",
  "email": "player@fiap.com.br"
}
```

### Topico `payment-processed`

Quando uma mensagem valida e recebida neste topico, a API simula o envio de um e-mail de pagamento processado apenas se `status` for `Approved`. Mesmo formato de evento publicado pelo `PaymentsAPI` e consumido pelo `CatalogAPI` — um unico contrato compartilhado pelos tres servicos.

Payload esperado:

```json
{
  "correlationId": "a446a667-2d23-4c22-9ea1-414deb0ea1f3",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "gameId": 1,
  "status": "Approved",
  "reason": null
}
```

## Observacoes tecnicas

- O envio de e-mail e apenas uma simulacao feita via `Console.WriteLine`.
- O Swagger e habilitado somente em ambiente `Development`.
- O consumidor de pagamento ignora notificacoes quando o status do pagamento nao e `Approved`.
- Os consumidores Kafka estao implementados no projeto `FIAP.NotificationsAPI.Infrastructure` e registrados no `Program.cs` via `AddInfrastructure(builder.Configuration)`.

## Contratos principais

### `SendWelcomeEmailRequest`

| Campo | Tipo | Obrigatorio | Validacao |
| --- | --- | --- | --- |
| `email` | string | Sim | Formato de e-mail |
| `name` | string | Sim | Nao vazio |

### `SendPaymentProcessedEmailRequest`

| Campo | Tipo | Obrigatorio | Validacao |
| --- | --- | --- | --- |
| `userId` | guid | Sim | - |
| `gameId` | integer | Sim | - |
| `correlationId` | string | Nao | - |
| `status` | integer | Nao | `1` = Approved, `2` = Rejected |

## Comandos uteis

```bash
docker compose up -d
dotnet restore FIAP.NotificationsAPI.slnx
dotnet build FIAP.NotificationsAPI.slnx
dotnet test FIAP.NotificationsAPI.slnx
dotnet run --project src/FIAP.NotificationsAPI.Api/FIAP.NotificationsAPI.Api.csproj
```
