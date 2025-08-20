import React, { useState, useEffect, useCallback, useContext } from 'react';
import { AuthContext } from '../context/AuthContext';
import apiService from '../services/apiService';
import {
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    Typography, Box, Button, Dialog, DialogActions, DialogContent,
    DialogTitle, TextField, IconButton, Select, MenuItem, FormControl, InputLabel
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';

const initialFormState = { id: 0, nome: '', localizacao: '', usuarioId: null };

function GranjasPage() {
    const { user } = useContext(AuthContext);
    const [granjas, setGranjas] = useState([]);
    const [produtores, setProdutores] = useState([]);
    const [open, setOpen] = useState(false);
    const [currentGranja, setCurrentGranja] = useState(initialFormState);
    const [isEditMode, setIsEditMode] = useState(false);

    const fetchData = useCallback(async () => {
        try {
            const granjasRes = await apiService.getGranjas();
            setGranjas(granjasRes.data);

            if (user?.role === 'Administrador') {
                const usersRes = await apiService.getUsers();
                setProdutores(usersRes.data.filter(u => u.perfilNome === 'Produtor'));
            }
        } catch (error) {
            console.error("Houve um erro ao buscar os dados:", error);
        }
    }, [user?.role]);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    const handleClickOpen = (granja = null) => {
        if (granja) {
            setIsEditMode(true);
            setCurrentGranja({ id: granja.id, nome: granja.nome, localizacao: granja.localizacao, usuarioId: granja.usuarioId });
        } else {
            setIsEditMode(false);
            setCurrentGranja(initialFormState);
        }
        setOpen(true);
    };

    const handleClose = () => setOpen(false);
    
    const handleInputChange = (event) => {
        const { name, value } = event.target;
        setCurrentGranja({ ...currentGranja, [name]: value });
    };

    const handleSubmit = async () => {
        try {
            if (isEditMode) {
                const updateDto = {
                    nome: currentGranja.nome,
                    localizacao: currentGranja.localizacao,
                    usuarioId: currentGranja.usuarioId
                };
                await apiService.updateGranja(currentGranja.id, updateDto);
            } else {
                const createDto = {
                    nome: currentGranja.nome,
                    localizacao: currentGranja.localizacao,
                    usuarioId: currentGranja.usuarioId ? parseInt(currentGranja.usuarioId, 10) : null
                };
                if (user?.role === 'Administrador' && !createDto.usuarioId) {
                    alert('Como Administrador, você deve selecionar um produtor dono da granja.');
                    return;
                }
                await apiService.createGranja(createDto);
            }
            handleClose();
            fetchData();
        } catch (error) {
            console.error("Houve um erro ao salvar a granja:", error);
            alert("Ocorreu um erro ao salvar a granja.");
        }
    };
    
    const handleDelete = async (id) => {
        if (window.confirm('Tem certeza que deseja excluir esta granja?')) {
            try {
                await apiService.deleteGranja(id);
                fetchData();
            } catch (error) {
                console.error("Houve um erro ao excluir a granja:", error);
            }
        }
    };

    return (
        <Box sx={{ margin: '20px', padding: '20px' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h4" component="h1">
                    Gerenciamento de Granjas
                </Typography>
                {user?.role !== 'Financeiro' && (
                    <Button variant="contained" onClick={() => handleClickOpen()}>
                        Adicionar Nova Granja
                    </Button>
                )}
            </Box>

            <Dialog open={open} onClose={handleClose}>
                <DialogTitle>{isEditMode ? 'Editar Granja' : 'Cadastrar Nova Granja'}</DialogTitle>
                <DialogContent>
                    <TextField autoFocus margin="dense" required fullWidth name="nome" label="Nome da Granja" value={currentGranja.nome} onChange={handleInputChange} />
                    <TextField margin="dense" fullWidth name="localizacao" label="Localização" value={currentGranja.localizacao || ''} onChange={handleInputChange} />
                    
                    {user?.role === 'Administrador' && (
                        <FormControl fullWidth margin="normal" required>
                            <InputLabel id="produtor-select-label">Dono (Produtor)</InputLabel>
                            <Select
                                name="usuarioId"
                                labelId="produtor-select-label"
                                value={currentGranja.usuarioId || ''}
                                label="Dono (Produtor)"
                                onChange={handleInputChange}
                            >
                                {produtores.map((p) => (
                                    <MenuItem key={p.id} value={p.id}>{p.nome}</MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                    )}
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose}>Cancelar</Button>
                    <Button onClick={handleSubmit}>Salvar</Button>
                </DialogActions>
            </Dialog>

            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Código</TableCell>
                            <TableCell>Nome</TableCell>
                            <TableCell>Localização</TableCell>
                            {/* COLUNA CONDICIONAL PARA O DONO */}
                            {(user?.role === 'Administrador' || user?.role === 'Financeiro') && <TableCell>Dono (Produtor)</TableCell>}
                            {user?.role !== 'Financeiro' && <TableCell align="right">Ações</TableCell>}
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {granjas.map((granja) => (
                            <TableRow key={granja.id}>
                                <TableCell>{granja.codigo}</TableCell>
                                <TableCell>{granja.nome}</TableCell>
                                <TableCell>{granja.localizacao}</TableCell>
                                {/* CÉLULA CONDICIONAL PARA O DONO */}
                                {(user?.role === 'Administrador' || user?.role === 'Financeiro') && <TableCell>{granja.usuario?.nome || 'N/A'}</TableCell>}
                                {user?.role !== 'Financeiro' && (
                                    <TableCell align="right">
                                        <IconButton onClick={() => handleClickOpen(granja)}><EditIcon /></IconButton>
                                        <IconButton onClick={() => handleDelete(granja.id)}><DeleteIcon /></IconButton>
                                    </TableCell>
                                )}
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </Box>
    );
}

export default GranjasPage;
