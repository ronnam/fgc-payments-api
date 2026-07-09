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
Fgc.

Payments
│
├── Fgc.

Payments.

Api
│   ├── Controllers
│   ├── Program.cs
│   └── appsettings.json
│
├── Fgc.

Payments.

Application
│   ├── Services
│   ├── Interfaces
│   ├── Consumers
│   └── DTOs
│
├── Fgc.

Payments.

Domain
│   ├── Entities
│   └── Enums
│
├── Fgc.

Payments.

Infrastructure
│   └── Messaging
│
└── tests
    ├── Fgc.

Payments.

UnitTests
    └── Fgc.

Payments.

IntegrationTests