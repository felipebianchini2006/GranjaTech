import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';

// Importando mais componentes e ícones do Material-UI
import { 
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    Typography, Box, Button, Dialog, DialogActions, DialogContent, 
    DialogTitle, TextField, IconButton 
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';

function GranjasPage() {
    const [granjas, setGranjas] = useState([]);
    const [open, setOpen] = useState(false);
    
    // Estado para guardar os dados do formulário (agora chamado 'currentGranja')
    const [currentGranja, setCurrentGranja] = useState({ id: 0, nome: '', localizacao: '' });

    // Envolvemos fetchGranjas com useCallback para otimização
    const fetchGranjas = useCallback(async () => {
        try {
            const response = await apiService.getGranjas();
            setGranjas(response.data);
        } catch (error) {
            console.error("Houve um erro ao buscar as granjas:", error);
        }
    }, []);

    useEffect(() => {
        fetchGranjas();
    }, [fetchGranjas]);

    // Funções para controlar o modal
    const handleClickOpen = (granja = { id: 0, nome: '', localizacao: '' }) => {
        setCurrentGranja(granja); // Se for edição, carrega dados; se for criação, usa o objeto vazio
        setOpen(true);
    };
    
    const handleClose = () => {
        setOpen(false);
    };

    const handleInputChange = (event) => {
        const { name, value } = event.target;
        setCurrentGranja({ ...currentGranja, [name]: value });
    };

    const handleSubmit = async () => {
        try {
            if (currentGranja.id === 0) {
                // Criar nova granja
                await apiService.createGranja({ nome: currentGranja.nome, localizacao: currentGranja.localizacao });
            } else {
                // Atualizar granja existente
                await apiService.updateGranja(currentGranja.id, currentGranja);
            }
            handleClose();
            fetchGranjas(); // Atualiza a lista
        } catch (error) {
            console.error("Houve um erro ao salvar a granja:", error);
        }
    };

    const handleDelete = async (id) => {
        // Pede confirmação antes de excluir
        if (window.confirm('Tem certeza que deseja excluir esta granja?')) {
            try {
                await apiService.deleteGranja(id);
                fetchGranjas(); // Atualiza a lista
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
                <Button variant="contained" onClick={() => handleClickOpen()}>
                    Adicionar Nova Granja
                </Button>
            </Box>

            <Dialog open={open} onClose={handleClose}>
                <DialogTitle>{currentGranja.id === 0 ? 'Cadastrar Nova Granja' : 'Editar Granja'}</DialogTitle>
                <DialogContent>
                    <TextField autoFocus margin="dense" name="nome" label="Nome da Granja" type="text" fullWidth variant="standard" value={currentGranja.nome} onChange={handleInputChange}/>
                    <TextField margin="dense" name="localizacao" label="Localização" type="text" fullWidth variant="standard" value={currentGranja.localizacao} onChange={handleInputChange}/>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose}>Cancelar</Button>
                    <Button onClick={handleSubmit}>Salvar</Button>
                </DialogActions>
            </Dialog>

            <TableContainer component={Paper}>
                <Table>
                    <TableHead style={{ backgroundColor: '#f5f5f5' }}>
                        <TableRow>
                            <TableCell>ID</TableCell>
                            <TableCell>Nome</TableCell>
                            <TableCell>Localização</TableCell>
                            <TableCell align="right">Ações</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {granjas.map((granja) => (
                            <TableRow key={granja.id}>
                                <TableCell>{granja.id}</TableCell>
                                <TableCell>{granja.nome}</TableCell>
                                <TableCell>{granja.localizacao}</TableCell>
                                <TableCell align="right">
                                    <IconButton onClick={() => handleClickOpen(granja)}>
                                        <EditIcon />
                                    </IconButton>
                                    <IconButton onClick={() => handleDelete(granja.id)}>
                                        <DeleteIcon />
                                    </IconButton>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </Box>
    );
}

export default GranjasPage;