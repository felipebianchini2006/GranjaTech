# GranjaTech - Gestão de Granjas Avícolas

Sistema web moderno para gestão de granjas de corte com **React + .NET**.

## 🚀 Quick Start

### 🐳 Docker (Recomendado)
```bash
docker-compose up -d  # Inicia tudo (Frontend + Backend + PostgreSQL)

# Acesse:
# Frontend:  http://localhost:3000
# Swagger:   http://localhost:5099/swagger
# IoT MQTT:  http://localhost:5099/api/iot/status
```

### Simulador IoT via Docker

O `docker-compose.yml` tambem sobe um broker MQTT (`mqtt-broker`) e um container de firmware (`iot-simulator`) que publica temperatura, umidade e luminosidade. A API consome essas mensagens em background e grava as leituras nos sensores do sistema.

Validacao rapida:

```bash
docker compose up --build
docker compose logs -f iot-simulator
```

Simulacao manual durante a apresentacao:

No Windows, basta abrir com duplo clique:

```text
abrir-simulador-iot.cmd
```

Ou pelo terminal:

```bash
docker compose stop iot-simulator
docker compose run --rm iot-manual-simulator
```

Detalhes do payload, topico MQTT e roteiro de apresentacao: [`docs/iot-simulator.md`](docs/iot-simulator.md).

### 💻 Desenvolvimento Local
**Pré-requisitos:** .NET 8 SDK, Node.js 18+, PostgreSQL 12+

```bash
# Backend
cd GranjaTech.Api && dotnet run  # https://localhost:7135

# Frontend  
cd frontend && npm install && npm start  # http://localhost:3000
```

## ⚡ Funcionalidades

• **Autenticação** JWT com perfis (Admin/Produtor/Financeiro)  
• **Gestão** completa de granjas, lotes, usuários e estoque  
• **Sensores** IoT com leituras em tempo real  
• **Relatórios** exportáveis (Excel/PDF) e dashboards  
• **Auditoria** completa de ações do sistema  

## 🛠️ Stack

**Backend:** .NET 8, PostgreSQL, Entity Framework  
**Frontend:** React 19, Material-UI, Recharts  
**DevOps:** Docker, Docker Compose
## 📁 Estrutura

```
GranjaTech/
├── GranjaTech.Api/            # API REST .NET
├── GranjaTech.Application/    # DTOs e serviços
├── GranjaTech.Domain/         # Entidades de domínio
├── GranjaTech.Infrastructure/ # DbContext e repositórios
└── frontend/                  # React SPA
```

## 👥 Projeto Acadêmico FATEC

**Equipe:**
- **Felipe Bianchini** – Backend (.NET, PostgreSQL, API)
- **Wendell Nascimento** – Frontend (React, MUI, testes)
- **Guilherme Oliveira** – Banco de dados, auditoria, endpoints
- **Adryan Thiago** – Relatórios, dashboards, sensores
