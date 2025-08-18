import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';

import { 
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    Typography, Box, Button, Dialog, DialogActions, DialogContent, 
    DialogTitle, TextField, Select, MenuItem, FormControl, InputLabel
} from '@mui/material';

const initialFormState = {
    descricao: '',
    valor: '',
    tipo: 'Saida', // Valor padrão
    data: new Date().toISOString().split('T')[0],
    loteId: '' // Começa vazio
};

function FinanceiroPage() {
    const [transacoes, setTransacoes] = useState([]);
    const [lotes, setLotes] = useState([]);
    const [open, setOpen] = useState(false);
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

    const handleOpen = () => {
        setFormData(initialFormState);
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
            await apiService.createTransacao(transacaoParaEnviar);
            handleClose();
            fetchData();
        } catch (error) {
            console.error("Erro ao criar transação:", error);
            alert("Falha ao registrar transação.");
        }
    };

    return (
        <Box sx={{ margin: '20px', padding: '20px' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h4" component="h1">
                    Gestão Financeira
                </Typography>
                <Button variant="contained" onClick={handleOpen}>
                    Adicionar Nova Transação
                </Button>
            </Box>

            {/* Formulário em Dialog */}
            <Dialog open={open} onClose={handleClose}>
                <DialogTitle>Registrar Nova Transação</DialogTitle>
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

            {/* Tabela de Transações */}
            <TableContainer component={Paper}>
                <Table>
                    <TableHead style={{ backgroundColor: '#f5f5f5' }}>
                        <TableRow>
                            <TableCell>Descrição</TableCell>
                            <TableCell align="right">Valor</TableCell>
                            <TableCell>Tipo</TableCell>
                            <TableCell>Data</TableCell>
                            <TableCell>Lote Associado</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {transacoes.map((transacao) => (
                            <TableRow key={transacao.id}>
                                <TableCell>{transacao.descricao}</TableCell>
                                <TableCell align="right" sx={{ color: transacao.tipo === 'Entrada' ? 'green' : 'red' }}>
                                    {transacao.valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                                </TableCell>
                                <TableCell>{transacao.tipo}</TableCell>
                                <TableCell>{new Date(transacao.data).toLocaleDateString('pt-BR', { timeZone: 'UTC' })}</TableCell>
                                <TableCell>{transacao.lote?.identificador || '-'}</TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </Box>
    );
}

export default FinanceiroPage;