    import React, { useState, useEffect, useCallback, useContext } from 'react';
    import { AuthContext } from '../context/AuthContext';
    import apiService from '../services/apiService';
    import { 
        Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
        Typography, Box, Button, Dialog, DialogActions, DialogContent, 
        DialogTitle, TextField, Select, MenuItem, FormControl, InputLabel, IconButton
    } from '@mui/material';
    import EditIcon from '@mui/icons-material/Edit';
    import DeleteIcon from '@mui/icons-material/Delete';

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

        const fetchData = useCallback(async () => {
            try {
                const [estoqueRes, granjasRes] = await Promise.all([
                    apiService.getEstoque(),
                    apiService.getGranjas() // Para popular o dropdown de granjas
                ]);
                setEstoque(estoqueRes.data);
                setGranjas(granjasRes.data);
            } catch (error) {
                console.error("Erro ao buscar dados de estoque:", error);
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
        
        const handleClose = () => setOpen(false);

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
                    const { id, ...produtoFinal } = produtoParaEnviar;
                    await apiService.createProduto(produtoFinal);
                }
                
                handleClose();
                fetchData();
            } catch (error) {
                console.error("Erro ao salvar produto:", error);
                alert("Falha ao salvar produto no estoque.");
            }
        };

        const handleDelete = async (id) => {
            if (window.confirm('Tem certeza que deseja excluir este item do estoque?')) {
                try {
                    await apiService.deleteProduto(id);
                    fetchData();
                } catch (error) {
                    console.error("Erro ao excluir produto:", error);
                    alert("Falha ao excluir produto.");
                }
            }
        };

        return (
            <Box sx={{ margin: '20px', padding: '20px' }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                    <Typography variant="h4" component="h1">
                        Controlo de Estoque
                    </Typography>
                    <Button variant="contained" onClick={() => handleOpen()}>
                        Adicionar Produto
                    </Button>
                </Box>

                <Dialog open={open} onClose={handleClose}>
                    <DialogTitle>{isEditMode ? 'Editar Produto' : 'Adicionar Novo Produto'}</DialogTitle>
                    <DialogContent>
                        <TextField autoFocus margin="dense" name="nome" label="Nome do Produto" type="text" fullWidth variant="standard" value={formData.nome} onChange={handleInputChange} />
                        <TextField margin="dense" name="tipo" label="Tipo (Ração, Vacina, etc.)" type="text" fullWidth variant="standard" value={formData.tipo} onChange={handleInputChange} />
                        <TextField margin="dense" name="quantidade" label="Quantidade" type="number" fullWidth variant="standard" value={formData.quantidade} onChange={handleInputChange} />
                        <TextField margin="dense" name="unidadeDeMedida" label="Unidade (kg, un, litro)" type="text" fullWidth variant="standard" value={formData.unidadeDeMedida} onChange={handleInputChange} />
                        <FormControl fullWidth margin="dense" variant="standard" required>
                            <InputLabel>Granja</InputLabel>
                            <Select name="granjaId" value={formData.granjaId} onChange={handleInputChange}>
                                {granjas.map((granja) => (
                                    <MenuItem key={granja.id} value={granja.id}>{granja.nome} ({granja.codigo})</MenuItem>
                                ))}
                            </Select>
                        </FormControl>
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
                                <TableCell>Nome</TableCell>
                                <TableCell>Tipo</TableCell>
                                <TableCell align="right">Quantidade</TableCell>
                                <TableCell>Unidade</TableCell>
                                <TableCell>Granja</TableCell>
                                {user?.role === 'Administrador' && <TableCell>Dono da Granja</TableCell>}
                                <TableCell align="right">Ações</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {estoque.map((produto) => (
                                <TableRow key={produto.id}>
                                    <TableCell>{produto.nome}</TableCell>
                                    <TableCell>{produto.tipo}</TableCell>
                                    <TableCell align="right">{produto.quantidade}</TableCell>
                                    <TableCell>{produto.unidadeDeMedida}</TableCell>
                                    <TableCell>{produto.granja?.nome || 'N/A'}</TableCell>
                                    {user?.role === 'Administrador' && <TableCell>{produto.granja?.usuario?.nome || 'N/A'}</TableCell>}
                                    <TableCell align="right">
                                        <IconButton onClick={() => handleOpen(produto)}><EditIcon /></IconButton>
                                        <IconButton onClick={() => handleDelete(produto.id)}><DeleteIcon /></IconButton>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Box>
        );
    }

    export default EstoquePage;
