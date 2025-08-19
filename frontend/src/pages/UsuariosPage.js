import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';
import { 
    Button, TextField, Box, Typography, Container, Paper, Select, MenuItem, 
    FormControl, InputLabel, Table, TableBody, TableCell, TableContainer, 
    TableHead, TableRow, IconButton, Dialog, DialogTitle, DialogContent, DialogActions, Chip
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';

const initialFormState = { id: 0, nome: '', email: '', senha: '', perfilId: '', produtorIds: [] };

function UsuariosPage() {
    const [users, setUsers] = useState([]);
    const [produtores, setProdutores] = useState([]);
    const [formData, setFormData] = useState(initialFormState);
    const [isEditMode, setIsEditMode] = useState(false);
    const [open, setOpen] = useState(false);
    const [message, setMessage] = useState('');
    const [error, setError] = useState('');

    const fetchUsers = useCallback(async () => {
        try {
            const response = await apiService.getUsers();
            setUsers(response.data);
            setProdutores(response.data.filter(user => user.perfilNome === 'Produtor'));
        } catch (err) {
            setError("Não foi possível carregar a lista de usuários.");
        }
    }, []);

    useEffect(() => {
        fetchUsers();
    }, [fetchUsers]);

    const handleOpen = async (user = null) => {
        setMessage('');
        setError('');
        if (user) {
            setIsEditMode(true);
            try {
                const response = await apiService.getUserById(user.id);
                setFormData(response.data);
            } catch (err) {
                setError("Falha ao carregar dados do usuário para edição.");
            }
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
                const { id, nome, email, perfilId, produtorIds } = formData;
                await apiService.updateUser(id, { nome, email, perfilId, produtorIds });
                setMessage('Usuário atualizado com sucesso!');
            } else {
                if (!formData.senha) {
                    setError('O campo senha é obrigatório para novos usuários.');
                    return;
                }
                if (formData.perfilId === 3 && formData.produtorIds.length === 0) {
                    setError('Para o perfil Financeiro, é obrigatório selecionar ao menos um Produtor.');
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
                if (err.response && err.response.data && err.response.data.message) {
                    alert(err.response.data.message);
                } else {
                    alert('Ocorreu um erro ao tentar excluir o usuário.');
                    console.error("Erro ao excluir usuário:", err);
                }
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
                                <TableCell>Código</TableCell>
                                <TableCell>Nome</TableCell>
                                <TableCell>Email</TableCell>
                                <TableCell>Perfil</TableCell>
                                <TableCell align="right">Ações</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {users.map((user) => (
                                <TableRow key={user.id}>
                                    <TableCell>{user.codigo}</TableCell>
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

                <Dialog open={open} onClose={handleClose} fullWidth maxWidth="sm">
                    <form onSubmit={handleSubmit}>
                        <DialogTitle>{isEditMode ? 'Editar Usuário' : 'Adicionar Novo Usuário'}</DialogTitle>
                        <DialogContent>
                            <TextField autoFocus margin="dense" required fullWidth id="nome" label="Nome Completo" name="nome" value={formData.nome} onChange={handleInputChange} />
                            <TextField margin="dense" required fullWidth id="email" label="Endereço de Email" name="email" value={formData.email} onChange={handleInputChange} />
                            {!isEditMode && (
                                <TextField margin="dense" required fullWidth name="senha" label="Senha" type="password" id="senha" value={formData.senha} onChange={handleInputChange} />
                            )}
                            <FormControl fullWidth margin="normal" required>
                                <InputLabel>Perfil</InputLabel>
                                <Select name="perfilId" value={formData.perfilId} onChange={handleInputChange}>
                                    <MenuItem value={1}>Administrador</MenuItem>
                                    <MenuItem value={2}>Produtor</MenuItem>
                                    <MenuItem value={3}>Financeiro</MenuItem>
                                </Select>
                            </FormControl>
                            {formData.perfilId === 3 && (
                                <FormControl fullWidth margin="normal" required>
                                    <InputLabel>Produtores Associados</InputLabel>
                                    <Select
                                        name="produtorIds"
                                        multiple
                                        value={formData.produtorIds}
                                        onChange={handleInputChange}
                                        renderValue={(selected) => (
                                            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                                                {selected.map((id) => {
                                                    const produtor = produtores.find(p => p.id === id);
                                                    return <Chip key={id} label={produtor ? produtor.nome : id} />;
                                                })}
                                            </Box>
                                        )}
                                    >
                                        {produtores.map((produtor) => (
                                            <MenuItem key={produtor.id} value={produtor.id}>
                                                {produtor.nome}
                                            </MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                            )}
                        </DialogContent>
                        <DialogActions>
                            <Box sx={{ flexGrow: 1, ml: 2, mr: 2 }}>
                                {message && <Typography color="primary">{message}</Typography>}
                                {error && <Typography color="error">{error}</Typography>}
                            </Box>
                            <Button onClick={handleClose}>Cancelar</Button>
                            <Button type="submit">Salvar</Button>
                        </DialogActions>
                    </form>
                </Dialog>
            </Paper>
        </Container>
    );
}
export default UsuariosPage;
