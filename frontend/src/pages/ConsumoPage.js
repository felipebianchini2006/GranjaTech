import React, { useState, useEffect, useCallback } from 'react';
import PageContainer from '../components/PageContainer';
import LoadingSpinner from '../components/LoadingSpinner';
import { 
    Grid, Card, CardContent, Typography, Box, Alert, Button, 
    Select, MenuItem, FormControl, InputLabel, Divider, Tooltip, 
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    Dialog, DialogActions, DialogContent, DialogTitle, TextField,
    Tabs, Tab, Chip, IconButton
} from '@mui/material';
import {
    Add as AddIcon,
    Water as WaterIcon,
    Restaurant as RestaurantIcon,
    TrendingUp as TrendingUpIcon,
    Analytics as AnalyticsIcon,
    Edit as EditIcon,
    Delete as DeleteIcon,
    Refresh as RefreshIcon
} from '@mui/icons-material';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, Legend, ResponsiveContainer, BarChart, Bar } from 'recharts';
import apiService from '../services/apiService';

function ConsumoPage() {
    const [lotes, setLotes] = useState([]);
    const [selectedLote, setSelectedLote] = useState(null);
    const [tabValue, setTabValue] = useState(0);
    const [consumosRacao, setConsumosRacao] = useState([]);
    const [consumosAgua, setConsumosAgua] = useState([]);
    const [resumoConsumo, setResumoConsumo] = useState(null);
    const [loading, setLoading] = useState(true);
    const [loadingData, setLoadingData] = useState(false);
    const [error, setError] = useState('');
    
    // Estados dos diálogos
    const [openRacao, setOpenRacao] = useState(false);
    const [openAgua, setOpenAgua] = useState(false);
    
    // Estados dos formulários
    const [formRacao, setFormRacao] = useState({
        data: new Date().toISOString().split('T')[0],
        quantidadeKg: '',
        tipoRacao: 'Inicial',
        avesVivas: '',
        observacoes: ''
    });
    
    const [formAgua, setFormAgua] = useState({
        data: new Date().toISOString().split('T')[0],
        quantidadeLitros: '',
        avesVivas: '',
        temperaturaAmbiente: '',
        observacoes: ''
    });

    const tiposRacao = ['Inicial', 'Crescimento', 'Terminação'];

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

    const fetchDadosConsumo = useCallback(async (loteId) => {
        if (!loteId) return;
        
        try {
            setLoadingData(true);
            setError('');
            
            const [racaoRes, aguaRes, resumoRes] = await Promise.all([
                apiService.get(`/api/consumo/racao/${loteId}`),
                apiService.get(`/api/consumo/agua/${loteId}`),
                apiService.get(`/api/consumo/resumo/${loteId}`)
            ]);

            setConsumosRacao(racaoRes.data || []);
            setConsumosAgua(aguaRes.data || []);
            setResumoConsumo(resumoRes.data);
            
        } catch (error) {
            console.error('Erro ao buscar dados de consumo:', error);
            setError('Erro ao carregar dados de consumo');
        } finally {
            setLoadingData(false);
        }
    }, []);

    useEffect(() => {
        fetchLotes();
    }, [fetchLotes]);

    useEffect(() => {
        if (selectedLote) {
            fetchDadosConsumo(selectedLote.id);
            // Atualizar valores padrão nos formulários
            setFormRacao(prev => ({ ...prev, avesVivas: selectedLote.quantidadeAvesAtual || '' }));
            setFormAgua(prev => ({ ...prev, avesVivas: selectedLote.quantidadeAvesAtual || '' }));
        }
    }, [selectedLote, fetchDadosConsumo]);

    const handleLoteChange = (event) => {
        const loteId = event.target.value;
        const lote = lotes.find(l => l.id === loteId);
        setSelectedLote(lote);
    };

    const handleSubmitRacao = async () => {
        try {
            await apiService.post('/api/consumo/racao', {
                loteId: selectedLote.id,
                data: formRacao.data,
                quantidadeKg: parseFloat(formRacao.quantidadeKg),
                tipoRacao: formRacao.tipoRacao,
                avesVivas: parseInt(formRacao.avesVivas),
                observacoes: formRacao.observacoes || null
            });

            setOpenRacao(false);
            setFormRacao({
                data: new Date().toISOString().split('T')[0],
                quantidadeKg: '',
                tipoRacao: 'Inicial',
                avesVivas: selectedLote.quantidadeAvesAtual || '',
                observacoes: ''
            });
            fetchDadosConsumo(selectedLote.id);
        } catch (error) {
            console.error('Erro ao registrar consumo de ração:', error);
            setError('Erro ao registrar consumo de ração');
        }
    };

    const handleSubmitAgua = async () => {
        try {
            await apiService.post('/api/consumo/agua', {
                loteId: selectedLote.id,
                data: formAgua.data,
                quantidadeLitros: parseFloat(formAgua.quantidadeLitros),
                avesVivas: parseInt(formAgua.avesVivas),
                temperaturaAmbiente: formAgua.temperaturaAmbiente ? parseFloat(formAgua.temperaturaAmbiente) : null,
                observacoes: formAgua.observacoes || null
            });

            setOpenAgua(false);
            setFormAgua({
                data: new Date().toISOString().split('T')[0],
                quantidadeLitros: '',
                avesVivas: selectedLote.quantidadeAvesAtual || '',
                temperaturaAmbiente: '',
                observacoes: ''
            });
            fetchDadosConsumo(selectedLote.id);
        } catch (error) {
            console.error('Erro ao registrar consumo de água:', error);
            setError('Erro ao registrar consumo de água');
        }
    };

    const prepareChartData = () => {
        const racaoData = consumosRacao.map(item => ({
            data: new Date(item.data).toLocaleDateString('pt-BR'),
            racao: item.quantidadeKg,
            consumoPorAve: item.consumoPorAveGramas
        }));

        const aguaData = consumosAgua.map(item => ({
            data: new Date(item.data).toLocaleDateString('pt-BR'),
            agua: item.quantidadeLitros,
            consumoPorAve: item.consumoPorAveMl
        }));

        return { racaoData, aguaData };
    };

    if (loading) {
        return (
            <PageContainer title="Gestão de Consumo" subtitle="Controle de consumo de ração e água">
                <LoadingSpinner />
            </PageContainer>
        );
    }

    const { racaoData, aguaData } = prepareChartData();

    return (
        <PageContainer 
            title="Gestão de Consumo" 
            subtitle="Controle detalhado de consumo de ração e água por lote"
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
                        onClick={() => fetchDadosConsumo(selectedLote?.id)} 
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
                    Nenhum lote ativo encontrado. Crie um lote para registrar consumos.
                </Alert>
            ) : (
                <>
                    {/* Resumo de Consumo */}
                    {resumoConsumo && (
                        <Grid container spacing={3} sx={{ mb: 3 }}>
                            <Grid item xs={12} md={4}>
                                <Card>
                                    <CardContent>
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                                            <RestaurantIcon color="primary" />
                                            <Typography variant="h6">Consumo de Ração</Typography>
                                        </Box>
                                        <Typography variant="h4" color="primary">
                                            {resumoConsumo.consumoRacao.totalKg.toFixed(1)} kg
                                        </Typography>
                                        <Typography variant="body2" color="text.secondary">
                                            Média: {resumoConsumo.consumoRacao.mediaPorAve.toFixed(1)}g/ave
                                        </Typography>
                                    </CardContent>
                                </Card>
                            </Grid>
                            
                            <Grid item xs={12} md={4}>
                                <Card>
                                    <CardContent>
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                                            <WaterIcon color="info" />
                                            <Typography variant="h6">Consumo de Água</Typography>
                                        </Box>
                                        <Typography variant="h4" color="info.main">
                                            {resumoConsumo.consumoAgua.totalLitros.toFixed(1)} L
                                        </Typography>
                                        <Typography variant="body2" color="text.secondary">
                                            Média: {resumoConsumo.consumoAgua.mediaPorAve.toFixed(1)}ml/ave
                                        </Typography>
                                    </CardContent>
                                </Card>
                            </Grid>
                            
                            <Grid item xs={12} md={4}>
                                <Card>
                                    <CardContent>
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                                            <AnalyticsIcon color="success" />
                                            <Typography variant="h6">Relação Água/Ração</Typography>
                                        </Box>
                                        <Typography variant="h4" color="success.main">
                                            {resumoConsumo.relacaoAguaRacao.toFixed(2)}
                                        </Typography>
                                        <Typography variant="body2" color="text.secondary">
                                            Ideal: 2.0 L/kg
                                        </Typography>
                                    </CardContent>
                                </Card>
                            </Grid>
                        </Grid>
                    )}

                    {/* Tabs para Ração e Água */}
                    <Card>
                        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                            <Tabs value={tabValue} onChange={(e, newValue) => setTabValue(newValue)}>
                                <Tab 
                                    label="Consumo de Ração" 
                                    icon={<RestaurantIcon />} 
                                    iconPosition="start"
                                />
                                <Tab 
                                    label="Consumo de Água" 
                                    icon={<WaterIcon />} 
                                    iconPosition="start"
                                />
                            </Tabs>
                        </Box>

                        <CardContent>
                            {tabValue === 0 && (
                                <>
                                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                                        <Typography variant="h6">Registros de Consumo de Ração</Typography>
                                        <Button
                                            variant="contained"
                                            startIcon={<AddIcon />}
                                            onClick={() => setOpenRacao(true)}
                                        >
                                            Registrar Consumo
                                        </Button>
                                    </Box>

                                    {loadingData ? (
                                        <LoadingSpinner />
                                    ) : (
                                        <>
                                            {/* Gráfico de Consumo de Ração */}
                                            {racaoData.length > 0 && (
                                                <Box sx={{ mb: 4 }}>
                                                    <Typography variant="subtitle1" gutterBottom>
                                                        Evolução do Consumo de Ração
                                                    </Typography>
                                                    <Box sx={{ width: '100%', height: 300 }}>
                                                        <ResponsiveContainer>
                                                            <LineChart data={racaoData}>
                                                                <CartesianGrid strokeDasharray="3 3" />
                                                                <XAxis dataKey="data" />
                                                                <YAxis yAxisId="left" label={{ value: 'Total (kg)', angle: -90, position: 'insideLeft' }} />
                                                                <YAxis yAxisId="right" orientation="right" label={{ value: 'Por Ave (g)', angle: 90, position: 'insideRight' }} />
                                                                <RechartsTooltip />
                                                                <Legend />
                                                                <Bar yAxisId="left" dataKey="racao" fill="#1976d2" name="Total (kg)" />
                                                                <Line yAxisId="right" type="monotone" dataKey="consumoPorAve" stroke="#ff9800" name="Por Ave (g)" />
                                                            </LineChart>
                                                        </ResponsiveContainer>
                                                    </Box>
                                                </Box>
                                            )}

                                            {/* Tabela de Registros de Ração */}
                                            <TableContainer component={Paper} variant="outlined">
                                                <Table>
                                                    <TableHead>
                                                        <TableRow>
                                                            <TableCell>Data</TableCell>
                                                            <TableCell>Quantidade (kg)</TableCell>
                                                            <TableCell>Tipo</TableCell>
                                                            <TableCell>Aves Vivas</TableCell>
                                                            <TableCell>Por Ave (g)</TableCell>
                                                            <TableCell>Observações</TableCell>
                                                        </TableRow>
                                                    </TableHead>
                                                    <TableBody>
                                                        {consumosRacao.length === 0 ? (
                                                            <TableRow>
                                                                <TableCell colSpan={6} align="center">
                                                                    <Typography color="text.secondary">
                                                                        Nenhum registro de consumo de ração encontrado
                                                                    </Typography>
                                                                </TableCell>
                                                            </TableRow>
                                                        ) : (
                                                            consumosRacao.map((consumo) => (
                                                                <TableRow key={consumo.id}>
                                                                    <TableCell>
                                                                        {new Date(consumo.data).toLocaleDateString('pt-BR')}
                                                                    </TableCell>
                                                                    <TableCell>{consumo.quantidadeKg}</TableCell>
                                                                    <TableCell>
                                                                        <Chip 
                                                                            label={consumo.tipoRacao} 
                                                                            size="small"
                                                                            color={
                                                                                consumo.tipoRacao === 'Inicial' ? 'success' :
                                                                                consumo.tipoRacao === 'Crescimento' ? 'warning' : 'error'
                                                                            }
                                                                        />
                                                                    </TableCell>
                                                                    <TableCell>{consumo.avesVivas.toLocaleString()}</TableCell>
                                                                    <TableCell>{consumo.consumoPorAveGramas.toFixed(1)}</TableCell>
                                                                    <TableCell>{consumo.observacoes || '-'}</TableCell>
                                                                </TableRow>
                                                            ))
                                                        )}
                                                    </TableBody>
                                                </Table>
                                            </TableContainer>
                                        </>
                                    )}
                                </>
                            )}

                            {tabValue === 1 && (
                                <>
                                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                                        <Typography variant="h6">Registros de Consumo de Água</Typography>
                                        <Button
                                            variant="contained"
                                            startIcon={<AddIcon />}
                                            onClick={() => setOpenAgua(true)}
                                        >
                                            Registrar Consumo
                                        </Button>
                                    </Box>

                                    {loadingData ? (
                                        <LoadingSpinner />
                                    ) : (
                                        <>
                                            {/* Gráfico de Consumo de Água */}
                                            {aguaData.length > 0 && (
                                                <Box sx={{ mb: 4 }}>
                                                    <Typography variant="subtitle1" gutterBottom>
                                                        Evolução do Consumo de Água
                                                    </Typography>
                                                    <Box sx={{ width: '100%', height: 300 }}>
                                                        <ResponsiveContainer>
                                                            <LineChart data={aguaData}>
                                                                <CartesianGrid strokeDasharray="3 3" />
                                                                <XAxis dataKey="data" />
                                                                <YAxis yAxisId="left" label={{ value: 'Total (L)', angle: -90, position: 'insideLeft' }} />
                                                                <YAxis yAxisId="right" orientation="right" label={{ value: 'Por Ave (ml)', angle: 90, position: 'insideRight' }} />
                                                                <RechartsTooltip />
                                                                <Legend />
                                                                <Bar yAxisId="left" dataKey="agua" fill="#2196f3" name="Total (L)" />
                                                                <Line yAxisId="right" type="monotone" dataKey="consumoPorAve" stroke="#ff5722" name="Por Ave (ml)" />
                                                            </LineChart>
                                                        </ResponsiveContainer>
                                                    </Box>
                                                </Box>
                                            )}

                                            {/* Tabela de Registros de Água */}
                                            <TableContainer component={Paper} variant="outlined">
                                                <Table>
                                                    <TableHead>
                                                        <TableRow>
                                                            <TableCell>Data</TableCell>
                                                            <TableCell>Quantidade (L)</TableCell>
                                                            <TableCell>Aves Vivas</TableCell>
                                                            <TableCell>Por Ave (ml)</TableCell>
                                                            <TableCell>Temperatura (°C)</TableCell>
                                                            <TableCell>Observações</TableCell>
                                                        </TableRow>
                                                    </TableHead>
                                                    <TableBody>
                                                        {consumosAgua.length === 0 ? (
                                                            <TableRow>
                                                                <TableCell colSpan={6} align="center">
                                                                    <Typography color="text.secondary">
                                                                        Nenhum registro de consumo de água encontrado
                                                                    </Typography>
                                                                </TableCell>
                                                            </TableRow>
                                                        ) : (
                                                            consumosAgua.map((consumo) => (
                                                                <TableRow key={consumo.id}>
                                                                    <TableCell>
                                                                        {new Date(consumo.data).toLocaleDateString('pt-BR')}
                                                                    </TableCell>
                                                                    <TableCell>{consumo.quantidadeLitros}</TableCell>
                                                                    <TableCell>{consumo.avesVivas.toLocaleString()}</TableCell>
                                                                    <TableCell>{consumo.consumoPorAveMl.toFixed(1)}</TableCell>
                                                                    <TableCell>
                                                                        {consumo.temperaturaAmbiente ? 
                                                                            `${consumo.temperaturaAmbiente}°C` : '-'
                                                                        }
                                                                    </TableCell>
                                                                    <TableCell>{consumo.observacoes || '-'}</TableCell>
                                                                </TableRow>
                                                            ))
                                                        )}
                                                    </TableBody>
                                                </Table>
                                            </TableContainer>
                                        </>
                                    )}
                                </>
                            )}
                        </CardContent>
                    </Card>

                    {/* Dialog para Ração */}
                    <Dialog open={openRacao} onClose={() => setOpenRacao(false)} maxWidth="md" fullWidth>
                        <DialogTitle>Registrar Consumo de Ração</DialogTitle>
                        <DialogContent>
                            <Grid container spacing={3} sx={{ mt: 1 }}>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Data"
                                        type="date"
                                        value={formRacao.data}
                                        onChange={(e) => setFormRacao(prev => ({ ...prev, data: e.target.value }))}
                                        InputLabelProps={{ shrink: true }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Quantidade (kg)"
                                        type="number"
                                        value={formRacao.quantidadeKg}
                                        onChange={(e) => setFormRacao(prev => ({ ...prev, quantidadeKg: e.target.value }))}
                                        inputProps={{ min: 0, step: 0.1 }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <FormControl fullWidth>
                                        <InputLabel>Tipo de Ração</InputLabel>
                                        <Select
                                            value={formRacao.tipoRacao}
                                            onChange={(e) => setFormRacao(prev => ({ ...prev, tipoRacao: e.target.value }))}
                                            label="Tipo de Ração"
                                        >
                                            {tiposRacao.map(tipo => (
                                                <MenuItem key={tipo} value={tipo}>{tipo}</MenuItem>
                                            ))}
                                        </Select>
                                    </FormControl>
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Aves Vivas"
                                        type="number"
                                        value={formRacao.avesVivas}
                                        onChange={(e) => setFormRacao(prev => ({ ...prev, avesVivas: e.target.value }))}
                                        inputProps={{ min: 1 }}
                                    />
                                </Grid>
                                <Grid item xs={12}>
                                    <TextField
                                        fullWidth
                                        label="Observações"
                                        multiline
                                        rows={3}
                                        value={formRacao.observacoes}
                                        onChange={(e) => setFormRacao(prev => ({ ...prev, observacoes: e.target.value }))}
                                    />
                                </Grid>
                            </Grid>
                        </DialogContent>
                        <DialogActions>
                            <Button onClick={() => setOpenRacao(false)}>Cancelar</Button>
                            <Button onClick={handleSubmitRacao} variant="contained">Registrar</Button>
                        </DialogActions>
                    </Dialog>

                    {/* Dialog para Água */}
                    <Dialog open={openAgua} onClose={() => setOpenAgua(false)} maxWidth="md" fullWidth>
                        <DialogTitle>Registrar Consumo de Água</DialogTitle>
                        <DialogContent>
                            <Grid container spacing={3} sx={{ mt: 1 }}>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Data"
                                        type="date"
                                        value={formAgua.data}
                                        onChange={(e) => setFormAgua(prev => ({ ...prev, data: e.target.value }))}
                                        InputLabelProps={{ shrink: true }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Quantidade (Litros)"
                                        type="number"
                                        value={formAgua.quantidadeLitros}
                                        onChange={(e) => setFormAgua(prev => ({ ...prev, quantidadeLitros: e.target.value }))}
                                        inputProps={{ min: 0, step: 0.1 }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Aves Vivas"
                                        type="number"
                                        value={formAgua.avesVivas}
                                        onChange={(e) => setFormAgua(prev => ({ ...prev, avesVivas: e.target.value }))}
                                        inputProps={{ min: 1 }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Temperatura Ambiente (°C)"
                                        type="number"
                                        value={formAgua.temperaturaAmbiente}
                                        onChange={(e) => setFormAgua(prev => ({ ...prev, temperaturaAmbiente: e.target.value }))}
                                        inputProps={{ min: -10, max: 50, step: 0.1 }}
                                    />
                                </Grid>
                                <Grid item xs={12}>
                                    <TextField
                                        fullWidth
                                        label="Observações"
                                        multiline
                                        rows={3}
                                        value={formAgua.observacoes}
                                        onChange={(e) => setFormAgua(prev => ({ ...prev, observacoes: e.target.value }))}
                                    />
                                </Grid>
                            </Grid>
                        </DialogContent>
                        <DialogActions>
                            <Button onClick={() => setOpenAgua(false)}>Cancelar</Button>
                            <Button onClick={handleSubmitAgua} variant="contained">Registrar</Button>
                        </DialogActions>
                    </Dialog>
                </>
            )}
        </PageContainer>
    );
}

export default ConsumoPage;
