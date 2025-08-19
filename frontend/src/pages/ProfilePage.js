import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';
import { Button, TextField, Box, Typography, Container, Paper, Grid, Alert } from '@mui/material';

function ProfilePage() {
    const [profileData, setProfileData] = useState({ nome: '', email: '' });
    const [passwordData, setPasswordData] = useState({ senhaAtual: '', novaSenha: '', confirmaNovaSenha: '' });
    
    const [profileMessage, setProfileMessage] = useState({ type: '', text: '' });
    const [passwordMessage, setPasswordMessage] = useState({ type: '', text: '' });

    const fetchProfile = useCallback(async () => {
        try {
            const response = await apiService.getProfile();
            setProfileData(response.data);
        } catch (error) {
            console.error("Erro ao buscar perfil:", error);
            setProfileMessage({ type: 'error', text: 'Erro ao carregar dados do perfil.' });
        }
    }, []);

    useEffect(() => {
        fetchProfile();
    }, [fetchProfile]);

    const handleProfileChange = (e) => {
        setProfileMessage({ type: '', text: '' });
        const { name, value } = e.target;
        setProfileData({ ...profileData, [name]: value });
    };

    const handlePasswordChange = (e) => {
        setPasswordMessage({ type: '', text: '' });
        const { name, value } = e.target;
        setPasswordData({ ...passwordData, [name]: value });
    };

    const handleProfileSubmit = async (e) => {
        e.preventDefault();
        setProfileMessage({ type: '', text: '' });
        try {
            await apiService.updateProfile(profileData);
            setProfileMessage({ type: 'success', text: 'Perfil atualizado com sucesso!' });
        } catch (error) {
            setProfileMessage({ type: 'error', text: error.response?.data?.message || 'Erro ao atualizar perfil.' });
        }
    };

    const handlePasswordSubmit = async (e) => {
        e.preventDefault();
        setPasswordMessage({ type: '', text: '' });
        if (passwordData.novaSenha !== passwordData.confirmaNovaSenha) {
            setPasswordMessage({ type: 'error', text: 'As novas senhas não coincidem.' });
            return;
        }
        try {
            const response = await apiService.changePassword({
                senhaAtual: passwordData.senhaAtual,
                novaSenha: passwordData.novaSenha,
            });
            setPasswordMessage({ type: 'success', text: response.data.message });
            setPasswordData({ senhaAtual: '', novaSenha: '', confirmaNovaSenha: '' });
        } catch (error) {
            setPasswordMessage({ type: 'error', text: error.response?.data?.message || 'Erro ao alterar a senha.' });
        }
    };

    return (
        <Container maxWidth="md" sx={{ mt: 4 }}>
            <Typography variant="h4" gutterBottom>Meu Perfil</Typography>
            <Grid container spacing={4}>
                <Grid item xs={12} md={6}>
                    <Paper sx={{ p: 3 }}>
                        <Typography variant="h6" gutterBottom>Editar Informações</Typography>
                        <Box component="form" onSubmit={handleProfileSubmit}>
                            <TextField fullWidth required margin="normal" name="nome" label="Nome Completo" value={profileData.nome} onChange={handleProfileChange} />
                            <TextField fullWidth required margin="normal" name="email" label="Email" type="email" value={profileData.email} onChange={handleProfileChange} />
                            {profileMessage.text && <Alert severity={profileMessage.type} sx={{ mt: 2 }}>{profileMessage.text}</Alert>}
                            <Button type="submit" variant="contained" sx={{ mt: 2 }}>Salvar Alterações</Button>
                        </Box>
                    </Paper>
                </Grid>
                <Grid item xs={12} md={6}>
                    <Paper sx={{ p: 3 }}>
                        <Typography variant="h6" gutterBottom>Alterar Senha</Typography>
                        <Box component="form" onSubmit={handlePasswordSubmit}>
                            <TextField fullWidth required margin="normal" name="senhaAtual" label="Senha Atual" type="password" value={passwordData.senhaAtual} onChange={handlePasswordChange} />
                            <TextField fullWidth required margin="normal" name="novaSenha" label="Nova Senha" type="password" value={passwordData.novaSenha} onChange={handlePasswordChange} />
                            <TextField fullWidth required margin="normal" name="confirmaNovaSenha" label="Confirmar Nova Senha" type="password" value={passwordData.confirmaNovaSenha} onChange={handlePasswordChange} />
                            {passwordMessage.text && <Alert severity={passwordMessage.type} sx={{ mt: 2 }}>{passwordMessage.text}</Alert>}
                            <Button type="submit" variant="contained" sx={{ mt: 2 }}>Alterar Senha</Button>
                        </Box>
                    </Paper>
                </Grid>
            </Grid>
        </Container>
    );
}

export default ProfilePage;
