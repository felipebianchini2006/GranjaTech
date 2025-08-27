import React, { useState, useEffect, useCallback, useContext } from 'react';
import { AuthContext } from '../context/AuthContext';
import apiService from '../services/apiService';
import PageContainer from '../components/PageContainer';
import LoadingSpinner from '../components/LoadingSpinner';
import { 
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    Typography, Box, Button, Dialog, DialogActions, DialogContent, 
    DialogTitle, TextField, IconButton, Select, MenuItem, FormControl, InputLabel,
    Card, Chip, Alert
} from '@mui/material';
import {
    Edit as EditIcon,
    Delete as DeleteIcon,
    Add as AddIcon,
    Pets as PetsIcon,
    Agriculture as AgricultureIcon,
    CalendarToday as CalendarIcon,
    Numbers as NumbersIcon,
    CheckCircle as CheckCircleIcon,
    Schedule as ScheduleIcon
} from '@mui/icons-material';

const initialLoteState = { 
    id: 0, 
    identificador: '', 
    quantidadeAvesInicial: '', 
    dataEntrada: new Date().toISOString().split('T')[0], 
    dataSaida: null, 
    granjaId: '' 
};

function LotesPage() {
    const { user } = useContext(AuthContext);
    const [lotes, setLotes] = useState([]);
    const [granjas, setGranjas] = useState([]);
    const [open, setOpen] = useState(false);
    const [isEditMode, setIsEditMode] = useState(false);
    const [currentLote, setCurrentLote] = useState(initialLoteState);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const fetchData = useCallback(async () => {
        try {
            setLoading(true);
            setError('');
            const [lotesResponse, granjasResponse] = await Promise.all([
                apiService.getLotes(),
                apiService.getGranjas()
            ]);
            setLotes(lotesResponse.data);
            setGranjas(granjasResponse.data);
        } catch (error) {
            console.error("Houve um erro ao buscar os dados:", error);
            setError('Erro ao carregar os lotes. Tente novamente.');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    const handleClickOpen = (lote = null) => {
        if (lote) {
            setIsEditMode(true);
            const formattedLote = {
                ...lote,
                dataEntrada: lote.dataEntrada ? lote.dataEntrada.split('T')[0] : '',
                dataSaida: lote.dataSaida ? lote.dataSaida.split('T')[0] : null
            };
            setCurrentLote(formattedLote);
        } else {
            setIsEditMode(false);
            setCurrentLote(initialLoteState);
        }
        setOpen(true);
    };

    const handleClose = () => {
        setOpen(false);
        setCurrentLote(initialLoteState);
        setIsEditMode(false);
    };

    const handleInputChange = (event) => {
        const { name, value } = event.target;
        setCurrentLote({ ...currentLote, [name]: value === '' && name === 'dataSaida' ? null : value });
    };

    const handleSubmit = async (event) => {
        event.preventDefault();
        try {
            const loteData = {
                ...currentLote,
                quantidadeAvesInicial: parseInt(currentLote.quantidadeAvesInicial, 10),
                granjaId: parseInt(currentLote.granjaId, 10),
                dataEntrada: new Date(`${currentLote.dataEntrada}T00:00:00.000Z`).toISOString(),
                dataSaida: currentLote.dataSaida ? new Date(`${currentLote.dataSaida}T00:00:00.000Z`).toISOString() : null
            };

            if (isEditMode) {
                await apiService.updateLote(currentLote.id, loteData);
            } else {
                await apiService.createLote(loteData);
            }
            handleClose();
            fetchData();
        } catch (error) {
            console.error("Houve um erro ao salvar o lote:", error);
            alert("Ocorreu um erro ao salvar o lote.");
        }
    };

    const handleDelete = async (id) => {
        if (window.confirm('Tem certeza que deseja excluir este lote?')) {
            try {
                await apiService.deleteLote(id);
                fetchData();
            } catch (error) {
                console.error("Houve um erro ao excluir o lote:", error);
                alert("Ocorreu um erro ao excluir o lote.");
            }
        }
    };

    if (loading) {
        return <LoadingSpinner message="Carregando lotes..." />;
    }

    return (
        <PageContainer
            title="Lotes"
            subtitle="Gerencie os lotes de produção"
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
                        Novo Lote
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
                <form onSubmit={handleSubmit}>
                    <DialogTitle sx={{ pb: 2 }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                            <PetsIcon color="primary" />
                            <Typography variant="h6" component="div">
                                {isEditMode ? 'Editar Lote' : 'Novo Lote'}
                            </Typography>
                        </Box>
                    </DialogTitle>
                    <DialogContent sx={{ pt: 2 }}>
                        <TextField 
                            autoFocus 
                            margin="normal" 
                            name="identificador" 
                            label="Identificador do Lote" 
                            type="text" 
                            fullWidth 
                            value={currentLote.identificador} 
                            onChange={handleInputChange}
                            sx={{ mb: 2 }}
                        />
                        <TextField 
                            margin="normal" 
                            name="quantidadeAvesInicial" 
                            label="Quantidade de Aves" 
                            type="number" 
                            fullWidth 
                            value={currentLote.quantidadeAvesInicial} 
                            onChange={handleInputChange}
                            sx={{ mb: 2 }}
                        />
                        <TextField 
                            margin="normal" 
                            name="dataEntrada" 
                            label="Data de Entrada" 
                            type="date" 
                            fullWidth 
                            InputLabelProps={{ shrink: true }} 
                            value={currentLote.dataEntrada} 
                            onChange={handleInputChange}
                            sx={{ mb: 2 }}
                        />
                        <TextField 
                            margin="normal" 
                            name="dataSaida" 
                            label="Data de Saída (opcional)" 
                            type="date" 
                            fullWidth 
                            InputLabelProps={{ shrink: true }} 
                            value={currentLote.dataSaida || ''} 
                            onChange={handleInputChange}
                            sx={{ mb: 2 }}
                        />
                        <FormControl fullWidth margin="normal">
                            <InputLabel id="granja-select-label">Granja</InputLabel>
                            <Select 
                                labelId="granja-select-label" 
                                name="granjaId" 
                                value={currentLote.granjaId} 
                                onChange={handleInputChange}
                            >
                                {granjas.map((granja) => (
                                    <MenuItem key={granja.id} value={granja.id}>
                                        {granja.nome} ({granja.codigo})
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                    </DialogContent>
                    <DialogActions sx={{ p: 3, pt: 2 }}>
                        <Button onClick={handleClose} variant="outlined">
                            Cancelar
                        </Button>
                        <Button type="submit" variant="contained">
                            {isEditMode ? 'Atualizar' : 'Criar'}
                        </Button>
                    </DialogActions>
                </form>
            </Dialog>

            <Card>
                <TableContainer>
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableCell sx={{ fontWeight: 600 }}>Código</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Identificador</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Qtd. Aves</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Data Entrada</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Data Saída</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Granja</TableCell>
                                {user?.role !== 'Financeiro' && (
                                    <TableCell align="right" sx={{ fontWeight: 600 }}>Ações</TableCell>
                                )}
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {lotes.length === 0 ? (
                                <TableRow>
                                    <TableCell 
                                        colSpan={user?.role === 'Financeiro' ? 7 : 8} 
                                        align="center" 
                                        sx={{ py: 8 }}
                                    >
                                        <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
                                            <PetsIcon sx={{ fontSize: 48, color: 'text.secondary' }} />
                                            <Typography color="text.secondary">
                                                Nenhum lote encontrado
                                            </Typography>
                                        </Box>
                                    </TableCell>
                                </TableRow>
                            ) : (
                                lotes.map((lote) => {
                                    const isActive = !lote.dataSaida;
                                    return (
                                        <TableRow 
                                            key={lote.id}
                                            sx={{ 
                                                '&:hover': { backgroundColor: 'action.hover' },
                                                transition: 'background-color 0.2s ease-in-out',
                                            }}
                                        >
                                            <TableCell>
                                                <Typography variant="body2" fontWeight={500}>
                                                    {lote.codigo}
                                                </Typography>
                                            </TableCell>
                                            <TableCell>
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                                                    <PetsIcon color="primary" sx={{ fontSize: 20 }} />
                                                    <Typography fontWeight={500}>
                                                        {lote.identificador}
                                                    </Typography>
                                                </Box>
                                            </TableCell>
                                            <TableCell>
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                    <NumbersIcon color="action" sx={{ fontSize: 16 }} />
                                                    <Typography variant="body2">
                                                        {lote.quantidadeAvesInicial?.toLocaleString() || 'N/A'}
                                                    </Typography>
                                                </Box>
                                            </TableCell>
                                            <TableCell>
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                    <CalendarIcon color="action" sx={{ fontSize: 16 }} />
                                                    <Typography variant="body2">
                                                        {lote.dataEntrada ? new Date(lote.dataEntrada).toLocaleDateString('pt-BR', { timeZone: 'UTC' }) : 'N/A'}
                                                    </Typography>
                                                </Box>
                                            </TableCell>
                                            <TableCell>
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                    <CalendarIcon color="action" sx={{ fontSize: 16 }} />
                                                    <Typography variant="body2">
                                                        {lote.dataSaida ? new Date(lote.dataSaida).toLocaleDateString('pt-BR', { timeZone: 'UTC' }) : 'Em produção'}
                                                    </Typography>
                                                </Box>
                                            </TableCell>
                                            <TableCell>
                                                <Chip
                                                    icon={isActive ? <ScheduleIcon /> : <CheckCircleIcon />}
                                                    label={isActive ? 'Ativo' : 'Finalizado'}
                                                    color={isActive ? 'warning' : 'success'}
                                                    variant="outlined"
                                                    size="small"
                                                />
                                            </TableCell>
                                            <TableCell>
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                    <AgricultureIcon color="action" sx={{ fontSize: 16 }} />
                                                    <Typography variant="body2">
                                                        {lote.granja?.nome || 'N/A'}
                                                    </Typography>
                                                </Box>
                                            </TableCell>
                                            {user?.role !== 'Financeiro' && (
                                                <TableCell align="right">
                                                    <IconButton 
                                                        onClick={() => handleClickOpen(lote)}
                                                        size="small"
                                                        sx={{ mr: 1 }}
                                                    >
                                                        <EditIcon fontSize="small" />
                                                    </IconButton>
                                                    <IconButton 
                                                        onClick={() => handleDelete(lote.id)}
                                                        size="small"
                                                        color="error"
                                                    >
                                                        <DeleteIcon fontSize="small" />
                                                    </IconButton>
                                                </TableCell>
                                            )}
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

export default LotesPage;