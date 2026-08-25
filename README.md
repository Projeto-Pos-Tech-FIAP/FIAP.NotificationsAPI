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

## Migracao serverless (AWS Lambda + Kafka trigger)

Alem da API tradicional, o consumo dos topicos `user-created` e `payment-processed`
foi migrado para uma AWS Lambda (`src/FIAP.NotificationsAPI.Lambda`), acionada
diretamente pelo Kafka via *self-managed Kafka event source mapping*. O Kafka
continua sendo o mesmo (mesmos topicos, mesmo broker) — so muda quem consome.

Como o Kafka roda localmente (Docker), e a Lambda roda na AWS, a conexao entre
os dois se da por um tunel TCP (ngrok), autenticado com SASL/PLAIN e
criptografado com TLS (a AWS exige TLS para brokers self-managed acessiveis
pela internet publica).

**Cada pessoa que for rodar isso precisa gerar os proprios certificados e usar
o proprio tunel** — nao da pra reaproveitar os de outra pessoa, ja que o
endereco do tunel muda a cada sessao do ngrok.

> Se voce so vai desenvolver/testar as outras APIs (Catalog/Users/Payment) e
> nao vai mexer na Lambda: **pode pular esta secao inteira**. `cp .env.example .env`
> e `docker compose up -d` ja sobem o Kafka normalmente, usando um certificado
> de exemplo versionado em `kafka-certs-default/` — o listener `TUNNEL` fica
> la, so inerte (ninguem consegue autenticar nele), sem afetar os outros
> listeners que Payments/Users/Catalog usam.

### Pre-requisitos

- Conta AWS (com um usuario IAM proprio, nao a conta root) e AWS CLI configurado
  (`aws configure`) — **nunca** compartilhe suas chaves de acesso com outra
  pessoa nem as coloque em arquivos versionados.
- [ngrok](https://ngrok.com) instalado e com conta (o plano gratuito hoje exige
  cadastro de cartao so para verificacao, sem cobranca, para liberar tuneis TCP).
- `dotnet tool install -g Amazon.Lambda.Tools`.
- OpenSSL (ja vem instalado no macOS/Linux).

### Passo a passo

1. **Suba o Kafka** (veja secao "Como executar" acima).

2. **Abra o tunel** num terminal que vai ficar aberto durante todo o uso:
   ```bash
   ngrok tcp 9094
   ```
   Anote o endereco que aparecer em `Forwarding` (ex: `0.tcp.sa.ngrok.io:16275`).

3. **Gere os certificados** do listener `TUNNEL`, passando so o host (sem porta):
   ```bash
   ./generate-kafka-tunnel-certs.sh 0.tcp.sa.ngrok.io
   ```
   O script gera tudo em `kafka-certs/` (gitignored) e imprime os valores que
   voce precisa colar no `.env`.

4. **Crie o `.env`** a partir do `.env.example`, com o endereco completo
   (host:porta) do passo 2 e as senhas impressas no passo 3:
   ```bash
   cp .env.example .env
   ```

5. **Reinicie o Kafka** para aplicar a config nova:
   ```bash
   docker compose up -d --force-recreate kafka
   ```

6. **Deploy da Lambda**:
   ```bash
   cd src/FIAP.NotificationsAPI.Lambda

   # cria a IAM Role de execucao (uma vez so)
   aws iam create-role --role-name notifications-lambda-execution-role \
     --assume-role-policy-document '{"Version":"2012-10-17","Statement":[{"Effect":"Allow","Principal":{"Service":"lambda.amazonaws.com"},"Action":"sts:AssumeRole"}]}'
   aws iam attach-role-policy --role-name notifications-lambda-execution-role \
     --policy-arn arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole

   # ajuste aws-lambda-tools-defaults.json: profile, region e function-role
   # (o ARN da role criada acima) antes de rodar o deploy
   dotnet lambda deploy-function
   ```

7. **Guarde as credenciais do Kafka no Secrets Manager** (usadas pela Lambda
   pra autenticar no broker):
   ```bash
   aws secretsmanager create-secret --name notifications-lambda/kafka-tunnel-creds \
     --secret-string '{"username":"lambda","password":"<KAFKA_TUNNEL_SASL_PASSWORD do seu .env>"}'

   aws secretsmanager create-secret --name notifications-lambda/kafka-tunnel-ca-cert \
     --secret-string "{\"certificate\": \"$(cat kafka-certs/ca-cert.pem)\"}"

   # de permissao pra Role ler os dois segredos acima (troque os ARNs)
   aws iam put-role-policy --role-name notifications-lambda-execution-role \
     --policy-name read-kafka-tunnel-secret \
     --policy-document '{"Version":"2012-10-17","Statement":[{"Effect":"Allow","Action":"secretsmanager:GetSecretValue","Resource":["<ARN-creds>","<ARN-ca-cert>"]}]}'
   ```

8. **Crie os dois Event Source Mappings** (um por topico — a AWS so aceita um
   topico por mapping para Kafka):
   ```bash
   aws lambda create-event-source-mapping --function-name FIAP-NotificationsLambda \
     --topics user-created --starting-position TRIM_HORIZON \
     --self-managed-event-source '{"Endpoints":{"KAFKA_BOOTSTRAP_SERVERS":["<host:porta do tunel>"]}}' \
     --source-access-configurations Type=BASIC_AUTH,URI=<ARN-creds> Type=SERVER_ROOT_CA_CERTIFICATE,URI=<ARN-ca-cert>

   aws lambda create-event-source-mapping --function-name FIAP-NotificationsLambda \
     --topics payment-processed --starting-position TRIM_HORIZON \
     --self-managed-event-source '{"Endpoints":{"KAFKA_BOOTSTRAP_SERVERS":["<host:porta do tunel>"]}}' \
     --source-access-configurations Type=BASIC_AUTH,URI=<ARN-creds> Type=SERVER_ROOT_CA_CERTIFICATE,URI=<ARN-ca-cert>
   ```

9. **Teste**: publique uma mensagem no topico e acompanhe o CloudWatch Logs:
   ```bash
   docker exec -i fiap-kafka kafka-console-producer --bootstrap-server 0.0.0.0:9092 --topic user-created <<< '{"name":"Teste","email":"teste@fiap.com.br"}'
   aws logs tail /aws/lambda/FIAP-NotificationsLambda --since 2m
   ```

Se algum dia o processo do `ngrok` cair, o endereco do tunel muda — repita os
passos 2 a 8 (o Kafka e a Lambda continuam existindo, so precisa apontar os
dois pro endereco novo).

**Nao rode a `FIAP.NotificationsAPI.Api` (com os `BackgroundService`) ao mesmo
tempo que a Lambda** — os dois vao consumir os mesmos topicos em paralelo
(grupos de consumidor diferentes), duplicando o "envio" de e-mail simulado.
