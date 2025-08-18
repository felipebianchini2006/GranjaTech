import React, { useContext } from 'react';
import { BrowserRouter as Router, Routes, Route, Link, Navigate } from 'react-router-dom';
import GranjasPage from './pages/GranjasPage';
import LotesPage from './pages/LotesPage';
import LoginPage from './pages/LoginPage';
import UsuariosPage from './pages/UsuariosPage';
import FinanceiroPage from './pages/FinanceiroPage'; // Importe a nova página
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
              GranjaTech
            </Typography>
            {token && (
              <>
                <Button color="inherit" component={Link} to="/">Granjas</Button>
                <Button color="inherit" component={Link} to="/lotes">Lotes</Button>
                
                {/* Link condicional para Financeiro */}
                {(user?.role === 'Administrador' || user?.role === 'Financeiro') && (
                   <Button color="inherit" component={Link} to="/financeiro">Financeiro</Button>
                )}

                {/* Link condicional para Usuários */}
                {user?.role === 'Administrador' && (
                   <Button color="inherit" component={Link} to="/usuarios">Usuários</Button>
                )}
                
                <Button color="inherit" onClick={logout}>Logout</Button>
              </>
            )}
          </Toolbar>
        </AppBar>
        
        <Routes>
          {/* Rotas públicas */}
          <Route path="/login" element={<LoginPage />} />
          
          {/* Rotas protegidas */}
          <Route path="/" element={<ProtectedRoute><GranjasPage /></ProtectedRoute>} />
          <Route path="/lotes" element={<ProtectedRoute><LotesPage /></ProtectedRoute>} />
          <Route path="/usuarios" element={<ProtectedRoute><UsuariosPage /></ProtectedRoute>} />
          <Route path="/financeiro" element={<ProtectedRoute><FinanceiroPage /></ProtectedRoute>} /> {/* Nova rota protegida */}

          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </Box>
    </Router>
  );
}

export default App;