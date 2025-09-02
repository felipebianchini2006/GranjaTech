# GranjaTech - Gestão de Granjas Avícolas

Sistema web para gestão de granjas de corte, com backend em .NET 8 e frontend em React.

## 🚀 Execução Local
**Pré-requisitos:** .NET 8 SDK, Node.js 18+, PostgreSQL 12+

**Backend**
```bash
cd GranjaTech.Api
dotnet run
# API em: https://localhost:7135
```

**Frontend**
```bash
cd frontend
npm install
npm start
# Frontend em: http://localhost:3000
```

## 📊 Funcionalidades

**Backend (.NET)**
- Autenticação JWT + perfis
- CRUD de Granjas, Lotes, Estoque, Financeiro
- Sensores e leituras
- Relatórios e dashboards
- Auditoria de ações

**Frontend (React)**
- Páginas: Login, Dashboard, Granjas, Lotes, Estoque, Financeiro, Sensores, Relatórios, Usuários, Auditoria
- Exportação de relatórios (Excel/PDF)
- UI responsiva (MUI) e gráficos interativos

## 🔧 Melhorias Recentes
- Middleware global de exceções
- Limite de registros e validação de intervalo em relatórios
- Logs detalhados e endpoint de debug
- Health check (`/api/relatorios/health`)

## 🔄 Próximos Passos
- Índices e paginação
- Cache e processamento assíncrono
- Variáveis de ambiente para secrets
- Rate limiting
- Relatórios PDF no servidor
- Integração IoT
- CI/CD, Docker e monitoramento

## 📁 Estrutura
```
GranjaTech/
├── GranjaTech.Api/          # API
├── GranjaTech.Application/  # DTOs e serviços
├── GranjaTech.Domain/       # Entidades
├── GranjaTech.Infrastructure/ # DbContext, repositórios
└── frontend/                # React SPA
```

**Status:** ✅ Estável e funcional

---

### 📘 Projeto Acadêmico
Projeto desenvolvido para o **Projeto Integrador FATEC**

**Integrantes:**
- Wendell Nascimento
- Felipe Bianchini
- Guilherme Oliveira
- Adryan Thiago
