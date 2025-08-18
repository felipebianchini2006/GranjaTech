import React, { useState, useContext } from 'react';
import { AuthContext } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { Button, TextField, Box, Typography, Container, Paper } from '@mui/material';

function LoginPage() {
    const [email, setEmail] = useState('');
    const [senha, setSenha] = useState('');
    const [error, setError] = useState('');
    const { login } = useContext(AuthContext);
    const navigate = useNavigate();

    const handleSubmit = async (event) => {
        event.preventDefault();
        try {
            await login(email, senha);
            navigate('/');
        } catch (err) {
            setError('Email ou senha inválidos.');
        }
    };

    return (
        <Container component="main" maxWidth="xs">
            <Paper elevation={6} sx={{ marginTop: 8, padding: 4, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                <Typography component="h1" variant="h5">
                    Login - GranjaTech
                </Typography>
                <Box component="form" onSubmit={handleSubmit} noValidate sx={{ mt: 1 }}>
                    <TextField margin="normal" required fullWidth id="email" label="Endereço de Email" name="email" autoComplete="email" autoFocus value={email} onChange={(e) => setEmail(e.target.value)} />
                    <TextField margin="normal" required fullWidth name="senha" label="Senha" type="password" id="senha" autoComplete="current-password" value={senha} onChange={(e) => setSenha(e.target.value)} />
                    {error && <Typography color="error" align="center">{error}</Typography>}
                    <Button type="submit" fullWidth variant="contained" sx={{ mt: 3, mb: 2 }}>
                        Entrar
                    </Button>
                </Box>
            </Paper>
        </Container>
    );
}

export default LoginPage;