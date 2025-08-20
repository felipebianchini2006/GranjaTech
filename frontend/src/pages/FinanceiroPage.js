import React, { useState, useEffect, useCallback, useContext } from 'react';
import { AuthContext } from '../context/AuthContext';
import apiService from '../services/apiService';
import { 
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    Typography, Box, Button, Dialog, DialogActions, DialogContent, 
    DialogTitle, TextField, Select, MenuItem, FormControl, InputLabel, IconButton, Tooltip
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';

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

    const fetchData = useCallback(async () => {
        try {
            const [transacoesRes, lotesRes] = await Promise.all([
                apiService.getTransacoes(),
                apiService.getLotes()
            ]);
            setTransacoes(transacoesRes.data);
            setLotes(lotesRes.data);
        } catch (error) {
            console.error("Erro ao buscar dados financeiros:", error);
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

    return (
        <Box sx={{ margin: '20px', padding: '20px' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h4" component="h1">
                    Gestão Financeira
                </Typography>
                <Button variant="contained" onClick={() => handleOpen()}>
                    Adicionar Nova Transação
                </Button>
            </Box>

            <Dialog open={open} onClose={handleClose}>
                <DialogTitle>{isEditMode ? 'Editar Transação' : 'Registar Nova Transação'}</DialogTitle>
                <DialogContent>
                    <TextField autoFocus margin="dense" name="descricao" label="Descrição" type="text" fullWidth variant="standard" value={formData.descricao} onChange={handleInputChange} />
                    <TextField margin="dense" name="valor" label="Valor (R$)" type="number" fullWidth variant="standard" value={formData.valor} onChange={handleInputChange} />
                    <FormControl fullWidth margin="dense" variant="standard">
                        <InputLabel>Tipo</InputLabel>
                        <Select name="tipo" value={formData.tipo} onChange={handleInputChange}>
                            <MenuItem value="Entrada">Entrada</MenuItem>
                            <MenuItem value="Saida">Saída</MenuItem>
                        </Select>
                    </FormControl>
                    <TextField margin="dense" name="data" label="Data" type="date" fullWidth variant="standard" InputLabelProps={{ shrink: true }} value={formData.data} onChange={handleInputChange} />
                    <FormControl fullWidth margin="dense" variant="standard">
                        <InputLabel>Lote (Opcional)</InputLabel>
                        <Select name="loteId" value={formData.loteId} onChange={handleInputChange}>
                            <MenuItem value="">Nenhum</MenuItem>
                            {lotes.map((lote) => (
                                <MenuItem key={lote.id} value={lote.id}>{lote.identificador}</MenuItem>
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
                            <TableCell>Descrição</TableCell>
                            <TableCell align="right">Valor</TableCell>
                            <TableCell>Tipo</TableCell>
                            <TableCell>Data</TableCell>
                            <TableCell>Lote Associado</TableCell>
                            <TableCell>Registado por</TableCell>
                            <TableCell align="right">Ações</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {transacoes.map((transacao) => {
                            // A CORREÇÃO ESTÁ AQUI:
                            const canEdit = isEditable(transacao.timestampCriacao) || user?.role === 'Administrador';
                            const tooltipMessage = !canEdit && user?.role !== 'Administrador' 
                                ? "O tempo para edição (5 minutos) expirou" 
                                : "Editar Transação";

                            return (
                                <TableRow key={transacao.id}>
                                    <TableCell>{transacao.descricao}</TableCell>
                                    <TableCell align="right" sx={{ color: transacao.tipo === 'Entrada' ? 'green' : 'red' }}>
                                        {transacao.valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                                    </TableCell>
                                    <TableCell>{transacao.tipo}</TableCell>
                                    <TableCell>{new Date(transacao.data).toLocaleDateString('pt-BR', { timeZone: 'UTC' })}</TableCell>
                                    <TableCell>{transacao.lote?.identificador || '-'}</TableCell>
                                    <TableCell>{transacao.usuario?.nome || 'N/A'}</TableCell>
                                    <TableCell align="right">
                                        <Tooltip title={tooltipMessage}>
                                            <span>
                                                <IconButton onClick={() => handleOpen(transacao)} disabled={!canEdit}>
                                                    <EditIcon />
                                                </IconButton>
                                            </span>
                                        </Tooltip>
                                        {user?.role === 'Administrador' && (
                                            <IconButton onClick={() => handleDelete(transacao.id)}>
                                                <DeleteIcon />
                                            </IconButton>
                                        )}
                                    </TableCell>
                                </TableRow>
                            );
                        })}
                    </TableBody>
                </Table>
            </TableContainer>
        </Box>
    );
}

export default FinanceiroPage;
