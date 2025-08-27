import React, { useState, useEffect, useCallback, useContext } from 'react';
import { AuthContext } from '../context/AuthContext';
import apiService from '../services/apiService';
import PageContainer from '../components/PageContainer';
import LoadingSpinner from '../components/LoadingSpinner';
import { 
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    Typography, Box, Button, Dialog, DialogActions, DialogContent, 
    DialogTitle, TextField, Select, MenuItem, FormControl, InputLabel, IconButton,
    Card, Chip, Alert
} from '@mui/material';
import {
    Edit as EditIcon,
    Delete as DeleteIcon,
    Add as AddIcon,
    Inventory as InventoryIcon,
    Agriculture as AgricultureIcon,
    Category as CategoryIcon,
    Scale as ScaleIcon
} from '@mui/icons-material';

const initialFormState = {
    nome: '', tipo: '', quantidade: '', unidadeDeMedida: '', granjaId: ''
};

function EstoquePage() {
    const { user } = useContext(AuthContext);
    const [estoque, setEstoque] = useState([]);
    const [granjas, setGranjas] = useState([]);
    const [open, setOpen] = useState(false);
    const [isEditMode, setIsEditMode] = useState(false);
    const [formData, setFormData] = useState(initialFormState);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const fetchData = useCallback(async () => {
        try {
            setLoading(true);
            setError('');
            const [estoqueRes, granjasRes] = await Promise.all([
                apiService.getEstoque(),
                apiService.getGranjas()
            ]);
            setEstoque(estoqueRes.data);
            setGranjas(granjasRes.data);
        } catch (error) {
            console.error("Erro ao buscar dados de estoque:", error);
            setError('Erro ao carregar o estoque. Tente novamente.');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    const handleOpen = (produto = null) => {
        if (produto) {
            setIsEditMode(true);
            setFormData(produto);
        } else {
            setIsEditMode(false);
            setFormData(initialFormState);
        }
        setOpen(true);
    };

    const handleClose = () => {
        setOpen(false);
        setFormData(initialFormState);
    };

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setFormData({ ...formData, [name]: value });
    };

    const handleSubmit = async () => {
        try {
            const produtoParaEnviar = {
                ...formData,
                quantidade: parseFloat(formData.quantidade),
                granjaId: parseInt(formData.granjaId, 10)
            };

            if (isEditMode) {
                await apiService.updateProduto(formData.id, produtoParaEnviar);
            } else {
                await apiService.createProduto(produtoParaEnviar);
            }
            
            handleClose();
            fetchData();
        } catch (error) {
            console.error("Erro ao salvar produto:", error);
            alert("Falha ao salvar produto no estoque.");
        }
    };

    const handleDelete = async (id) => {
        if (window.confirm('Tem certeza que deseja excluir este produto?')) {
            try {
                await apiService.deleteProduto(id);
                fetchData();
            } catch (error) {
                console.error("Erro ao excluir produto:", error);
                alert("Falha ao excluir produto.");
            }
        }
    };

    const getTypeColor = (tipo) => {
        switch (tipo?.toLowerCase()) {
            case 'ração': return 'primary';
            case 'medicamento': return 'error';
            case 'vacina': return 'success';
            case 'equipamento': return 'warning';
            default: return 'default';
        }
    };

    if (loading) {
        return <LoadingSpinner message="Carregando estoque..." />;
    }

    return (
        <PageContainer
            title="Estoque"
            subtitle="Gerencie produtos, insumos e equipamentos"
            action={
                <Button 
                    variant="contained" 
                    startIcon={<AddIcon />}
                    onClick={() => handleOpen()}
                    sx={{ 
                        borderRadius: 2,
                        px: 3,
                        py: 1.5,
                    }}
                >
                    Novo Produto
                </Button>
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
                        <InventoryIcon color="primary" />
                        <Typography variant="h6" component="div">
                            {isEditMode ? 'Editar Produto' : 'Novo Produto'}
                        </Typography>
                    </Box>
                </DialogTitle>
                <DialogContent sx={{ pt: 2 }}>
                    <TextField 
                        autoFocus 
                        margin="normal" 
                        name="nome" 
                        label="Nome do Produto" 
                        fullWidth 
                        value={formData.nome} 
                        onChange={handleInputChange}
                        sx={{ mb: 2 }}
                    />
                    <FormControl fullWidth margin="normal" sx={{ mb: 2 }}>
                        <InputLabel>Tipo</InputLabel>
                        <Select name="tipo" value={formData.tipo} onChange={handleInputChange}>
                            <MenuItem value="Ração">Ração</MenuItem>
                            <MenuItem value="Medicamento">Medicamento</MenuItem>
                            <MenuItem value="Vacina">Vacina</MenuItem>
                            <MenuItem value="Equipamento">Equipamento</MenuItem>
                            <MenuItem value="Outros">Outros</MenuItem>
                        </Select>
                    </FormControl>
                    <TextField 
                        margin="normal" 
                        name="quantidade" 
                        label="Quantidade" 
                        type="number" 
                        fullWidth 
                        value={formData.quantidade} 
                        onChange={handleInputChange}
                        sx={{ mb: 2 }}
                    />
                    <TextField 
                        margin="normal" 
                        name="unidadeDeMedida" 
                        label="Unidade de Medida" 
                        fullWidth 
                        value={formData.unidadeDeMedida} 
                        onChange={handleInputChange}
                        sx={{ mb: 2 }}
                    />
                    <FormControl fullWidth margin="normal">
                        <InputLabel>Granja</InputLabel>
                        <Select name="granjaId" value={formData.granjaId} onChange={handleInputChange}>
                            {granjas.map((granja) => (
                                <MenuItem key={granja.id} value={granja.id}>
                                    {granja.nome}
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
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
                                <TableCell sx={{ fontWeight: 600 }}>Nome</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Tipo</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Quantidade</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Unidade</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Granja</TableCell>
                                <TableCell align="right" sx={{ fontWeight: 600 }}>Ações</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {estoque.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={6} align="center" sx={{ py: 8 }}>
                                        <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
                                            <InventoryIcon sx={{ fontSize: 48, color: 'text.secondary' }} />
                                            <Typography color="text.secondary">
                                                Nenhum produto em estoque
                                            </Typography>
                                        </Box>
                                    </TableCell>
                                </TableRow>
                            ) : (
                                estoque.map((produto) => (
                                    <TableRow 
                                        key={produto.id}
                                        sx={{ 
                                            '&:hover': { backgroundColor: 'action.hover' },
                                            transition: 'background-color 0.2s ease-in-out',
                                        }}
                                    >
                                        <TableCell>
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                                                <InventoryIcon color="primary" sx={{ fontSize: 20 }} />
                                                <Typography fontWeight={500}>
                                                    {produto.nome}
                                                </Typography>
                                            </Box>
                                        </TableCell>
                                        <TableCell>
                                            <Chip
                                                icon={<CategoryIcon />}
                                                label={produto.tipo}
                                                color={getTypeColor(produto.tipo)}
                                                variant="outlined"
                                                size="small"
                                            />
                                        </TableCell>
                                        <TableCell>
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                <ScaleIcon color="action" sx={{ fontSize: 16 }} />
                                                <Typography 
                                                    variant="body2"
                                                    fontWeight={produto.quantidade < 10 ? 600 : 400}
                                                    color={produto.quantidade < 10 ? 'error.main' : 'text.primary'}
                                                >
                                                    {produto.quantidade?.toLocaleString() || '0'}
                                                </Typography>
                                            </Box>
                                        </TableCell>
                                        <TableCell>
                                            <Typography variant="body2">
                                                {produto.unidadeDeMedida || 'N/A'}
                                            </Typography>
                                        </TableCell>
                                        <TableCell>
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                <AgricultureIcon color="action" sx={{ fontSize: 16 }} />
                                                <Typography variant="body2">
                                                    {produto.granja?.nome || 'N/A'}
                                                </Typography>
                                            </Box>
                                        </TableCell>
                                        <TableCell align="right">
                                            <IconButton 
                                                onClick={() => handleOpen(produto)}
                                                size="small"
                                                sx={{ mr: 1 }}
                                            >
                                                <EditIcon fontSize="small" />
                                            </IconButton>
                                            <IconButton 
                                                onClick={() => handleDelete(produto.id)}
                                                size="small"
                                                color="error"
                                            >
                                                <DeleteIcon fontSize="small" />
                                            </IconButton>
                                        </TableCell>
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

export default EstoquePage;