import React, { useState, useEffect, useCallback, useContext } from 'react';
import { AuthContext } from '../context/AuthContext';
import apiService from '../services/apiService';
import PageContainer from '../components/PageContainer';
import LoadingSpinner from '../components/LoadingSpinner';
import { 
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    Typography, Box, Button, Dialog, DialogActions, DialogContent, 
    DialogTitle, TextField, Select, MenuItem, FormControl, InputLabel, IconButton, Tooltip,
    Card, Chip, Alert
} from '@mui/material';
import {
    Edit as EditIcon,
    Delete as DeleteIcon,
    Add as AddIcon,
    AttachMoney as MoneyIcon,
    TrendingUp as TrendingUpIcon,
    TrendingDown as TrendingDownIcon,
    CalendarToday as CalendarIcon,
    Person as PersonIcon,
    Agriculture as AgricultureIcon
} from '@mui/icons-material';

const initialFormState = {
    descricao: '', valor: '', tipo: 'Saida',
    data: new Date().toISOString().split('T')[0], loteId: ''
};

function FinanceiroPage() {
    const { user } = useContext(AuthContext);
    const [transacoes, setTransacoes] = useState([]);
    const [lotes, setLotes] = useState([]);
    const [open, setOpen] = useState(false);
    const [isEditMode, setIsEditMode] = useState(false);
    const [formData, setFormData] = useState(initialFormState);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const fetchData = useCallback(async () => {
        try {
            setLoading(true);
            setError('');
            const [transacoesRes, lotesRes] = await Promise.all([
                apiService.getTransacoes(),
                apiService.getLotes()
            ]);
            setTransacoes(transacoesRes.data);
            setLotes(lotesRes.data);
        } catch (error) {
            console.error("Erro ao buscar dados financeiros:", error);
            setError('Erro ao carregar dados financeiros. Tente novamente.');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    const handleOpen = (transacao = null) => {
        if (transacao) {
            setIsEditMode(true);
            setFormData({
                id: transacao.id,
                descricao: transacao.descricao,
                valor: transacao.valor,
                tipo: transacao.tipo,
                data: new Date(transacao.data).toISOString().split('T')[0],
                loteId: transacao.loteId || ''
            });
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
            const transacaoParaEnviar = {
                ...formData,
                valor: parseFloat(formData.valor),
                data: new Date(`${formData.data}T00:00:00.000Z`).toISOString(),
                loteId: formData.loteId ? parseInt(formData.loteId, 10) : null
            };

            if (isEditMode) {
                await apiService.updateTransacao(formData.id, transacaoParaEnviar);
            } else {
                const { id, ...transacaoFinal } = transacaoParaEnviar;
                await apiService.createTransacao(transacaoFinal);
            }
            
            handleClose();
            fetchData();
        } catch (error) {
            console.error("Erro ao salvar transação:", error);
            alert(error.response?.data?.message || "Falha ao registar transação.");
        }
    };

    const handleDelete = async (id) => {
        if (window.confirm('Tem certeza que deseja excluir esta transação?')) {
            try {
                await apiService.deleteTransacao(id);
                fetchData();
            } catch (error) {
                console.error("Erro ao excluir transação:", error);
                alert("Falha ao excluir transação.");
            }
        }
    };

    const isEditable = (timestamp) => {
        const FIVE_MINUTES = 5 * 60 * 1000;
        const transactionTime = new Date(timestamp).getTime();
        const currentTime = new Date().getTime();
        return (currentTime - transactionTime) < FIVE_MINUTES;
    };

    if (loading) {
        return <LoadingSpinner message="Carregando dados financeiros..." />;
    }

    return (
        <PageContainer
            title="Financeiro"
            subtitle="Gerencie entradas e saídas financeiras"
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
                    Nova Transação
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
                        <MoneyIcon color="primary" />
                        <Typography variant="h6" component="div">
                            {isEditMode ? 'Editar Transação' : 'Nova Transação'}
                        </Typography>
                    </Box>
                </DialogTitle>
                <DialogContent sx={{ pt: 2 }}>
                    <TextField 
                        autoFocus 
                        margin="normal" 
                        name="descricao" 
                        label="Descrição" 
                        type="text" 
                        fullWidth 
                        value={formData.descricao} 
                        onChange={handleInputChange}
                        sx={{ mb: 2 }}
                    />
                    <TextField 
                        margin="normal" 
                        name="valor" 
                        label="Valor (R$)" 
                        type="number" 
                        fullWidth 
                        value={formData.valor} 
                        onChange={handleInputChange}
                        sx={{ mb: 2 }}
                    />
                    <FormControl fullWidth margin="normal" sx={{ mb: 2 }}>
                        <InputLabel>Tipo</InputLabel>
                        <Select name="tipo" value={formData.tipo} onChange={handleInputChange}>
                            <MenuItem value="Entrada">Entrada</MenuItem>
                            <MenuItem value="Saida">Saída</MenuItem>
                        </Select>
                    </FormControl>
                    <TextField 
                        margin="normal" 
                        name="data" 
                        label="Data" 
                        type="date" 
                        fullWidth 
                        InputLabelProps={{ shrink: true }} 
                        value={formData.data} 
                        onChange={handleInputChange}
                        sx={{ mb: 2 }}
                    />
                    <FormControl fullWidth margin="normal">
                        <InputLabel>Lote (Opcional)</InputLabel>
                        <Select name="loteId" value={formData.loteId} onChange={handleInputChange}>
                            <MenuItem value="">Nenhum</MenuItem>
                            {lotes.map((lote) => (
                                <MenuItem key={lote.id} value={lote.id}>{lote.identificador}</MenuItem>
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
                                <TableCell sx={{ fontWeight: 600 }}>Descrição</TableCell>
                                <TableCell align="right" sx={{ fontWeight: 600 }}>Valor</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Tipo</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Data</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Lote Associado</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Registado por</TableCell>
                                <TableCell align="right" sx={{ fontWeight: 600 }}>Ações</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {transacoes.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={7} align="center" sx={{ py: 8 }}>
                                        <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
                                            <MoneyIcon sx={{ fontSize: 48, color: 'text.secondary' }} />
                                            <Typography color="text.secondary">
                                                Nenhuma transação encontrada
                                            </Typography>
                                        </Box>
                                    </TableCell>
                                </TableRow>
                            ) : (
                                transacoes.map((transacao) => {
                                    const canEdit = isEditable(transacao.timestampCriacao) || user?.role === 'Administrador';
                                    const tooltipMessage = !canEdit && user?.role !== 'Administrador' 
                                        ? "O tempo para edição (5 minutos) expirou" 
                                        : "Editar Transação";

                                    return (
                                        <TableRow 
                                            key={transacao.id}
                                            sx={{ 
                                                '&:hover': { backgroundColor: 'action.hover' },
                                                transition: 'background-color 0.2s ease-in-out',
                                            }}
                                        >
                                            <TableCell>
                                                <Typography fontWeight={500}>
                                                    {transacao.descricao}
                                                </Typography>
                                            </TableCell>
                                            <TableCell align="right">
                                                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end', gap: 1 }}>
                                                    {transacao.tipo === 'Entrada' ? 
                                                        <TrendingUpIcon sx={{ color: 'success.main', fontSize: 16 }} /> :
                                                        <TrendingDownIcon sx={{ color: 'error.main', fontSize: 16 }} />
                                                    }
                                                    <Typography 
                                                        fontWeight={600}
                                                        color={transacao.tipo === 'Entrada' ? 'success.main' : 'error.main'}
                                                    >
                                                        {transacao.valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                                                    </Typography>
                                                </Box>
                                            </TableCell>
                                            <TableCell>
                                                <Chip 
                                                    label={transacao.tipo}
                                                    color={transacao.tipo === 'Entrada' ? 'success' : 'error'}
                                                    variant="outlined"
                                                    size="small"
                                                />
                                            </TableCell>
                                            <TableCell>
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                    <CalendarIcon color="action" sx={{ fontSize: 16 }} />
                                                    <Typography variant="body2">
                                                        {new Date(transacao.data).toLocaleDateString('pt-BR', { timeZone: 'UTC' })}
                                                    </Typography>
                                                </Box>
                                            </TableCell>
                                            <TableCell>
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                    <AgricultureIcon color="action" sx={{ fontSize: 16 }} />
                                                    <Typography variant="body2">
                                                        {transacao.lote?.identificador || 'Sem lote'}
                                                    </Typography>
                                                </Box>
                                            </TableCell>
                                            <TableCell>
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                    <PersonIcon color="action" sx={{ fontSize: 16 }} />
                                                    <Typography variant="body2">
                                                        {transacao.usuario?.nome || 'N/A'}
                                                    </Typography>
                                                </Box>
                                            </TableCell>
                                            <TableCell align="right">
                                                <Tooltip title={tooltipMessage}>
                                                    <span>
                                                        <IconButton 
                                                            onClick={() => handleOpen(transacao)} 
                                                            disabled={!canEdit}
                                                            size="small"
                                                            sx={{ mr: 1 }}
                                                        >
                                                            <EditIcon fontSize="small" />
                                                        </IconButton>
                                                    </span>
                                                </Tooltip>
                                                {user?.role === 'Administrador' && (
                                                    <IconButton 
                                                        onClick={() => handleDelete(transacao.id)}
                                                        size="small"
                                                        color="error"
                                                    >
                                                        <DeleteIcon fontSize="small" />
                                                    </IconButton>
                                                )}
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

export default FinanceiroPage;
