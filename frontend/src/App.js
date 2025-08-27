import React, { useContext } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import { CssBaseline } from '@mui/material';
import theme from './theme';
import GranjasPage from './pages/GranjasPage';
import LotesPage from './pages/LotesPage';
import LoginPage from './pages/LoginPage';
import UsuariosPage from './pages/UsuariosPage';
import FinanceiroPage from './pages/FinanceiroPage';
import DashboardPage from './pages/DashboardPage';
import AuditoriaPage from './pages/AuditoriaPage';
import ProfilePage from './pages/ProfilePage';
import EstoquePage from './pages/EstoquePage';
import SensoresPage from './pages/SensoresPage';
import RelatoriosPage from './pages/RelatoriosPage';
import ProtectedRoute from './components/ProtectedRoute';
import ResponsiveNavigation from './components/ResponsiveNavigation';
import { AuthContext } from './context/AuthContext';

function App() {
  const { token } = useContext(AuthContext);

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Router>
        {token && <ResponsiveNavigation />}
        
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          
          <Route path="/" element={<ProtectedRoute><DashboardPage /></ProtectedRoute>} />
          <Route path="/granjas" element={<ProtectedRoute><GranjasPage /></ProtectedRoute>} />
          <Route path="/lotes" element={<ProtectedRoute><LotesPage /></ProtectedRoute>} />
          <Route path="/usuarios" element={<ProtectedRoute><UsuariosPage /></ProtectedRoute>} />
          <Route path="/financeiro" element={<ProtectedRoute><FinanceiroPage /></ProtectedRoute>} />
          <Route path="/auditoria" element={<ProtectedRoute><AuditoriaPage /></ProtectedRoute>} />
          <Route path="/perfil" element={<ProtectedRoute><ProfilePage /></ProtectedRoute>} />
          <Route path="/estoque" element={<ProtectedRoute><EstoquePage /></ProtectedRoute>} />
          <Route path="/sensores" element={<ProtectedRoute><SensoresPage /></ProtectedRoute>} />
          <Route path="/relatorios" element={<ProtectedRoute><RelatoriosPage /></ProtectedRoute>} />

          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </Router>
    </ThemeProvider>
  );
}

export default App;