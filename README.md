# GranjaTech - Gestão de Granjas Avícolas

Sistema web para gestão de granjas de corte.\
**Backend:** .NET 8 + PostgreSQL\
**Frontend:** React + MUI

## 🚀 Execução Local

Pré-requisitos: .NET 8 SDK, Node.js 18+, PostgreSQL 12+

``` bash
# Backend
cd GranjaTech.Api
dotnet run  # https://localhost:7135

# Frontend
cd frontend
npm install
npm start   # http://localhost:3000
```

## 📊 Funcionalidades

-   Autenticação JWT e perfis (Admin, Produtor, Financeiro)\
-   CRUD de Granjas, Lotes, Usuários, Estoque\
-   Sensores e leituras em tempo real\
-   Consumo (ração/água), Pesagem, Sanitário\
-   Relatórios e dashboards exportáveis (Excel/PDF)\
-   Auditoria de ações (Logs)

## 🔧 Melhorias Recentes

-   Validação em relatórios (intervalo e limite)\
-   Logs detalhados e endpoint de debug
-   Registro mortalidade
-   Endpoint arrumado para registro de aves em lotes

## 📁 Estrutura

    GranjaTech/
    ├── GranjaTech.Api/            # API .NET
    ├── GranjaTech.Application/    # DTOs e serviços
    ├── GranjaTech.Domain/         # Entidades
    ├── GranjaTech.Infrastructure/ # DbContext, repositórios
    └── frontend/                  # React SPA

**Status:** ✅ Estável e em evolução

------------------------------------------------------------------------

### 📘 Projeto Acadêmico

Desenvolvido para o **Projeto Integrador FATEC**

**Integrantes:**\
- Wendell Nascimento\
- Felipe Bianchini\
- Guilherme Oliveira\
- Adryan Thiago
