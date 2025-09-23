# GranjaTech - Gestão de Granjas Avícolas

Sistema web moderno para gestão de granjas de corte com **React + .NET**.

## 🚀 Quick Start

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
