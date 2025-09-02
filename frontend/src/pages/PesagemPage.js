import React, { useState, useEffect, useCallback } from 'react';
import PageContainer from '../components/PageContainer';
import LoadingSpinner from '../components/LoadingSpinner';
import { 
    Grid, Card, CardContent, Typography, Box, Alert, Button, 
    Select, MenuItem, FormControl, InputLabel, Tooltip, 
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    Dialog, DialogActions, DialogContent, DialogTitle, TextField, IconButton
} from '@mui/material';
import {
    Add as AddIcon,
    Scale as ScaleIcon,
    TrendingUp as TrendingUpIcon,
    Analytics as AnalyticsIcon,
    Assignment as AssignmentIcon,
    Refresh as RefreshIcon,
    Info as InfoIcon
} from '@mui/icons-material';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, Legend, ResponsiveContainer, BarChart, Bar } from 'recharts';
import apiService from '../services/apiService';

function PesagemPage() {
    const [lotes, setLotes] = useState([]);
    const [selectedLote, setSelectedLote] = useState(null);
    const [pesagens, setPesagens] = useState([]);
    const [loading, setLoading] = useState(true);
    const [loadingData, setLoadingData] = useState(false);
    const [error, setError] = useState('');
    const [open, setOpen] = useState(false);
    
    const [formData, setFormData] = useState({
        dataPesagem: new Date().toLocaleDateString('en-CA'),
        idadeDias: '',
        semanaVida: '',
        pesoMedioGramas: '',
        quantidadeAmostrada: 50,
        pesoMinimo: '',
        pesoMaximo: '',
        desvioPadrao: '',
        coeficienteVariacao: '',
        ganhoSemanal: '',
        observacoes: ''
    });

    const fetchLotes = useCallback(async () => {
        try {
            setLoading(true);
            const response = await apiService.getLotes();
            const lotesAtivos = response.data.filter(lote => lote.status === 'Ativo');
            setLotes(lotesAtivos);
            
            if (lotesAtivos.length > 0 && !selectedLote) {
                setSelectedLote(lotesAtivos[0]);
            }
        } catch (error) {
            console.error('Erro ao buscar lotes:', error);
            setError('Erro ao carregar lotes ativos');
        } finally {
            setLoading(false);
        }
    }, [selectedLote]);

    const fetchPesagens = useCallback(async (loteId) => {
        if (!loteId) return;
        
        try {
            setLoadingData(true);
            setError('');
            
            const response = await apiService.get(`/pesagem/${loteId}`);
            setPesagens(response.data || []);
            
        } catch (error) {
            console.error('Erro ao buscar pesagens:', error);
            setError('Erro ao carregar pesagens');
        } finally {
            setLoadingData(false);
        }
    }, []);

    useEffect(() => {
        fetchLotes();
    }, [fetchLotes]);

    useEffect(() => {
        if (selectedLote) {
            fetchPesagens(selectedLote.id);
            
            // Calcular semana e idade baseado na data atual e data de entrada
            const hoje = new Date();
            const dataEntrada = new Date(selectedLote.dataEntrada);
            const idadeDias = Math.floor((hoje - dataEntrada) / (1000 * 60 * 60 * 24));
            const semanaVida = Math.ceil(idadeDias / 7);
            
            setFormData(prev => ({
                ...prev,
                idadeDias: idadeDias.toString(),
                semanaVida: semanaVida.toString()
            }));
        }
    }, [selectedLote, fetchPesagens]);

    const handleLoteChange = (event) => {
        const loteId = event.target.value;
        const lote = lotes.find(l => l.id === loteId);
        setSelectedLote(lote);
    };

    const handleSubmit = async () => {
        try {
            await apiService.post('/pesagem', {
                loteId: selectedLote.id,
                dataPesagem: new Date(formData.dataPesagem).toISOString(),               // Z
                idadeDias: Number(formData.idadeDias),
                semanaVida: Number(formData.semanaVida),
                pesoMedioGramas: Number(formData.pesoMedioGramas),
                quantidadeAmostrada: Number(formData.quantidadeAmostrada),
                pesoMinimo: formData.pesoMinimo !== '' ? Number(formData.pesoMinimo) : null,
                pesoMaximo: formData.pesoMaximo !== '' ? Number(formData.pesoMaximo) : null,
                desvioPadrao: formData.desvioPadrao !== '' ? Number(formData.desvioPadrao) : null,
                coeficienteVariacao: formData.coeficienteVariacao !== '' ? Number(formData.coeficienteVariacao) : null,
                ganhoSemanal: formData.ganhoSemanal !== '' ? Number(formData.ganhoSemanal) : null,
                observacoes: formData.observacoes || null
            });

            setOpen(false);
            setFormData({
                dataPesagem: new Date().toLocaleDateString('en-CA'),
                idadeDias: '',
                semanaVida: '',
                pesoMedioGramas: '',
                quantidadeAmostrada: 50,
                pesoMinimo: '',
                pesoMaximo: '',
                desvioPadrao: '',
                coeficienteVariacao: '',
                ganhoSemanal: '',
                observacoes: ''
            });
            fetchPesagens(selectedLote.id);
        } catch (error) {
            console.error('Erro ao registrar pesagem:', error);
            setError('Erro ao registrar pesagem');
        }
    };

    const calcularGMD = () => {
        if (pesagens.length < 2) return 0;
        
        const pesagensOrdenadas = [...pesagens].sort((a, b) => a.semanaVida - b.semanaVida);
        let totalGmd = 0;
        let semanas = 0;
        
        for (let i = 1; i < pesagensOrdenadas.length; i++) {
            const ganhoPeso = pesagensOrdenadas[i].pesoMedioGramas - pesagensOrdenadas[i - 1].pesoMedioGramas;
            const diasEntrePesagens = 7; // Assumindo pesagens semanais
            const gmd = ganhoPeso / diasEntrePesagens;
            totalGmd += gmd;
            semanas++;
        }
        
        return semanas > 0 ? totalGmd / semanas : 0;
    };

    const prepareChartData = () => {
        return pesagens
            .sort((a, b) => a.semanaVida - b.semanaVida)
            .map(pesagem => ({
                semana: pesagem.semanaVida,
                peso: pesagem.pesoMedioGramas,
                ganho: pesagem.ganhoSemanal || 0,
                uniformidade: 100 - (pesagem.coeficienteVariacao || 0)
            }));
    };

    if (loading) {
        return (
            <PageContainer title="Pesagens Semanais" subtitle="Controle de crescimento e uniformidade do lote">
                <LoadingSpinner />
            </PageContainer>
        );
    }

    const chartData = prepareChartData();
    const gmdMedio = calcularGMD();

    return (
        <PageContainer 
            title="Pesagens Semanais" 
            subtitle="Monitoramento de crescimento e análise de uniformidade"
            action={
                <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
                    <FormControl size="small" sx={{ minWidth: 200 }}>
                        <InputLabel>Selecionar Lote</InputLabel>
                        <Select
                            value={selectedLote?.id || ''}
                            onChange={handleLoteChange}
                            label="Selecionar Lote"
                        >
                            {lotes.map((lote) => (
                                <MenuItem key={lote.id} value={lote.id}>
                                    {lote.identificador} - {lote.idadeAtualDias} dias
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <IconButton 
                        onClick={() => fetchPesagens(selectedLote?.id)} 
                        disabled={loadingData}
                        color="primary"
                    >
                        <RefreshIcon />
                    </IconButton>
                </Box>
            }
        >
            {error && (
                <Alert severity="error" sx={{ mb: 3 }}>
                    {error}
                </Alert>
            )}

            {!selectedLote ? (
                <Alert severity="info">
                    Nenhum lote ativo encontrado. Crie um lote para registrar pesagens.
                </Alert>
            ) : (
                <>
                    {/* KPIs de Crescimento */}
                    <Grid container spacing={3} sx={{ mb: 3 }}>
                        <Grid item xs={12} md={3}>
                            <Card>
                                <CardContent>
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                                        <ScaleIcon color="primary" />
                                        <Typography variant="h6">Peso Atual</Typography>
                                    </Box>
                                    <Typography variant="h4" color="primary">
                                        {pesagens.length > 0 ? 
                                            `${Math.max(...pesagens.map(p => p.pesoMedioGramas)).toFixed(0)}g` 
                                            : 'N/A'
                                        }
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        Peso médio mais recente
                                    </Typography>
                                </CardContent>
                            </Card>
                        </Grid>
                        
                        <Grid item xs={12} md={3}>
                            <Card>
                                <CardContent>
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                                        <TrendingUpIcon color="success" />
                                        <Typography variant="h6">GMD Médio</Typography>
                                    </Box>
                                    <Typography variant="h4" color="success.main">
                                        {gmdMedio.toFixed(1)}g
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        Ganho médio diário
                                    </Typography>
                                </CardContent>
                            </Card>
                        </Grid>
                        
                        <Grid item xs={12} md={3}>
                            <Card>
                                <CardContent>
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                                        <AnalyticsIcon color="info" />
                                        <Typography variant="h6">Uniformidade</Typography>
                                    </Box>
                                    <Typography variant="h4" color="info.main">
                                        {pesagens.length > 0 && pesagens[pesagens.length - 1].coeficienteVariacao ?
                                            `${(100 - pesagens[pesagens.length - 1].coeficienteVariacao).toFixed(1)}%`
                                            : 'N/A'
                                        }
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        Uniformidade do lote
                                    </Typography>
                                </CardContent>
                            </Card>
                        </Grid>
                        
                        <Grid item xs={12} md={3}>
                            <Card>
                                <CardContent>
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                                        <AssignmentIcon color="warning" />
                                        <Typography variant="h6">Pesagens</Typography>
                                    </Box>
                                    <Typography variant="h4" color="warning.main">
                                        {pesagens.length}
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        Total de pesagens
                                    </Typography>
                                </CardContent>
                            </Card>
                        </Grid>
                    </Grid>

                    {/* Gráficos */}
                    {chartData.length > 0 && (
                        <Grid container spacing={3} sx={{ mb: 3 }}>
                            <Grid item xs={12} md={6}>
                                <Card>
                                    <CardContent>
                                        <Typography variant="h6" gutterBottom>
                                            Curva de Crescimento
                                        </Typography>
                                        <Box sx={{ width: '100%', height: 300 }}>
                                            <ResponsiveContainer>
                                                <LineChart data={chartData}>
                                                    <CartesianGrid strokeDasharray="3 3" />
                                                    <XAxis dataKey="semana" label={{ value: 'Semana', position: 'insideBottom', offset: -5 }} />
                                                    <YAxis label={{ value: 'Peso (g)', angle: -90, position: 'insideLeft' }} />
                                                    <RechartsTooltip 
                                                        labelFormatter={(value) => `Semana ${value}`}
                                                        formatter={(value) => [`${value}g`, 'Peso Médio']}
                                                    />
                                                    <Line 
                                                        type="monotone" 
                                                        dataKey="peso" 
                                                        stroke="#1976d2" 
                                                        strokeWidth={3}
                                                        dot={{ r: 6 }}
                                                    />
                                                </LineChart>
                                            </ResponsiveContainer>
                                        </Box>
                                    </CardContent>
                                </Card>
                            </Grid>
                            
                            <Grid item xs={12} md={6}>
                                <Card>
                                    <CardContent>
                                        <Typography variant="h6" gutterBottom>
                                            Ganho de Peso Semanal
                                        </Typography>
                                        <Box sx={{ width: '100%', height: 300 }}>
                                            <ResponsiveContainer>
                                                <BarChart data={chartData}>
                                                    <CartesianGrid strokeDasharray="3 3" />
                                                    <XAxis dataKey="semana" label={{ value: 'Semana', position: 'insideBottom', offset: -5 }} />
                                                    <YAxis label={{ value: 'Ganho (g)', angle: -90, position: 'insideLeft' }} />
                                                    <RechartsTooltip 
                                                        labelFormatter={(value) => `Semana ${value}`}
                                                        formatter={(value) => [`${value}g`, 'Ganho Semanal']}
                                                    />
                                                    <Bar dataKey="ganho" fill="#4caf50" />
                                                </BarChart>
                                            </ResponsiveContainer>
                                        </Box>
                                    </CardContent>
                                </Card>
                            </Grid>
                        </Grid>
                    )}

                    {/* Tabela e Botão */}
                    <Card>
                        <CardContent>
                            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                                <Typography variant="h6">Histórico de Pesagens</Typography>
                                <Button
                                    variant="contained"
                                    startIcon={<AddIcon />}
                                    onClick={() => setOpen(true)}
                                >
                                    Nova Pesagem
                                </Button>
                            </Box>

                            {loadingData ? (
                                <LoadingSpinner />
                            ) : (
                                <TableContainer component={Paper} variant="outlined">
                                    <Table>
                                        <TableHead>
                                            <TableRow>
                                                <TableCell>Semana</TableCell>
                                                <TableCell>Data</TableCell>
                                                <TableCell>Idade (dias)</TableCell>
                                                <TableCell>Peso Médio (g)</TableCell>
                                                <TableCell>Amostra</TableCell>
                                                <TableCell>Min/Max (g)</TableCell>
                                                <TableCell>
                                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                        Uniformidade (%)
                                                        <Tooltip title="Uniformidade = 100% - Coeficiente de Variação">
                                                            <InfoIcon fontSize="small" />
                                                        </Tooltip>
                                                    </Box>
                                                </TableCell>
                                                <TableCell>Ganho Semanal (g)</TableCell>
                                                <TableCell>Observações</TableCell>
                                            </TableRow>
                                        </TableHead>
                                        <TableBody>
                                            {pesagens.length === 0 ? (
                                                <TableRow>
                                                    <TableCell colSpan={9} align="center">
                                                        <Typography color="text.secondary">
                                                            Nenhuma pesagem registrada para este lote
                                                        </Typography>
                                                    </TableCell>
                                                </TableRow>
                                            ) : (
                                                pesagens
                                                    .sort((a, b) => b.semanaVida - a.semanaVida)
                                                    .map((pesagem) => (
                                                        <TableRow key={pesagem.id}>
                                                            <TableCell>{pesagem.semanaVida}ª</TableCell>
                                                            <TableCell>
                                                                {new Date(pesagem.dataPesagem).toLocaleDateString('pt-BR')}
                                                            </TableCell>
                                                            <TableCell>{pesagem.idadeDias}</TableCell>
                                                            <TableCell>
                                                                <Typography variant="body2" fontWeight={600}>
                                                                    {pesagem.pesoMedioGramas.toFixed(0)}g
                                                                </Typography>
                                                            </TableCell>
                                                            <TableCell>{pesagem.quantidadeAmostrada} aves</TableCell>
                                                            <TableCell>
                                                                {pesagem.pesoMinimo && pesagem.pesoMaximo ? 
                                                                    `${pesagem.pesoMinimo.toFixed(0)} - ${pesagem.pesoMaximo.toFixed(0)}` 
                                                                    : '-'
                                                                }
                                                            </TableCell>
                                                            <TableCell>
                                                                {pesagem.coeficienteVariacao ? 
                                                                    `${(100 - pesagem.coeficienteVariacao).toFixed(1)}%` 
                                                                    : '-'
                                                                }
                                                            </TableCell>
                                                            <TableCell>
                                                                {pesagem.ganhoSemanal ? 
                                                                    `${pesagem.ganhoSemanal.toFixed(0)}g` 
                                                                    : '-'
                                                                }
                                                            </TableCell>
                                                            <TableCell>{pesagem.observacoes || '-'}</TableCell>
                                                        </TableRow>
                                                    ))
                                            )}
                                        </TableBody>
                                    </Table>
                                </TableContainer>
                            )}
                        </CardContent>
                    </Card>

                    {/* Dialog para Nova Pesagem */}
                    <Dialog open={open} onClose={() => setOpen(false)} maxWidth="lg" fullWidth>
                        <DialogTitle>Registrar Nova Pesagem</DialogTitle>
                        <DialogContent>
                            <Grid container spacing={3} sx={{ mt: 1 }}>
                                <Grid item xs={12} md={4}>
                                    <TextField
                                        fullWidth
                                        label="Data da Pesagem"
                                        type="date"
                                        value={formData.dataPesagem}
                                        onChange={(e) => setFormData(prev => ({ ...prev, dataPesagem: e.target.value }))}
                                        InputLabelProps={{ shrink: true }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <TextField
                                        fullWidth
                                        label="Idade (dias)"
                                        type="number"
                                        value={formData.idadeDias}
                                        onChange={(e) => setFormData(prev => ({ ...prev, idadeDias: e.target.value }))}
                                        inputProps={{ min: 1 }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <TextField
                                        fullWidth
                                        label="Semana de Vida"
                                        type="number"
                                        value={formData.semanaVida}
                                        onChange={(e) => setFormData(prev => ({ ...prev, semanaVida: e.target.value }))}
                                        inputProps={{ min: 1 }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Peso Médio (gramas)"
                                        type="number"
                                        value={formData.pesoMedioGramas}
                                        onChange={(e) => setFormData(prev => ({ ...prev, pesoMedioGramas: e.target.value }))}
                                        inputProps={{ min: 10, step: 0.1 }}
                                        required
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Quantidade Amostrada"
                                        type="number"
                                        value={formData.quantidadeAmostrada}
                                        onChange={(e) => setFormData(prev => ({ ...prev, quantidadeAmostrada: e.target.value }))}
                                        inputProps={{ min: 10 }}
                                        required
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Peso Mínimo (gramas)"
                                        type="number"
                                        value={formData.pesoMinimo}
                                        onChange={(e) => setFormData(prev => ({ ...prev, pesoMinimo: e.target.value }))}
                                        inputProps={{ min: 5, step: 0.1 }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Peso Máximo (gramas)"
                                        type="number"
                                        value={formData.pesoMaximo}
                                        onChange={(e) => setFormData(prev => ({ ...prev, pesoMaximo: e.target.value }))}
                                        inputProps={{ min: 5, step: 0.1 }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Desvio Padrão"
                                        type="number"
                                        value={formData.desvioPadrao}
                                        onChange={(e) => setFormData(prev => ({ ...prev, desvioPadrao: e.target.value }))}
                                        inputProps={{ min: 0, step: 0.01 }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Coeficiente de Variação (%)"
                                        type="number"
                                        value={formData.coeficienteVariacao}
                                        onChange={(e) => setFormData(prev => ({ ...prev, coeficienteVariacao: e.target.value }))}
                                        inputProps={{ min: 0, max: 100, step: 0.1 }}
                                    />
                                </Grid>
                                <Grid item xs={12}>
                                    <TextField
                                        fullWidth
                                        label="Ganho Semanal (gramas)"
                                        type="number"
                                        value={formData.ganhoSemanal}
                                        onChange={(e) => setFormData(prev => ({ ...prev, ganhoSemanal: e.target.value }))}
                                        inputProps={{ step: 0.1 }}
                                        helperText="Ganho de peso em relação à pesagem anterior"
                                    />
                                </Grid>
                                <Grid item xs={12}>
                                    <TextField
                                        fullWidth
                                        label="Observações"
                                        multiline
                                        rows={3}
                                        value={formData.observacoes}
                                        onChange={(e) => setFormData(prev => ({ ...prev, observacoes: e.target.value }))}
                                        placeholder="Observações sobre a pesagem, condições do lote, etc."
                                    />
                                </Grid>
                            </Grid>
                        </DialogContent>
                        <DialogActions>
                            <Button onClick={() => setOpen(false)}>Cancelar</Button>
                            <Button 
                                onClick={handleSubmit} 
                                variant="contained"
                                disabled={!formData.pesoMedioGramas || !formData.quantidadeAmostrada}
                            >
                                Registrar Pesagem
                            </Button>
                        </DialogActions>
                    </Dialog>
                </>
            )}
        </PageContainer>
    );
}

export default PesagemPage;
