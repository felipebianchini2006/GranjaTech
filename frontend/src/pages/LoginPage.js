import React, { useState, useContext } from 'react';
import { AuthContext } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { 
    Button, 
    TextField, 
    Box, 
    Typography, 
    Container, 
    Card, 
    CardContent,
    Alert,
    Avatar
} from '@mui/material';
import { Agriculture as AgricultureIcon } from '@mui/icons-material';
import LoadingSpinner from '../components/LoadingSpinner';

function LoginPage() {
    const [email, setEmail] = useState('');
    const [senha, setSenha] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const { login } = useContext(AuthContext);
    const navigate = useNavigate();

    const handleSubmit = async (event) => {
        event.preventDefault();
        setLoading(true);
        setError('');
        
        try {
            await login(email, senha);
            navigate('/');
        } catch (err) {
            setError('Email ou senha inválidos.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Box
            sx={{
                minHeight: '100vh',
                background: 'linear-gradient(135deg, #2E7D32 0%, #66BB6A 100%)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                p: 2,
            }}
        >
            <Container component="main" maxWidth="sm">
                <Card 
                    elevation={24}
                    sx={{ 
                        borderRadius: 4,
                        overflow: 'visible',
                        position: 'relative',
                    }}
                >
                    <CardContent sx={{ p: 6 }}>
                        <Box
                            sx={{
                                display: 'flex',
                                flexDirection: 'column',
                                alignItems: 'center',
                                mb: 4,
                            }}
                        >
                            <Avatar
                                sx={{
                                    width: 80,
                                    height: 80,
                                    bgcolor: 'primary.main',
                                    mb: 2,
                                    boxShadow: '0px 8px 24px rgba(46, 125, 50, 0.3)',
                                }}
                            >
                                <AgricultureIcon sx={{ fontSize: 40 }} />
                            </Avatar>
                            
                            <Typography 
                                component="h1" 
                                variant="h4" 
                                sx={{ 
                                    fontWeight: 700,
                                    color: 'primary.main',
                                    mb: 1,
                                }}
                            >
                                GranjaTech
                            </Typography>
                            
                            <Typography
                                variant="body1"
                                color="text.secondary"
                                align="center"
                                sx={{ mb: 3 }}
                            >
                                Sistema de Gestão Agropecuária
                            </Typography>
                        </Box>

                        <Box component="form" onSubmit={handleSubmit} noValidate>
                            <TextField
                                margin="normal"
                                required
                                fullWidth
                                id="email"
                                label="Email"
                                name="email"
                                autoComplete="email"
                                autoFocus
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                disabled={loading}
                                sx={{ mb: 2 }}
                            />
                            
                            <TextField
                                margin="normal"
                                required
                                fullWidth
                                name="senha"
                                label="Senha"
                                type="password"
                                id="senha"
                                autoComplete="current-password"
                                value={senha}
                                onChange={(e) => setSenha(e.target.value)}
                                disabled={loading}
                                sx={{ mb: 3 }}
                            />
                            
                            {error && (
                                <Alert severity="error" sx={{ mb: 3 }}>
                                    {error}
                                </Alert>
                            )}
                            
                            <Button
                                type="submit"
                                fullWidth
                                variant="contained"
                                disabled={loading || !email || !senha}
                                sx={{
                                    py: 1.5,
                                    fontSize: '1rem',
                                    fontWeight: 600,
                                    background: 'linear-gradient(135deg, #2E7D32, #4CAF50)',
                                    '&:hover': {
                                        background: 'linear-gradient(135deg, #1B5E20, #388E3C)',
                                    },
                                }}
                            >
                                {loading ? <LoadingSpinner size={24} message="" /> : 'Entrar'}
                            </Button>
                        </Box>
                    </CardContent>
                </Card>
            </Container>
        </Box>
    );
}

export default LoginPage;