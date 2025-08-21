import React, { useContext } from 'react';
import { BrowserRouter as Router, Routes, Route, Link, Navigate } from 'react-router-dom';
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
import RelatoriosPage from './pages/RelatoriosPage'; // Importe a nova página
import ProtectedRoute from './components/ProtectedRoute';
import { AuthContext } from './context/AuthContext';
import { Box, AppBar, Toolbar, Button, Typography } from '@mui/material';

function App() {
  const { token, user, logout } = useContext(AuthContext);

  return (
    <Router>
      <Box sx={{ flexGrow: 1 }}>
        <AppBar position="static">
          <Toolbar>
            <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
              <Link to="/" style={{ textDecoration: 'none', color: 'inherit' }}>
                  GranjaTech
              </Link>
            </Typography>
            {token && (
              <>
                <Button color="inherit" component={Link} to="/granjas">Granjas</Button>
                <Button color="inherit" component={Link} to="/lotes">Lotes</Button>
                
                {(user?.role === 'Administrador' || user?.role === 'Produtor') && (
                   <Button color="inherit" component={Link} to="/estoque">Estoque</Button>
                )}

                {(user?.role === 'Administrador' || user?.role === 'Produtor') && (
                   <Button color="inherit" component={Link} to="/sensores">Sensores</Button>
                )}

                {(user?.role === 'Administrador' || user?.role === 'Financeiro') && (
                   <Button color="inherit" component={Link} to="/financeiro">Financeiro</Button>
                )}

                {/* ADICIONE O NOVO LINK CONDICIONAL */}
                {(user?.role === 'Administrador' || user?.role === 'Financeiro' || user?.role === 'Produtor') && (
                   <Button color="inherit" component={Link} to="/relatorios">Relatórios</Button>
                )}

                {user?.role === 'Administrador' && (
                   <Button color="inherit" component={Link} to="/usuarios">Utilizadores</Button>
                )}
                
                {user?.role === 'Administrador' && (
                   <Button color="inherit" component={Link} to="/auditoria">Auditoria</Button>
                )}

                <Button color="inherit" component={Link} to="/perfil">Perfil</Button>
                <Button color="inherit" onClick={logout}>Logout</Button>
              </>
            )}
          </Toolbar>
        </AppBar>
        
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
          <Route path="/relatorios" element={<ProtectedRoute><RelatoriosPage /></ProtectedRoute>} /> {/* Nova rota protegida */}

          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </Box>
    </Router>
  );
}

export default App;
