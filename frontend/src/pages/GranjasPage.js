import React, { useState, useEffect, useCallback, useContext } from 'react';
import { AuthContext } from '../context/AuthContext';
import apiService from '../services/apiService';
import PageContainer from '../components/PageContainer';
import LoadingSpinner from '../components/LoadingSpinner';
import {
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    Typography, Box, Button, Dialog, DialogActions, DialogContent,
    DialogTitle, TextField, IconButton, Select, MenuItem, FormControl, InputLabel,
    Card, CardContent, Chip, Alert
} from '@mui/material';
import { 
    Edit as EditIcon,
    Delete as DeleteIcon,
    Add as AddIcon,
    Agriculture as AgricultureIcon,
    LocationOn as LocationIcon,
    Person as PersonIcon
} from '@mui/icons-material';

const initialFormState = { id: 0, nome: '', localizacao: '', usuarioId: null };

function GranjasPage() {
    const { user } = useContext(AuthContext);
    const [granjas, setGranjas] = useState([]);
    const [produtores, setProdutores] = useState([]);
    const [open, setOpen] = useState(false);
    const [currentGranja, setCurrentGranja] = useState(initialFormState);
    const [isEditMode, setIsEditMode] = useState(false);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const fetchData = useCallback(async () => {
        try {
            setLoading(true);
            setError('');
            const granjasRes = await apiService.getGranjas();
            setGranjas(granjasRes.data);

            if (user?.role === 'Administrador') {
                const usersRes = await apiService.getUsers();
                setProdutores(usersRes.data.filter(u => u.perfilNome === 'Produtor'));
            }
        } catch (error) {
            console.error("Houve um erro ao buscar os dados:", error);
            setError('Erro ao carregar as granjas. Tente novamente.');
        } finally {
            setLoading(false);
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

    if (loading) {
        return <LoadingSpinner message="Carregando granjas..." />;
    }

    return (
        <PageContainer
            title="Granjas"
            subtitle="Gerencie suas propriedades agropecuárias"
            action={
                user?.role !== 'Financeiro' && (
                    <Button 
                        variant="contained" 
                        startIcon={<AddIcon />}
                        onClick={() => handleClickOpen()}
                        sx={{ 
                            borderRadius: 2,
                            px: 3,
                            py: 1.5,
                        }}
                    >
                        Nova Granja
                    </Button>
                )
            }
        >
            {error && (
                <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError('')}>
                    {error}
                </Alert>
            )}

            <Dialog 
                open={open} 
                onClose={handleClose}
                maxWidth="sm"
                fullWidth
                PaperProps={{
                    sx: { borderRadius: 3 }
                }}
            >
                <DialogTitle sx={{ pb: 2 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                        <AgricultureIcon color="primary" />
                        <Typography variant="h6" component="div">
                            {isEditMode ? 'Editar Granja' : 'Cadastrar Nova Granja'}
                        </Typography>
                    </Box>
                </DialogTitle>
                <DialogContent sx={{ pt: 2 }}>
                    <TextField 
                        autoFocus 
                        margin="normal" 
                        required 
                        fullWidth 
                        name="nome" 
                        label="Nome da Granja" 
                        value={currentGranja.nome} 
                        onChange={handleInputChange}
                        sx={{ mb: 2 }}
                    />
                    <TextField 
                        margin="normal" 
                        fullWidth 
                        name="localizacao" 
                        label="Localização" 
                        value={currentGranja.localizacao || ''} 
                        onChange={handleInputChange}
                        sx={{ mb: 2 }}
                    />
                    
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
                <DialogActions sx={{ p: 3, pt: 2 }}>
                    <Button onClick={handleClose} variant="outlined">
                        Cancelar
                    </Button>
                    <Button onClick={handleSubmit} variant="contained">
                        {isEditMode ? 'Atualizar' : 'Criar'}
                    </Button>
                </DialogActions>
            </Dialog>

            <Card>
                <TableContainer>
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableCell sx={{ fontWeight: 600 }}>Código</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Nome</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Localização</TableCell>
                                {(user?.role === 'Administrador' || user?.role === 'Financeiro') && 
                                    <TableCell sx={{ fontWeight: 600 }}>Dono (Produtor)</TableCell>
                                }
                                {user?.role !== 'Financeiro' && 
                                    <TableCell align="right" sx={{ fontWeight: 600 }}>Ações</TableCell>
                                }
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {granjas.length === 0 ? (
                                <TableRow>
                                    <TableCell 
                                        colSpan={user?.role === 'Financeiro' ? 4 : 5} 
                                        align="center" 
                                        sx={{ py: 8 }}
                                    >
                                        <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
                                            <AgricultureIcon sx={{ fontSize: 48, color: 'text.secondary' }} />
                                            <Typography color="text.secondary">
                                                Nenhuma granja encontrada
                                            </Typography>
                                        </Box>
                                    </TableCell>
                                </TableRow>
                            ) : (
                                granjas.map((granja) => (
                                    <TableRow 
                                        key={granja.id}
                                        sx={{ 
                                            '&:hover': { backgroundColor: 'action.hover' },
                                            transition: 'background-color 0.2s ease-in-out',
                                        }}
                                    >
                                        <TableCell>
                                            <Typography variant="body2" fontWeight={500}>
                                                {granja.codigo}
                                            </Typography>
                                        </TableCell>
                                        <TableCell>
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                                                <AgricultureIcon color="primary" sx={{ fontSize: 20 }} />
                                                <Typography fontWeight={500}>
                                                    {granja.nome}
                                                </Typography>
                                            </Box>
                                        </TableCell>
                                        <TableCell>
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                <LocationIcon color="action" sx={{ fontSize: 16 }} />
                                                <Typography variant="body2">
                                                    {granja.localizacao || 'Não informado'}
                                                </Typography>
                                            </Box>
                                        </TableCell>
                                        {(user?.role === 'Administrador' || user?.role === 'Financeiro') && (
                                            <TableCell>
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                    <PersonIcon color="action" sx={{ fontSize: 16 }} />
                                                    <Typography variant="body2">
                                                        {granja.usuario?.nome || 'N/A'}
                                                    </Typography>
                                                </Box>
                                            </TableCell>
                                        )}
                                        {user?.role !== 'Financeiro' && (
                                            <TableCell align="right">
                                                <IconButton 
                                                    onClick={() => handleClickOpen(granja)}
                                                    size="small"
                                                    sx={{ mr: 1 }}
                                                >
                                                    <EditIcon fontSize="small" />
                                                </IconButton>
                                                <IconButton 
                                                    onClick={() => handleDelete(granja.id)}
                                                    size="small"
                                                    color="error"
                                                >
                                                    <DeleteIcon fontSize="small" />
                                                </IconButton>
                                            </TableCell>
                                        )}
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Card>
        </PageContainer>
    );
}

export default GranjasPage;
