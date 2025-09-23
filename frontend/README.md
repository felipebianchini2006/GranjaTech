# GranjaTech Frontend

Sistema moderno de gestão agropecuária desenvolvido em React + Material-UI.

## ⚡ Funcionalidades

✅ **Dashboard** com KPIs e gráficos em tempo real  
✅ **CRUD completo** para granjas, lotes e estoque  
✅ **Sistema financeiro** com controle de entradas/saídas  
✅ **Relatórios** exportáveis em PDF e Excel  
✅ **Monitoramento** de sensores IoT  
✅ **Gestão de usuários** com diferentes perfis  
✅ **Auditoria** completa de ações  

## 🛠️ Stack

**Core:** React 19.1, Material-UI 7.3, React Router 7.8, Axios  
**Charts:** Recharts 3.1, jsPDF, XLSX  
**Auth:** JWT Decode  
**Style:** Emotion, Google Fonts (Inter)

## 🚀 Setup

```bash
# Pré-requisitos: Node.js 16+
npm install && npm start  # http://localhost:3000
npm run build            # Produção
```

## 📁 Estrutura

```
frontend/src/
├── components/    # Componentes reutilizáveis
├── context/       # Contextos React (Auth)
├── pages/         # Páginas da aplicação
├── services/      # API services
├── theme.js       # Tema Material-UI
└── App.js         # Componente principal
```

## 🎨 Design System

**Tema:** Verde agropecuário com tipografia Inter  
**Responsivo:** Mobile-first com breakpoints adaptativos  
**UX:** Loading states, validação em tempo real, toast notifications

## 🔧 Próximas Melhorias

- [ ] Dark mode e animações de transição
- [ ] Notificações push e modo offline  
- [ ] Lazy loading e bundle splitting