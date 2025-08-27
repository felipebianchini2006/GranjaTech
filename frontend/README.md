# GranjaTech Frontend

Sistema moderno de gestão agropecuária desenvolvido em React.js com Material-UI.

## 🚀 Funcionalidades

### ✅ Implementadas
- **Dashboard Interativo**: Visão geral com KPIs e gráficos em tempo real
- **Gestão de Granjas**: CRUD completo para propriedades agropecuárias
- **Gestão de Lotes**: Controle de lotes de produção
- **Sistema Financeiro**: Gestão de entradas e saídas
- **Relatórios**: Geração de relatórios em PDF e Excel
- **Gestão de Estoque**: Controle de produtos e insumos
- **Monitoramento de Sensores**: Dashboard para sensores IoT
- **Sistema de Usuários**: Gestão completa com diferentes perfis
- **Auditoria**: Log de ações do sistema
- **Perfil de Usuário**: Gestão de conta pessoal

## 🎨 Melhorias Visuais Recentes

### Design System Moderno
- **Tema Personalizado**: Paleta de cores profissional focada no agronegócio
- **Tipografia**: Fonte Inter para melhor legibilidade
- **Componentes**: Cards, botões e formulários redesenhados
- **Espaçamento**: Sistema de grid responsivo otimizado

### Navegação Responsiva
- **Sidebar Inteligente**: Menu lateral com ícones e categorização
- **Mobile First**: Menu hambúrguer para dispositivos móveis
- **Breadcrumbs**: Navegação contextual em todas as páginas
- **Estado Ativo**: Indicadores visuais da página atual

### Experiência do Usuário
- **Loading States**: Spinners e feedbacks visuais
- **Estados Vazios**: Mensagens e ícones para listas vazias
- **Sistema de Notificações**: Toast messages para feedback
- **Validação Visual**: Campos com validação em tempo real

### Componentes Visuais
- **KPI Cards**: Cards animados com gradientes e ícones
- **Tabelas Melhoradas**: Hover effects e tipografia aprimorada
- **Formulários**: Campos com melhor espaçamento e validação
- **Gráficos**: Charts responsivos com tooltips customizados

## 📱 Responsividade

### Breakpoints
- **Mobile**: < 600px
- **Tablet**: 600px - 960px
- **Desktop**: > 960px

### Adaptações Móveis
- Menu lateral colapsável
- Grids que se adaptam ao tamanho da tela
- Formulários otimizados para touch
- Tabelas com scroll horizontal quando necessário

## 🛠️ Tecnologias

### Core
- **React 19.1.1**: Framework principal
- **Material-UI 7.3.1**: Biblioteca de componentes
- **React Router 7.8.1**: Roteamento
- **Axios 1.11.0**: Cliente HTTP

### Funcionalidades
- **Recharts 3.1.2**: Gráficos e dashboards
- **jsPDF 3.0.1**: Geração de PDFs
- **jsPDF AutoTable 5.0.2**: Tabelas em PDF
- **XLSX 0.18.5**: Exportação para Excel
- **JWT Decode 4.0.0**: Autenticação

### Styling
- **Emotion**: CSS-in-JS para Material-UI
- **Google Fonts**: Fonte Inter
- **CSS Grid/Flexbox**: Layout responsivo

## 🚀 Como Executar

### Pré-requisitos
- Node.js 16+ 
- npm ou yarn

### Instalação
```bash
# Instalar dependências
npm install

# Executar em desenvolvimento
npm start

# Build para produção
npm run build
```

### Configuração
O frontend está configurado para se conectar ao backend em:
- **Desenvolvimento**: `http://localhost:5099`
- **Produção**: Configurar variável de ambiente

## 📁 Estrutura do Projeto

```
frontend/
├── public/                 # Arquivos públicos
├── src/
│   ├── components/         # Componentes reutilizáveis
│   │   ├── LoadingSpinner.js
│   │   ├── PageContainer.js
│   │   ├── ResponsiveNavigation.js
│   │   ├── NotificationContext.js
│   │   └── ProtectedRoute.js
│   ├── context/           # Contextos React
│   │   └── AuthContext.js
│   ├── pages/             # Páginas da aplicação
│   │   ├── DashboardPage.js
│   │   ├── GranjasPage.js
│   │   ├── LotesPage.js
│   │   ├── FinanceiroPage.js
│   │   ├── RelatoriosPage.js
│   │   ├── EstoquePage.js
│   │   ├── SensoresPage.js
│   │   ├── UsuariosPage.js
│   │   ├── AuditoriaPage.js
│   │   ├── ProfilePage.js
│   │   └── LoginPage.js
│   ├── services/          # Serviços de API
│   │   └── apiService.js
│   ├── theme.js           # Tema customizado
│   ├── App.js            # Componente principal
│   ├── index.js          # Ponto de entrada
│   └── index.css         # Estilos globais
```

## 🎯 Próximas Melhorias

### UX/UI
- [ ] Dark mode
- [ ] Animações de transição
- [ ] Drag and drop
- [ ] Upload de arquivos com preview

### Funcionalidades
- [ ] Notificações push
- [ ] Modo offline
- [ ] Busca avançada
- [ ] Filtros salvos

### Performance
- [ ] Lazy loading de páginas
- [ ] Cache de dados
- [ ] Service Worker
- [ ] Bundle splitting

## 🔧 Configuração de Desenvolvimento

### VSCode Extensions Recomendadas
- ES7+ React/Redux/React-Native snippets
- Prettier - Code formatter
- ESLint
- Auto Rename Tag
- Material Icon Theme

### Scripts Disponíveis
```bash
npm start          # Servidor de desenvolvimento
npm run build      # Build para produção
npm test           # Executar testes
npm run eject      # Ejetar configuração (não recomendado)
```

## 📝 Convenções

### Nomenclatura
- **Componentes**: PascalCase (ex: `PageContainer`)
- **Arquivos**: PascalCase para componentes, camelCase para utilitários
- **Pastas**: camelCase
- **Constantes**: UPPER_SNAKE_CASE

### Estrutura de Componentes
```jsx
// Imports
import React from 'react';
import { Component } from '@mui/material';

// Component
function MyComponent({ prop1, prop2 }) {
  // Hooks
  // Event handlers
  // Render
  return (
    <div>
      {/* JSX */}
    </div>
  );
}

// Export
export default MyComponent;
```

## 🌟 Características Técnicas

### Design System
- **Paleta de Cores**: Verde agropecuário com tons complementares
- **Tipografia**: Inter 300-800, otimizada para legibilidade
- **Spacing**: Sistema de 8px base
- **Border Radius**: 8px-16px para componentes
- **Sombras**: Sistema de elevação sutil

### Acessibilidade
- Contraste adequado (WCAG AA)
- Navegação por teclado
- Labels e descrições para screen readers
- Focus indicators customizados

### Performance
- Bundle size otimizado
- Tree shaking automático
- Componentes lazy quando necessário
- Imagens otimizadas

---

**Desenvolvido com ❤️ para o agronegócio brasileiro**