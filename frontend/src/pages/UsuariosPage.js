import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';
import { 
    Button, TextField, Box, Typography, Container, Paper, Select, MenuItem, 
    FormControl, InputLabel, Table, TableBody, TableCell, TableContainer, 
    TableHead, TableRow, IconButton, Dialog, DialogTitle, DialogContent, DialogActions
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';

const initialFormState = { id: 0, nome: '', email: '', senha: '', perfilId: '' };

function UsuariosPage() {
    const [users, setUsers] = useState([]);
    const [formData, setFormData] = useState(initialFormState);
    const [isEditMode, setIsEditMode] = useState(false);
    const [open, setOpen] = useState(false);
    const [message, setMessage] = useState('');
    const [error, setError] = useState('');

    const fetchUsers = useCallback(async () => {
        try {
            const response = await apiService.getUsers();
            setUsers(response.data);
        } catch (err) {
            setError("Não foi possível carregar a lista de usuários.");
        }
    }, []);

    useEffect(() => {
        fetchUsers();
    }, [fetchUsers]);

    const handleOpen = (user = null) => {
        setMessage('');
        setError('');
        if (user) {
            setIsEditMode(true);
            setFormData({ id: user.id, nome: user.nome, email: user.email, perfilId: users.find(u => u.perfilNome === user.perfilNome)?.id || '' });
        } else {
            setIsEditMode(false);
            setFormData(initialFormState);
        }
        setOpen(true);
    };

    const handleClose = () => setOpen(false);

    const handleInputChange = (event) => {
        const { name, value } = event.target;
        setFormData({ ...formData, [name]: value });
    };

    const handleSubmit = async (event) => {
        event.preventDefault();
        setMessage('');
        setError('');

        try {
            if (isEditMode) {
                // Lógica de Edição
                const { id, nome, email, perfilId } = formData;
                await apiService.updateUser(id, { nome, email, perfilId });
                setMessage('Usuário atualizado com sucesso!');
            } else {
                // Lógica de Criação
                if (!formData.senha) {
                    setError('O campo senha é obrigatório para novos usuários.');
                    return;
                }
                await apiService.registrar(formData);
                setMessage('Usuário registrado com sucesso!');
            }
            fetchUsers();
            handleClose();
        } catch (err) {
            setError(err.response?.data?.message || err.response?.data || 'Ocorreu um erro.');
        }
    };

    const handleDelete = async (id) => {
        if (window.confirm('Tem certeza que deseja excluir este usuário?')) {
            try {
                await apiService.deleteUser(id);
                fetchUsers();
            } catch (err) {
                setError('Erro ao excluir usuário.');
            }
        }
    };

    return (
        <Container component="main" maxWidth="md">
            <Paper elevation={6} sx={{ marginTop: 4, padding: 4 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                    <Typography component="h1" variant="h5">Gerenciamento de Usuários</Typography>
                    <Button variant="contained" onClick={() => handleOpen()}>Adicionar Novo Usuário</Button>
                </Box>

                <TableContainer component={Paper} sx={{ mb: 4 }}>
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableCell>Nome</TableCell>
                                <TableCell>Email</TableCell>
                                <TableCell>Perfil</TableCell>
                                <TableCell align="right">Ações</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {users.map((user) => (
                                <TableRow key={user.id}>
                                    <TableCell>{user.nome}</TableCell>
                                    <TableCell>{user.email}</TableCell>
                                    <TableCell>{user.perfilNome}</TableCell>
                                    <TableCell align="right">
                                        <IconButton onClick={() => handleOpen(user)}><EditIcon /></IconButton>
                                        <IconButton onClick={() => handleDelete(user.id)}><DeleteIcon /></IconButton>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>

                <Dialog open={open} onClose={handleClose}>
                    <DialogTitle>{isEditMode ? 'Editar Usuário' : 'Adicionar Novo Usuário'}</DialogTitle>
                    <DialogContent>
                        <TextField autoFocus margin="dense" required fullWidth id="nome" label="Nome Completo" name="nome" value={formData.nome} onChange={handleInputChange}/>
                        <TextField margin="dense" required fullWidth id="email" label="Endereço de Email" name="email" value={formData.email} onChange={handleInputChange}/>
                        {!isEditMode && (
                            <TextField margin="dense" required fullWidth name="senha" label="Senha" type="password" id="senha" value={formData.senha} onChange={handleInputChange}/>
                        )}
                        <FormControl fullWidth margin="normal" required>
                            <InputLabel id="perfil-select-label">Perfil</InputLabel>
                            <Select labelId="perfil-select-label" name="perfilId" value={formData.perfilId} label="Perfil" onChange={handleInputChange}>
                                <MenuItem value={1}>Administrador</MenuItem>
                                <MenuItem value={2}>Produtor</MenuItem>
                                <MenuItem value={3}>Financeiro</MenuItem>
                            </Select>
                        </FormControl>
                    </DialogContent>
                    <DialogActions>
                        {error && <Typography color="error" sx={{ mr: 2 }}>{error}</Typography>}
                        {message && <Typography color="primary" sx={{ mr: 2 }}>{message}</Typography>}
                        <Button onClick={handleClose}>Cancelar</Button>
                        <Button onClick={handleSubmit}>Salvar</Button>
                    </DialogActions>
                </Dialog>
            </Paper>
        </Container>
    );
}

export default UsuariosPage;