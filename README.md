# 💳 FCG Payments API – Fase 2

API REST desenvolvida em **.NET 8** como parte do **Desafio da Fase 2** da disciplina **Arquitetura de Sistemas .NET – FIAP**.

O projeto representa o microsserviço de **Pagamentos** da plataforma de games educacionais (**FCG – FIAP Game Center**). Responsável por processar (simular) pagamentos de compras de jogos de forma assíncrona via mensageria.

---

## 📌 Objetivo do Projeto

Processar pagamentos de forma orientada a eventos, garantindo:
- Consumo do evento **OrderPlacedEvent** via RabbitMQ
- Processamento (simulado) do pagamento
- Publicação do **PaymentProcessedEvent** com status **Approved** ou **Rejected**
- Containerização com **Docker**
- Base escalável para orquestração em Kubernetes

---

## 🛠️ Tecnologias Utilizadas

- **.NET 8**
- **ASP.NET Core Web API**
- **MassTransit** (Abstração de Mensageria)
- **RabbitMQ** (Message Broker)
- **Docker**
- **Swagger / OpenAPI**
- **ILogger** para logs estruturados

---

## 🧱 Arquitetura

O projeto segue uma separação clara de responsabilidades, inspirada em princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**.

### Camadas Principais

- **Api** — Controllers, middlewares, configuração da aplicação
- **Application** — Serviços de negócio, interfaces, DTOs e consumidores/produtores de eventos
- **Domain** — Entidades (ex: Payment), Value Objects e exceções de domínio
- **Infrastructure** — Repositórios, acesso a dados e configuração do RabbitMQ

---

## 📁 Estrutura de Pastas
```text
Fgc.Payments
│
├── Fgc.Payments.Api
│   ├── Controllers
│   ├── Consumers
│   ├── Program.cs
│   └── appsettings.json
│
├── Fgc.Payments.Application
│   ├── Services
│   ├── Interfaces
│   ├── Consumers
│   └── DTOs
│
├── Fgc.Payments.Domain
│   ├── Entities
│   ├── Enums
│   └── Exceptions
│
├── Fgc.Payments.Infraestructure
│   ├── Configuration
│   ├── Migrations
│   ├── Persistence
│   └── Repositories
│
└── tests
    ├── Fgc.Payments.UnitTests
    └── Fgc.Payments.IntegrationTests
```

---

## 🌐 Endpoints HTTP

Todos exigem JWT válido (`[Authorize]`), emitido pelo `fgc-users-api`:

* `POST /api/payments` — processa um novo pagamento (`PaymentRequest` no corpo)
* `GET /api/payments/{id}` — consulta um pagamento por ID

---

## 📨 Mensageria e Eventos (RabbitMQ)

* Consome **OrderPlacedEvent** na fila `payments-order-placed-queue`
* Publica **PaymentProcessedEvent** (status `Approved`/`Rejected`) após processar o pagamento
* Fila e host configurados via `RabbitMq:Host` (`appsettings.json` / env var `RabbitMq__Host`); usuário/senha do broker atualmente fixos em `admin`/`admin` no código (`Program.cs`)

---

## ▶️ Como Executar o Projeto

### Pré-requisitos

* .NET SDK 8 ou superior
* Docker e Docker Compose (para o RabbitMQ)

### Execução via Docker

Este serviço não sobe o próprio RabbitMQ — ele se conecta à mesma instância que o `fgc-users-api` sobe. Suba o broker primeiro:

```bash
docker-compose up -d   # a partir de fgc-users-api/Fgc.Users
```

Depois suba a API (o `docker-compose.yml` deste serviço entra na rede externa `fgcusers_fgc_network`):

```bash
docker-compose up -d --build
```

A API fica exposta em `http://localhost:8081` (mapeada pra porta interna 8080, evitando conflito com `fgc-notifications-api` na 8080).

O Dockerfile espera uma pasta `LocalPackages/` e um `nuget.config` na raiz do contexto de build (fornecem o pacote `Fgc.MessageContracts`), então o build deve ser feito com a raiz deste repositório como contexto:

```bash
docker build -f Dockerfile -t fgc-payments-api .
```

### Execução Local

```bash
dotnet restore
dotnet run --project Fgc.Payments/src/Fgc.Payments.Api
```

---

## 👥 Squad 8 – Turma 12NETT

**Integrantes**

* Yan Santos Wendt
* Ronnam de Lima da Silva