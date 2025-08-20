import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';
import { Button, TextField, Box, Typography, Container, Paper, Grid, Alert, Chip } from '@mui/material';

function ProfilePage() {
    // Estado para o formulário de edição
    const [profileData, setProfileData] = useState({ nome: '', email: '' });
    // Estado para os detalhes completos do perfil (incluindo cargo e associações)
    const [profileDetails, setProfileDetails] = useState(null);
    const [passwordData, setPasswordData] = useState({ senhaAtual: '', novaSenha: '', confirmaNovaSenha: '' });
    
    const [profileMessage, setProfileMessage] = useState({ type: '', text: '' });
    const [passwordMessage, setPasswordMessage] = useState({ type: '', text: '' });

    const fetchProfile = useCallback(async () => {
        try {
            const response = await apiService.getProfile();
            setProfileDetails(response.data);
            // Popula o formulário de edição com os dados recebidos
            setProfileData({ nome: response.data.nome, email: response.data.email });
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
            fetchProfile(); // Re-busca os dados para atualizar o nome no topo, se alterado
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
                {/* NOVO PAINEL DE INFORMAÇÕES DE CARGO */}
                {profileDetails && (
                    <Grid item xs={12}>
                        <Paper sx={{ p: 3, mb: 2 }}>
                            <Typography variant="h6" gutterBottom>Informações do Cargo</Typography>
                            <Typography variant="body1">
                                <strong>Cargo:</strong> {profileDetails.perfilNome}
                            </Typography>
                            {profileDetails.associados.length > 0 && (
                                <Box sx={{ mt: 2 }}>
                                    <Typography variant="body1" component="strong">
                                        {profileDetails.perfilNome === 'Produtor' ? 'Responsável por:' : 'Financeiro de:'}
                                    </Typography>
                                    <Box sx={{ mt: 1 }}>
                                        {profileDetails.associados.map((nome, index) => (
                                            <Chip key={index} label={nome} sx={{ mr: 1, mt: 1 }} />
                                        ))}
                                    </Box>
                                </Box>
                            )}
                        </Paper>
                    </Grid>
                )}

                {/* Formulário de Informações do Perfil */}
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
                {/* Formulário de Alteração de Senha */}
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
