# GranjaTech - Sistema de Gestão de Granjas Avícolas

Sistema completo para gestão de granjas de corte com backend .NET Core e frontend React.

## 🚀 Execução Local

### Pré-requisitos
- .NET 8 SDK
- Node.js 18+
- PostgreSQL 12+

### Backend
```bash
# Configurar string de conexão em GranjaTech.Api/appsettings.json
# Por padrão: Host=localhost;Port=5432;Database=GranjaTechDb;Username=postgres;Password=123456

# Executar API
cd GranjaTech.Api
dotnet run

# API estará disponível em: https://localhost:7135
```

### Frontend
```bash
cd frontend
npm install
npm start

# Frontend estará disponível em: http://localhost:3000
```

## 📊 Funcionalidades Implementadas

### Backend (C# .NET Core)
- ✅ **Autenticação JWT** com controle de acesso por perfil (Administrador, Produtor, Financeiro)
- ✅ **Gestão de Granjas** - CRUD completo
- ✅ **Gestão de Lotes** - Controle de aves por lote
- ✅ **Controle Financeiro** - Transações de entrada e saída
- ✅ **Estoque** - Gestão de produtos, vacinas e medicamentos
- ✅ **Sensores** - Cadastro e leituras ambientais
- ✅ **Relatórios** - Financeiro e de produção com filtros
- ✅ **Dashboard** - KPIs e resumos mensais
- ✅ **Auditoria** - Log de ações dos usuários

### Frontend (React)
- ✅ **Páginas principais**: Login, Dashboard, Granjas, Lotes, Estoque, Financeiro, Sensores, Relatórios, Usuários, Perfil, Auditoria
- ✅ **Exportação de relatórios** para Excel e PDF (client-side via jsPDF)
- ✅ **Interface responsiva** com Material-UI
- ✅ **Gráficos interativos** para dashboard

## 🔧 Melhorias Implementadas Recentemente

### Correção do Crash de Relatórios
**Problema**: Backend terminava abruptamente após gerar relatórios PDF.

**Causa Identificada**: Não era o PDF (gerado no frontend), mas sim:
- Queries sem limitação de resultados causando problemas de memória
- Falta de middleware global para exceções não tratadas
- Referências circulares potenciais no Entity Framework

**Soluções Aplicadas**:
1. ✅ **Middleware global de exceções** em `Program.cs` para capturar crashes
2. ✅ **Limitação de resultados** (1000 registros) nas queries de relatórios
3. ✅ **Validação de intervalo** no servidor (máximo 365 dias)
4. ✅ **Logs detalhados** para rastreamento de problemas
5. ✅ **Endpoint de debug de memória** (`/api/relatorios/debug/memory`)
6. ✅ **AsNoTracking()** em todas as queries para otimização

### Robustez Adicional
- ✅ **Health check** para relatórios (`/api/relatorios/health`)
- ✅ **Logs melhorados** com timestamps e contadores de registros
- ✅ **Verificação de resposta** antes de escrever headers HTTP

## 🔄 Próximas Melhorias Sugeridas

### Performance e Escalabilidade
- [ ] **Índices de banco** nas colunas de data e chaves estrangeiras
- [ ] **Paginação** para listas grandes
- [ ] **Cache** de relatórios por janela de tempo
- [ ] **Processamento assíncrono** de relatórios grandes (SignalR + fila)

### Segurança
- [ ] **Variáveis de ambiente** para secrets JWT e conexão DB
- [ ] **CORS dinâmico** por configuração
- [ ] **Rate limiting** nos endpoints de relatórios

### Funcionalidades
- [ ] **PDF no servidor** (QuestPDF) para relatórios complexos
- [ ] **Notificações push** para alertas de sensores
- [ ] **Backup automático** do banco de dados
- [ ] **API de integração** para sensores IoT

### DevOps
- [ ] **Pipeline CI/CD** para build e deploy
- [ ] **Scripts de migração** e seed do banco
- [ ] **Containerização** com Docker
- [ ] **Monitoramento** com Application Insights

## 🐛 Debug

### Endpoints Úteis
- Health check: `GET /api/relatorios/health`
- Memória: `GET /api/relatorios/debug/memory`
- Swagger: `https://localhost:7135/swagger`

### Logs Importantes
- Verifique logs do Entity Framework para queries lentas
- Monitore memória antes/depois da geração de relatórios
- Middleware global captura todas as exceções não tratadas

## 📁 Estrutura do Projeto

```
GranjaTech/
├── GranjaTech.Api/          # API Controllers e configuração
├── GranjaTech.Application/  # DTOs e interfaces de serviços
├── GranjaTech.Domain/       # Entidades do domínio
├── GranjaTech.Infrastructure/ # Implementações, DbContext, serviços
└── frontend/                # React SPA
    ├── src/pages/          # Páginas da aplicação
    ├── src/services/       # Integração com API
    └── public/             # Assets estáticos
```

---

**Status**: ✅ Sistema funcional com correções de estabilidade implementadas.
