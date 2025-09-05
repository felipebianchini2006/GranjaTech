// frontend/src/pages/UsuariosPage.js
import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';
import PageContainer from '../components/PageContainer';
import LoadingSpinner from '../components/LoadingSpinner';
import { 
    Button, TextField, Box, Typography, Select, MenuItem, 
    FormControl, InputLabel, Table, TableBody, TableCell, TableContainer, 
    TableHead, TableRow, IconButton, Dialog, DialogTitle, DialogContent, DialogActions, Chip,
    Card, Alert
} from '@mui/material';
import {
    Edit as EditIcon,
    Delete as DeleteIcon,
    Add as AddIcon,
    People as PeopleIcon,
    Email as EmailIcon,
    AdminPanelSettings as AdminIcon,
    Person as PersonIcon
} from '@mui/icons-material';

const initialFormState = { id: 0, nome: '', email: '', senha: '', perfilId: '', produtorIds: [] };

function UsuariosPage() {
    const [users, setUsers] = useState([]);
    const [produtores, setProdutores] = useState([]);
    const [formData, setFormData] = useState(initialFormState);
    const [isEditMode, setIsEditMode] = useState(false);
    const [open, setOpen] = useState(false);
    const [message, setMessage] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(true);

    const fetchUsers = useCallback(async () => {
        try {
            setLoading(true);
            const response = await apiService.getUsers();
            setUsers(response.data);
            setProdutores(response.data.filter(u => u.perfilNome === 'Produtor'));
            setError('');
        } catch {
            setError("Não foi possível carregar a lista de usuários.");
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchUsers();
    }, [fetchUsers]);

    const handleOpen = async (user = null) => {
        if (user) {
            setIsEditMode(true);
            // Busca detalhes para obter produtorIds atuais
            try {
                const { data } = await apiService.getUserById(user.id);
                setFormData({
                    id: user.id,
                    nome: user.nome,
                    email: user.email,
                    senha: '',
                    perfilId: user.perfilId,
                    produtorIds: data?.produtorIds || []
                });
            } catch {
                setFormData({
                    id: user.id,
                    nome: user.nome,
                    email: user.email,
                    senha: '',
                    perfilId: user.perfilId,
                    produtorIds: user.produtorIds || []
                });
            }
        } else {
            setIsEditMode(false);
            setFormData(initialFormState);
        }
        setOpen(true);
    };

    const handleClose = () => {
        setOpen(false);
        setFormData(initialFormState);
        setMessage('');
        setError('');
    };

    const handleInputChange = (event) => {
        const { name, value } = event.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async () => {
        try {
            const userData = {
                nome: formData.nome,
                email: formData.email,
                ...(formData.senha && { senha: formData.senha }),
                perfilId: parseInt(formData.perfilId, 10),
                ...(String(formData.perfilId) === '3'
                    ? { produtorIds: (formData.produtorIds || []).map(Number) }
                    : {})
            };

            if (isEditMode) {
                await apiService.updateUser(formData.id, userData);
                setMessage("Usuário atualizado com sucesso!");
            } else {
                await apiService.createUser(userData);
                setMessage("Usuário criado com sucesso!");
            }
            
            fetchUsers();
            setTimeout(() => handleClose(), 1500);
        } catch (err) {
            setError(err?.response?.data?.message || "Erro ao salvar usuário.");
        }
    };

    const handleDelete = async (id) => {
        if (window.confirm('Tem certeza que deseja excluir este usuário?')) {
            try {
                await apiService.deleteUser(id);
                setMessage("Usuário excluído com sucesso!");
                fetchUsers();
                setTimeout(() => setMessage(''), 3000);
            } catch {
                setError("Erro ao excluir usuário.");
            }
        }
    };

    const getRoleIcon = (role) => {
        switch (role) {
            case 'Administrador': return AdminIcon;
            case 'Financeiro': return EmailIcon;
            default: return PersonIcon;
        }
    };

    const getRoleColor = (role) => {
        switch (role) {
            case 'Administrador': return 'error';
            case 'Financeiro': return 'warning';
            case 'Produtor': return 'success';
            default: return 'default';
        }
    };

    if (loading) return <LoadingSpinner message="Carregando usuários..." />;

    return (
        <PageContainer
            title="Usuários"
            subtitle="Gerencie usuários e permissões do sistema"
            action={
                <Button 
                    variant="contained" 
                    startIcon={<AddIcon />}
                    onClick={() => handleOpen()}
                    sx={{ borderRadius: 2, px: 3, py: 1.5 }}
                >
                    Novo Usuário
                </Button>
            }
        >
            {message && <Alert severity="success" sx={{ mb: 3 }} onClose={() => setMessage('')}>{message}</Alert>}
            {error && <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError('')}>{error}</Alert>}

            <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth PaperProps={{ sx: { borderRadius: 3 } }}>
                <DialogTitle sx={{ pb: 2 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                        <PeopleIcon color="primary" />
                        <Typography variant="h6" component="div">
                            {isEditMode ? 'Editar Usuário' : 'Novo Usuário'}
                        </Typography>
                    </Box>
                </DialogTitle>
                <DialogContent sx={{ pt: 2 }}>
                    <TextField
                        autoFocus
                        margin="normal"
                        name="nome"
                        label="Nome"
                        fullWidth
                        value={formData.nome}
                        onChange={handleInputChange}
                        sx={{ mb: 2 }}
                    />
                    <TextField
                        margin="normal"
                        name="email"
                        label="Email"
                        type="email"
                        fullWidth
                        value={formData.email}
                        onChange={handleInputChange}
                        sx={{ mb: 2 }}
                    />
                    <TextField
                        margin="normal"
                        name="senha"
                        label={isEditMode ? "Nova Senha (deixe vazio para manter)" : "Senha"}
                        type="password"
                        fullWidth
                        value={formData.senha}
                        onChange={handleInputChange}
                        sx={{ mb: 2 }}
                    />
                    <FormControl fullWidth margin="normal">
                        <InputLabel>Perfil</InputLabel>
                        <Select
                            name="perfilId"
                            value={formData.perfilId}
                            onChange={handleInputChange}
                            label="Perfil"
                        >
                            <MenuItem value={1}>Administrador</MenuItem>
                            <MenuItem value={2}>Produtor</MenuItem>
                            <MenuItem value={3}>Financeiro</MenuItem>
                        </Select>
                    </FormControl>

                    {String(formData.perfilId) === '3' && (
                        <FormControl fullWidth margin="normal">
                            <InputLabel id="produtores-label">Produtores do Financeiro</InputLabel>
                            <Select
                                labelId="produtores-label"
                                multiple
                                name="produtorIds"
                                value={formData.produtorIds || []}
                                onChange={(e) => {
                                    const v = e.target.value;
                                    setFormData(prev => ({
                                        ...prev,
                                        produtorIds: typeof v === 'string' ? v.split(',') : v
                                    }));
                                }}
                                renderValue={(selected) => (
                                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                                        {(selected || []).map((id) => {
                                            const p = produtores.find(x => String(x.id) === String(id));
                                            return <Chip key={id} label={p ? p.nome : id} size="small" />;
                                        })}
                                    </Box>
                                )}
                            >
                                {produtores.map((p) => (
                                    <MenuItem key={p.id} value={p.id}>
                                        {p.nome} ({p.email})
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                    )}
                </DialogContent>
                <DialogActions sx={{ p: 3, pt: 2 }}>
                    <Button onClick={handleClose} variant="outlined">Cancelar</Button>
                    <Button onClick={handleSubmit} variant="contained">{isEditMode ? 'Atualizar' : 'Criar'}</Button>
                </DialogActions>
            </Dialog>

            <Card>
                <TableContainer>
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableCell sx={{ fontWeight: 600 }}>Nome</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Email</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Perfil</TableCell>
                                <TableCell align="right" sx={{ fontWeight: 600 }}>Ações</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {users.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={4} align="center" sx={{ py: 8 }}>
                                        <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
                                            <PeopleIcon sx={{ fontSize: 48, color: 'text.secondary' }} />
                                            <Typography color="text.secondary">Nenhum usuário encontrado</Typography>
                                        </Box>
                                    </TableCell>
                                </TableRow>
                            ) : (
                                users.map((user) => {
                                    const RoleIcon = getRoleIcon(user.perfilNome);
                                    return (
                                        <TableRow 
                                            key={user.id}
                                            sx={{ '&:hover': { backgroundColor: 'action.hover' }, transition: 'background-color 0.2s ease-in-out' }}
                                        >
                                            <TableCell>
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                                                    <PersonIcon color="primary" sx={{ fontSize: 20 }} />
                                                    <Typography fontWeight={500}>{user.nome}</Typography>
                                                </Box>
                                            </TableCell>
                                            <TableCell>
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                    <EmailIcon color="action" sx={{ fontSize: 16 }} />
                                                    <Typography variant="body2">{user.email}</Typography>
                                                </Box>
                                            </TableCell>
                                            <TableCell>
                                                <Chip
                                                    icon={<RoleIcon />}
                                                    label={user.perfilNome}
                                                    color={getRoleColor(user.perfilNome)}
                                                    variant="outlined"
                                                    size="small"
                                                />
                                            </TableCell>
                                            <TableCell align="right">
                                                <IconButton onClick={() => handleOpen(user)} size="small" sx={{ mr: 1 }}>
                                                    <EditIcon fontSize="small" />
                                                </IconButton>
                                                <IconButton onClick={() => handleDelete(user.id)} size="small" color="error">
                                                    <DeleteIcon fontSize="small" />
                                                </IconButton>
                                            </TableCell>
                                        </TableRow>
                                    );
                                })
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Card>
        </PageContainer>
    );
}

export default UsuariosPage;
