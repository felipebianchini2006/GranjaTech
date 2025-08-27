import React, { useState, useEffect, useCallback } from 'react';
import PageContainer from '../components/PageContainer';
import LoadingSpinner from '../components/LoadingSpinner';
import { 
    Grid, Card, CardContent, Typography, Box, Alert, Button, Chip, 
    Select, MenuItem, FormControl, InputLabel, Divider, Tooltip, 
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    LinearProgress, IconButton
} from '@mui/material';
import {
    Agriculture as AgricultureIcon,
    TrendingUp as TrendingUpIcon,
    TrendingDown as TrendingDownIcon,
    Speed as SpeedIcon,
    Favorite as HeartIcon,
    Scale as ScaleIcon,
    Warning as WarningIcon,
    CheckCircle as CheckCircleIcon,
    Error as ErrorIcon,
    Info as InfoIcon,
    Refresh as RefreshIcon,
    Analytics as AnalyticsIcon,
    Assessment as AssessmentIcon
} from '@mui/icons-material';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, Legend, ResponsiveContainer, BarChart, Bar, PieChart, Pie, Cell } from 'recharts';
import apiService from '../services/apiService';

function AviculturaPage() {
    const [lotes, setLotes] = useState([]);
    const [selectedLote, setSelectedLote] = useState(null);
    const [metricas, setMetricas] = useState(null);
    const [alertas, setAlertas] = useState([]);
    const [comparacaoIndustria, setComparacaoIndustria] = useState(null);
    const [curvasCrescimento, setCurvasCrescimento] = useState(null);
    const [loading, setLoading] = useState(true);
    const [loadingDetalhes, setLoadingDetalhes] = useState(false);
    const [error, setError] = useState('');

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

    const fetchDadosLote = useCallback(async (loteId) => {
        if (!loteId) return;
        
        try {
            setLoadingDetalhes(true);
            setError('');
            
            const [
                metricasRes,
                alertasRes,
                comparacaoRes,
                curvasRes
            ] = await Promise.all([
                apiService.get(`/api/avicultura/${loteId}/metricas`),
                apiService.get(`/api/avicultura/${loteId}/alertas`),
                apiService.get(`/api/avicultura/${loteId}/comparacao-industria`),
                apiService.get(`/api/avicultura/${loteId}/curvas-crescimento`)
            ]);

            setMetricas(metricasRes.data);
            setAlertas(alertasRes.data || []);
            setComparacaoIndustria(comparacaoRes.data);
            setCurvasCrescimento(curvasRes.data);
            
        } catch (error) {
            console.error('Erro ao buscar dados do lote:', error);
            setError('Erro ao carregar dados específicos do lote');
        } finally {
            setLoadingDetalhes(false);
        }
    }, []);

    useEffect(() => {
        fetchLotes();
    }, [fetchLotes]);

    useEffect(() => {
        if (selectedLote) {
            fetchDadosLote(selectedLote.id);
        }
    }, [selectedLote, fetchDadosLote]);

    const handleLoteChange = (event) => {
        const loteId = event.target.value;
        const lote = lotes.find(l => l.id === loteId);
        setSelectedLote(lote);
    };

    const getStatusColor = (status) => {
        switch (status) {
            case 'Excelente': return 'success';
            case 'Bom': return 'primary';
            case 'Regular': return 'warning';
            case 'Ruim': return 'error';
            default: return 'default';
        }
    };

    const getSeverityColor = (severidade) => {
        switch (severidade) {
            case 'Critica': return 'error';
            case 'Alta': return 'warning';
            case 'Media': return 'info';
            case 'Baixa': return 'success';
            default: return 'default';
        }
    };

    const MetricaCard = ({ titulo, valor, unidade, status, icone: Icon, comparacao }) => (
        <Card sx={{ height: '100%', position: 'relative' }}>
            <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                        {titulo}
                    </Typography>
                    <Icon color="primary" sx={{ fontSize: 24 }} />
                </Box>
                
                <Typography variant="h4" component="div" sx={{ mb: 1 }}>
                    {typeof valor === 'number' ? valor.toLocaleString('pt-BR', { 
                        minimumFractionDigits: titulo.includes('IEP') ? 0 : 2, 
                        maximumFractionDigits: titulo.includes('IEP') ? 0 : 2 
                    }) : valor}
                    {unidade && (
                        <Typography component="span" variant="body1" color="text.secondary" sx={{ ml: 0.5 }}>
                            {unidade}
                        </Typography>
                    )}
                </Typography>

                {status && (
                    <Chip 
                        label={status} 
                        color={getStatusColor(status)} 
                        size="small" 
                        sx={{ mb: 1 }}
                    />
                )}

                {comparacao && (
                    <Box sx={{ mt: 1 }}>
                        <Typography variant="caption" color="text.secondary">
                            Vs. Indústria: {comparacao > 0 ? '+' : ''}{comparacao.toFixed(1)}%
                        </Typography>
                        <LinearProgress 
                            variant="determinate" 
                            value={Math.min(Math.abs(comparacao), 100)} 
                            color={comparacao > 0 ? 'success' : 'error'}
                            sx={{ mt: 0.5, height: 4, borderRadius: 2 }}
                        />
                    </Box>
                )}
            </CardContent>
        </Card>
    );

    if (loading) {
        return (
            <PageContainer title="Avicultura Profissional" subtitle="Métricas e análises específicas de avicultura">
                <LoadingSpinner />
            </PageContainer>
        );
    }

    return (
        <PageContainer 
            title="Avicultura Profissional" 
            subtitle="Métricas avançadas e análises específicas para produção avícola"
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
                        onClick={() => fetchDadosLote(selectedLote?.id)} 
                        disabled={loadingDetalhes}
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
                <Alert severity="info" sx={{ mb: 3 }}>
                    Nenhum lote ativo encontrado. Crie um lote para visualizar as métricas de avicultura.
                </Alert>
            ) : (
                <>
                    {/* Informações do Lote Selecionado */}
                    <Card sx={{ mb: 3 }}>
                        <CardContent>
                            <Grid container spacing={3}>
                                <Grid item xs={12} md={3}>
                                    <Typography variant="subtitle2" color="text.secondary">Lote</Typography>
                                    <Typography variant="h6">{selectedLote.identificador}</Typography>
                                </Grid>
                                <Grid item xs={12} md={2}>
                                    <Typography variant="subtitle2" color="text.secondary">Idade</Typography>
                                    <Typography variant="h6">{selectedLote.idadeAtualDias} dias</Typography>
                                </Grid>
                                <Grid item xs={12} md={2}>
                                    <Typography variant="subtitle2" color="text.secondary">Aves Atuais</Typography>
                                    <Typography variant="h6">{selectedLote.quantidadeAvesAtual?.toLocaleString()}</Typography>
                                </Grid>
                                <Grid item xs={12} md={2}>
                                    <Typography variant="subtitle2" color="text.secondary">Viabilidade</Typography>
                                    <Typography variant="h6" color={selectedLote.viabilidade >= 95 ? 'success.main' : 'warning.main'}>
                                        {selectedLote.viabilidade?.toFixed(1)}%
                                    </Typography>
                                </Grid>
                                <Grid item xs={12} md={3}>
                                    <Typography variant="subtitle2" color="text.secondary">Linhagem</Typography>
                                    <Typography variant="h6">{selectedLote.linhagem || 'N/A'}</Typography>
                                </Grid>
                            </Grid>
                        </CardContent>
                    </Card>

                    {loadingDetalhes ? (
                        <LoadingSpinner />
                    ) : (
                        <>
                            {/* Métricas Principais */}
                            {metricas && (
                                <Grid container spacing={3} sx={{ mb: 3 }}>
                                    <Grid item xs={12} sm={6} md={3}>
                                        <MetricaCard
                                            titulo="IEP (Índice de Eficiência Produtiva)"
                                            valor={metricas.iep}
                                            status={comparacaoIndustria?.iep?.status}
                                            icone={SpeedIcon}
                                            comparacao={comparacaoIndustria?.iep?.percentualDiferenca}
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={6} md={3}>
                                        <MetricaCard
                                            titulo="Conversão Alimentar"
                                            valor={metricas.conversaoAlimentar}
                                            status={comparacaoIndustria?.conversaoAlimentar?.status}
                                            icone={ScaleIcon}
                                            comparacao={comparacaoIndustria?.conversaoAlimentar?.percentualDiferenca}
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={6} md={3}>
                                        <MetricaCard
                                            titulo="Ganho Médio Diário"
                                            valor={metricas.ganhoMedioDiario}
                                            unidade="g/dia"
                                            status={comparacaoIndustria?.ganhoMedioDiario?.status}
                                            icone={TrendingUpIcon}
                                            comparacao={comparacaoIndustria?.ganhoMedioDiario?.percentualDiferenca}
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={6} md={3}>
                                        <MetricaCard
                                            titulo="Viabilidade"
                                            valor={metricas.viabilidade}
                                            unidade="%"
                                            status={comparacaoIndustria?.viabilidade?.status}
                                            icone={HeartIcon}
                                            comparacao={comparacaoIndustria?.viabilidade?.percentualDiferenca}
                                        />
                                    </Grid>
                                </Grid>
                            )}

                            {/* Alertas */}
                            {alertas.length > 0 && (
                                <Card sx={{ mb: 3 }}>
                                    <CardContent>
                                        <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                            <WarningIcon color="warning" />
                                            Alertas e Recomendações
                                        </Typography>
                                        <Grid container spacing={2}>
                                            {alertas.map((alerta, index) => (
                                                <Grid item xs={12} md={6} key={index}>
                                                    <Alert 
                                                        severity={getSeverityColor(alerta.severidade)} 
                                                        sx={{ mb: 1 }}
                                                    >
                                                        <Typography variant="subtitle2" gutterBottom>
                                                            {alerta.tipoAlerta}: {alerta.descricao}
                                                        </Typography>
                                                        <Typography variant="body2">
                                                            Valor atual: {alerta.valorAtual} {alerta.unidade}
                                                        </Typography>
                                                        {alerta.recomendacao && (
                                                            <Typography variant="body2" sx={{ mt: 1, fontStyle: 'italic' }}>
                                                                💡 {alerta.recomendacao}
                                                            </Typography>
                                                        )}
                                                    </Alert>
                                                </Grid>
                                            ))}
                                        </Grid>
                                    </CardContent>
                                </Card>
                            )}

                            {/* Comparação com Indústria */}
                            {comparacaoIndustria && (
                                <Card sx={{ mb: 3 }}>
                                    <CardContent>
                                        <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                            <AssessmentIcon color="primary" />
                                            Comparação com Padrões da Indústria
                                        </Typography>
                                        
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                                            <Typography variant="h4" color="primary">
                                                {comparacaoIndustria.pontuacaoGeral}
                                            </Typography>
                                            <Chip 
                                                label={comparacaoIndustria.classificacaoGeral} 
                                                color={getStatusColor(comparacaoIndustria.classificacaoGeral)} 
                                                size="large"
                                            />
                                        </Box>

                                        <TableContainer component={Paper} variant="outlined">
                                            <Table size="small">
                                                <TableHead>
                                                    <TableRow>
                                                        <TableCell><strong>Métrica</strong></TableCell>
                                                        <TableCell align="center"><strong>Seu Lote</strong></TableCell>
                                                        <TableCell align="center"><strong>Padrão Indústria</strong></TableCell>
                                                        <TableCell align="center"><strong>Excelência</strong></TableCell>
                                                        <TableCell align="center"><strong>Status</strong></TableCell>
                                                    </TableRow>
                                                </TableHead>
                                                <TableBody>
                                                    {[
                                                        comparacaoIndustria.conversaoAlimentar,
                                                        comparacaoIndustria.ganhoMedioDiario,
                                                        comparacaoIndustria.viabilidade,
                                                        comparacaoIndustria.iep
                                                    ].map((metrica, index) => (
                                                        <TableRow key={index}>
                                                            <TableCell>{metrica.nome}</TableCell>
                                                            <TableCell align="center">
                                                                {metrica.valorAtual.toFixed(2)} {metrica.unidade}
                                                            </TableCell>
                                                            <TableCell align="center">
                                                                {metrica.valorPadraoIndustria.toFixed(2)} {metrica.unidade}
                                                            </TableCell>
                                                            <TableCell align="center">
                                                                {metrica.valorPadraoExcelencia.toFixed(2)} {metrica.unidade}
                                                            </TableCell>
                                                            <TableCell align="center">
                                                                <Chip 
                                                                    label={metrica.status} 
                                                                    color={getStatusColor(metrica.status)} 
                                                                    size="small"
                                                                />
                                                            </TableCell>
                                                        </TableRow>
                                                    ))}
                                                </TableBody>
                                            </Table>
                                        </TableContainer>
                                    </CardContent>
                                </Card>
                            )}

                            {/* Gráficos de Curvas de Crescimento */}
                            {curvasCrescimento && curvasCrescimento.curvaPeso && curvasCrescimento.curvaPeso.length > 0 && (
                                <Grid container spacing={3}>
                                    <Grid item xs={12} md={6}>
                                        <Card>
                                            <CardContent>
                                                <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                    <AnalyticsIcon color="primary" />
                                                    Curva de Crescimento
                                                </Typography>
                                                <Box sx={{ width: '100%', height: 300 }}>
                                                    <ResponsiveContainer>
                                                        <LineChart data={curvasCrescimento.curvaPeso}>
                                                            <CartesianGrid strokeDasharray="3 3" />
                                                            <XAxis dataKey="semana" label={{ value: 'Semana', position: 'insideBottom', offset: -5 }} />
                                                            <YAxis label={{ value: 'Peso (g)', angle: -90, position: 'insideLeft' }} />
                                                            <RechartsTooltip 
                                                                labelFormatter={(value) => `Semana ${value}`}
                                                                formatter={(value, name) => [`${value}g`, name === 'valor' ? 'Peso Atual' : name]}
                                                            />
                                                            <Legend />
                                                            <Line 
                                                                type="monotone" 
                                                                dataKey="valor" 
                                                                stroke="#1976d2" 
                                                                strokeWidth={3}
                                                                name="Peso Atual"
                                                                connectNulls={false}
                                                            />
                                                            {curvasCrescimento.curvaPeso[0]?.valorPadrao && (
                                                                <Line 
                                                                    type="monotone" 
                                                                    dataKey="valorPadrao" 
                                                                    stroke="#ff9800" 
                                                                    strokeDasharray="5 5"
                                                                    name="Padrão Indústria"
                                                                />
                                                            )}
                                                        </LineChart>
                                                    </ResponsiveContainer>
                                                </Box>
                                            </CardContent>
                                        </Card>
                                    </Grid>
                                    
                                    <Grid item xs={12} md={6}>
                                        <Card>
                                            <CardContent>
                                                <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                    <AnalyticsIcon color="primary" />
                                                    Consumo de Ração Semanal
                                                </Typography>
                                                <Box sx={{ width: '100%', height: 300 }}>
                                                    <ResponsiveContainer>
                                                        <BarChart data={curvasCrescimento.curvaConsumoRacao}>
                                                            <CartesianGrid strokeDasharray="3 3" />
                                                            <XAxis dataKey="semana" label={{ value: 'Semana', position: 'insideBottom', offset: -5 }} />
                                                            <YAxis label={{ value: 'Consumo (kg)', angle: -90, position: 'insideLeft' }} />
                                                            <RechartsTooltip 
                                                                labelFormatter={(value) => `Semana ${value}`}
                                                                formatter={(value) => [`${value}kg`, 'Consumo']}
                                                            />
                                                            <Bar dataKey="valor" fill="#4caf50" />
                                                        </BarChart>
                                                    </ResponsiveContainer>
                                                </Box>
                                            </CardContent>
                                        </Card>
                                    </Grid>
                                </Grid>
                            )}
                        </>
                    )}
                </>
            )}
        </PageContainer>
    );
}

export default AviculturaPage;
