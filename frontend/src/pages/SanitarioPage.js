import React, { useState, useEffect, useCallback } from 'react';
import PageContainer from '../components/PageContainer';
import LoadingSpinner from '../components/LoadingSpinner';
import { 
    Grid, Card, CardContent, Typography, Box, Alert, Button, 
    Select, MenuItem, FormControl, InputLabel, Chip, 
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    Dialog, DialogActions, DialogContent, DialogTitle, TextField, IconButton,
    Tabs, Tab, Accordion, AccordionSummary, AccordionDetails
} from '@mui/material';
import {
    Add as AddIcon,
    LocalHospital as MedicalIcon,
    Vaccines as VaccineIcon,
    Warning as WarningIcon,
    Assignment as AssignmentIcon,
    Refresh as RefreshIcon,
    ExpandMore as ExpandMoreIcon,
    Timeline as TimelineIcon
} from '@mui/icons-material';
import { PieChart, Pie, Cell, ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, Legend } from 'recharts';
import apiService from '../services/apiService';

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8'];

function SanitarioPage() {
    const [lotes, setLotes] = useState([]);
    const [selectedLote, setSelectedLote] = useState(null);
    const [eventos, setEventos] = useState([]);
    const [resumoSanitario, setResumoSanitario] = useState(null);
    const [loading, setLoading] = useState(true);
    const [loadingData, setLoadingData] = useState(false);
    const [error, setError] = useState('');
    const [open, setOpen] = useState(false);
    const [tabValue, setTabValue] = useState(0);
    
    const [formData, setFormData] = useState({
        data: new Date().toISOString().split('T')[0],
        tipoEvento: 'Vacinacao',
        produto: '',
        loteProduto: '',
        dosagem: '',
        viaAdministracao: '',
        avesTratadas: '',
        duracaoTratamentoDias: '',
        periodoCarenciaDias: '',
        responsavelAplicacao: '',
        sintomas: '',
        observacoes: '',
        custo: ''
    });

    const tiposEvento = ['Vacinacao', 'Medicacao', 'Doenca', 'Preventivo'];
    const viasAdministracao = ['Oral', 'Intramuscular', 'Subcutanea', 'Spray', 'Água', 'Ração'];

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

    const fetchEventosSanitarios = useCallback(async (loteId) => {
        if (!loteId) return;
        
        try {
            setLoadingData(true);
            setError('');
            
            const [eventosRes, resumoRes] = await Promise.all([
                apiService.get(`/api/sanitario/${loteId}`),
                apiService.get(`/api/avicultura/${loteId}/resumo-sanitario`)
            ]);

            setEventos(eventosRes.data || []);
            setResumoSanitario(resumoRes.data);
            
        } catch (error) {
            console.error('Erro ao buscar eventos sanitários:', error);
            setError('Erro ao carregar eventos sanitários');
        } finally {
            setLoadingData(false);
        }
    }, []);

    useEffect(() => {
        fetchLotes();
    }, [fetchLotes]);

    useEffect(() => {
        if (selectedLote) {
            fetchEventosSanitarios(selectedLote.id);
            setFormData(prev => ({ 
                ...prev, 
                avesTratadas: selectedLote.quantidadeAvesAtual?.toString() || '' 
            }));
        }
    }, [selectedLote, fetchEventosSanitarios]);

    const handleLoteChange = (event) => {
        const loteId = event.target.value;
        const lote = lotes.find(l => l.id === loteId);
        setSelectedLote(lote);
    };

    const handleSubmit = async () => {
        try {
            await apiService.post('/api/sanitario', {
                loteId: selectedLote.id,
                data: formData.data,
                tipoEvento: formData.tipoEvento,
                produto: formData.produto,
                loteProduto: formData.loteProduto || null,
                dosagem: formData.dosagem || null,
                viaAdministracao: formData.viaAdministracao || null,
                avesTratadas: formData.avesTratadas ? parseInt(formData.avesTratadas) : null,
                duracaoTratamentoDias: formData.duracaoTratamentoDias ? parseInt(formData.duracaoTratamentoDias) : null,
                periodoCarenciaDias: formData.periodoCarenciaDias ? parseInt(formData.periodoCarenciaDias) : null,
                responsavelAplicacao: formData.responsavelAplicacao || null,
                sintomas: formData.sintomas || null,
                observacoes: formData.observacoes || null,
                custo: formData.custo ? parseFloat(formData.custo) : null
            });

            setOpen(false);
            setFormData({
                data: new Date().toISOString().split('T')[0],
                tipoEvento: 'Vacinacao',
                produto: '',
                loteProduto: '',
                dosagem: '',
                viaAdministracao: '',
                avesTratadas: selectedLote.quantidadeAvesAtual?.toString() || '',
                duracaoTratamentoDias: '',
                periodoCarenciaDias: '',
                responsavelAplicacao: '',
                sintomas: '',
                observacoes: '',
                custo: ''
            });
            fetchEventosSanitarios(selectedLote.id);
        } catch (error) {
            console.error('Erro ao registrar evento sanitário:', error);
            setError('Erro ao registrar evento sanitário');
        }
    };

    const getTipoEventoColor = (tipo) => {
        switch (tipo) {
            case 'Vacinacao': return 'success';
            case 'Medicacao': return 'info';
            case 'Doenca': return 'error';
            case 'Preventivo': return 'warning';
            default: return 'default';
        }
    };

    const prepareEventosPorTipo = () => {
        const grupos = eventos.reduce((acc, evento) => {
            acc[evento.tipoEvento] = (acc[evento.tipoEvento] || 0) + 1;
            return acc;
        }, {});

        return Object.entries(grupos).map(([tipo, count]) => ({
            name: tipo,
            value: count
        }));
    };

    const prepareCustosPorMes = () => {
        const custosPorMes = eventos
            .filter(e => e.custo > 0)
            .reduce((acc, evento) => {
                const mes = new Date(evento.data).toLocaleDateString('pt-BR', { month: 'short', year: '2-digit' });
                acc[mes] = (acc[mes] || 0) + evento.custo;
                return acc;
            }, {});

        return Object.entries(custosPorMes).map(([mes, custo]) => ({
            mes,
            custo: custo.toFixed(2)
        }));
    };

    if (loading) {
        return (
            <PageContainer title="Controle Sanitário" subtitle="Gestão de vacinações, medicações e eventos sanitários">
                <LoadingSpinner />
            </PageContainer>
        );
    }

    const eventosPorTipo = prepareEventosPorTipo();
    const custosPorMes = prepareCustosPorMes();

    return (
        <PageContainer 
            title="Controle Sanitário" 
            subtitle="Gestão completa de vacinações, medicações e eventos sanitários"
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
                        onClick={() => fetchEventosSanitarios(selectedLote?.id)} 
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
                    Nenhum lote ativo encontrado. Crie um lote para registrar eventos sanitários.
                </Alert>
            ) : (
                <>
                    {/* Resumo Sanitário */}
                    {resumoSanitario && (
                        <Grid container spacing={3} sx={{ mb: 3 }}>
                            <Grid item xs={12} md={3}>
                                <Card>
                                    <CardContent>
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                                            <AssignmentIcon color="primary" />
                                            <Typography variant="h6">Total Eventos</Typography>
                                        </Box>
                                        <Typography variant="h4" color="primary">
                                            {resumoSanitario.totalEventos || eventos.length}
                                        </Typography>
                                        <Typography variant="body2" color="text.secondary">
                                            Eventos registrados
                                        </Typography>
                                    </CardContent>
                                </Card>
                            </Grid>
                            
                            <Grid item xs={12} md={3}>
                                <Card>
                                    <CardContent>
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                                            <VaccineIcon color="success" />
                                            <Typography variant="h6">Vacinações</Typography>
                                        </Box>
                                        <Typography variant="h4" color="success.main">
                                            {eventos.filter(e => e.tipoEvento === 'Vacinacao').length}
                                        </Typography>
                                        <Typography variant="body2" color="text.secondary">
                                            Vacinas aplicadas
                                        </Typography>
                                    </CardContent>
                                </Card>
                            </Grid>
                            
                            <Grid item xs={12} md={3}>
                                <Card>
                                    <CardContent>
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                                            <MedicalIcon color="info" />
                                            <Typography variant="h6">Medicações</Typography>
                                        </Box>
                                        <Typography variant="h4" color="info.main">
                                            {eventos.filter(e => e.tipoEvento === 'Medicacao').length}
                                        </Typography>
                                        <Typography variant="body2" color="text.secondary">
                                            Tratamentos realizados
                                        </Typography>
                                    </CardContent>
                                </Card>
                            </Grid>
                            
                            <Grid item xs={12} md={3}>
                                <Card>
                                    <CardContent>
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                                            <WarningIcon color="warning" />
                                            <Typography variant="h6">Custo Total</Typography>
                                        </Box>
                                        <Typography variant="h4" color="warning.main">
                                            R$ {eventos.reduce((total, e) => total + (e.custo || 0), 0).toFixed(2)}
                                        </Typography>
                                        <Typography variant="body2" color="text.secondary">
                                            Custos sanitários
                                        </Typography>
                                    </CardContent>
                                </Card>
                            </Grid>
                        </Grid>
                    )}

                    {/* Gráficos */}
                    {eventos.length > 0 && (
                        <Grid container spacing={3} sx={{ mb: 3 }}>
                            <Grid item xs={12} md={6}>
                                <Card>
                                    <CardContent>
                                        <Typography variant="h6" gutterBottom>
                                            Distribuição por Tipo de Evento
                                        </Typography>
                                        <Box sx={{ width: '100%', height: 300 }}>
                                            <ResponsiveContainer>
                                                <PieChart>
                                                    <Pie
                                                        data={eventosPorTipo}
                                                        cx="50%"
                                                        cy="50%"
                                                        labelLine={false}
                                                        label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                                                        outerRadius={80}
                                                        fill="#8884d8"
                                                        dataKey="value"
                                                    >
                                                        {eventosPorTipo.map((entry, index) => (
                                                            <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                                                        ))}
                                                    </Pie>
                                                    <RechartsTooltip />
                                                </PieChart>
                                            </ResponsiveContainer>
                                        </Box>
                                    </CardContent>
                                </Card>
                            </Grid>
                            
                            {custosPorMes.length > 0 && (
                                <Grid item xs={12} md={6}>
                                    <Card>
                                        <CardContent>
                                            <Typography variant="h6" gutterBottom>
                                                Custos Sanitários por Mês
                                            </Typography>
                                            <Box sx={{ width: '100%', height: 300 }}>
                                                <ResponsiveContainer>
                                                    <BarChart data={custosPorMes}>
                                                        <CartesianGrid strokeDasharray="3 3" />
                                                        <XAxis dataKey="mes" />
                                                        <YAxis />
                                                        <RechartsTooltip formatter={(value) => [`R$ ${value}`, 'Custo']} />
                                                        <Bar dataKey="custo" fill="#ff9800" />
                                                    </BarChart>
                                                </ResponsiveContainer>
                                            </Box>
                                        </CardContent>
                                    </Card>
                                </Grid>
                            )}
                        </Grid>
                    )}

                    {/* Tabela de Eventos */}
                    <Card>
                        <CardContent>
                            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                                <Typography variant="h6">Histórico de Eventos Sanitários</Typography>
                                <Button
                                    variant="contained"
                                    startIcon={<AddIcon />}
                                    onClick={() => setOpen(true)}
                                >
                                    Novo Evento
                                </Button>
                            </Box>

                            {loadingData ? (
                                <LoadingSpinner />
                            ) : (
                                <TableContainer component={Paper} variant="outlined">
                                    <Table>
                                        <TableHead>
                                            <TableRow>
                                                <TableCell>Data</TableCell>
                                                <TableCell>Tipo</TableCell>
                                                <TableCell>Produto</TableCell>
                                                <TableCell>Via</TableCell>
                                                <TableCell>Aves Tratadas</TableCell>
                                                <TableCell>Responsável</TableCell>
                                                <TableCell>Custo</TableCell>
                                                <TableCell>Status</TableCell>
                                            </TableRow>
                                        </TableHead>
                                        <TableBody>
                                            {eventos.length === 0 ? (
                                                <TableRow>
                                                    <TableCell colSpan={8} align="center">
                                                        <Typography color="text.secondary">
                                                            Nenhum evento sanitário registrado
                                                        </Typography>
                                                    </TableCell>
                                                </TableRow>
                                            ) : (
                                                eventos
                                                    .sort((a, b) => new Date(b.data) - new Date(a.data))
                                                    .map((evento) => (
                                                        <TableRow key={evento.id}>
                                                            <TableCell>
                                                                {new Date(evento.data).toLocaleDateString('pt-BR')}
                                                            </TableCell>
                                                            <TableCell>
                                                                <Chip 
                                                                    label={evento.tipoEvento} 
                                                                    color={getTipoEventoColor(evento.tipoEvento)}
                                                                    size="small"
                                                                />
                                                            </TableCell>
                                                            <TableCell>{evento.produto}</TableCell>
                                                            <TableCell>{evento.viaAdministracao || '-'}</TableCell>
                                                            <TableCell>
                                                                {evento.avesTratadas?.toLocaleString() || '-'}
                                                            </TableCell>
                                                            <TableCell>{evento.responsavelAplicacao || '-'}</TableCell>
                                                            <TableCell>
                                                                {evento.custo ? `R$ ${evento.custo.toFixed(2)}` : '-'}
                                                            </TableCell>
                                                            <TableCell>
                                                                {evento.periodoCarenciaDias && evento.periodoCarenciaDias > 0 ? (
                                                                    <Chip 
                                                                        label={`Carência: ${evento.periodoCarenciaDias}d`}
                                                                        color="warning"
                                                                        size="small"
                                                                    />
                                                                ) : (
                                                                    <Chip 
                                                                        label="Liberado"
                                                                        color="success"
                                                                        size="small"
                                                                    />
                                                                )}
                                                            </TableCell>
                                                        </TableRow>
                                                    ))
                                            )}
                                        </TableBody>
                                    </Table>
                                </TableContainer>
                            )}
                        </CardContent>
                    </Card>

                    {/* Dialog para Novo Evento */}
                    <Dialog open={open} onClose={() => setOpen(false)} maxWidth="lg" fullWidth>
                        <DialogTitle>Registrar Evento Sanitário</DialogTitle>
                        <DialogContent>
                            <Grid container spacing={3} sx={{ mt: 1 }}>
                                <Grid item xs={12} md={4}>
                                    <TextField
                                        fullWidth
                                        label="Data do Evento"
                                        type="date"
                                        value={formData.data}
                                        onChange={(e) => setFormData(prev => ({ ...prev, data: e.target.value }))}
                                        InputLabelProps={{ shrink: true }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <FormControl fullWidth>
                                        <InputLabel>Tipo de Evento</InputLabel>
                                        <Select
                                            value={formData.tipoEvento}
                                            onChange={(e) => setFormData(prev => ({ ...prev, tipoEvento: e.target.value }))}
                                            label="Tipo de Evento"
                                        >
                                            {tiposEvento.map(tipo => (
                                                <MenuItem key={tipo} value={tipo}>{tipo}</MenuItem>
                                            ))}
                                        </Select>
                                    </FormControl>
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <TextField
                                        fullWidth
                                        label="Produto/Vacina/Medicamento"
                                        value={formData.produto}
                                        onChange={(e) => setFormData(prev => ({ ...prev, produto: e.target.value }))}
                                        required
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Lote do Produto"
                                        value={formData.loteProduto}
                                        onChange={(e) => setFormData(prev => ({ ...prev, loteProduto: e.target.value }))}
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Dosagem"
                                        value={formData.dosagem}
                                        onChange={(e) => setFormData(prev => ({ ...prev, dosagem: e.target.value }))}
                                        placeholder="Ex: 1ml/ave, 0.5ml, 1g/L"
                                    />
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <FormControl fullWidth>
                                        <InputLabel>Via de Administração</InputLabel>
                                        <Select
                                            value={formData.viaAdministracao}
                                            onChange={(e) => setFormData(prev => ({ ...prev, viaAdministracao: e.target.value }))}
                                            label="Via de Administração"
                                        >
                                            {viasAdministracao.map(via => (
                                                <MenuItem key={via} value={via}>{via}</MenuItem>
                                            ))}
                                        </Select>
                                    </FormControl>
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <TextField
                                        fullWidth
                                        label="Aves Tratadas"
                                        type="number"
                                        value={formData.avesTratadas}
                                        onChange={(e) => setFormData(prev => ({ ...prev, avesTratadas: e.target.value }))}
                                        inputProps={{ min: 1 }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <TextField
                                        fullWidth
                                        label="Duração do Tratamento (dias)"
                                        type="number"
                                        value={formData.duracaoTratamentoDias}
                                        onChange={(e) => setFormData(prev => ({ ...prev, duracaoTratamentoDias: e.target.value }))}
                                        inputProps={{ min: 1 }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <TextField
                                        fullWidth
                                        label="Período de Carência (dias)"
                                        type="number"
                                        value={formData.periodoCarenciaDias}
                                        onChange={(e) => setFormData(prev => ({ ...prev, periodoCarenciaDias: e.target.value }))}
                                        inputProps={{ min: 0 }}
                                    />
                                </Grid>
                                <Grid item xs={12} md={4}>
                                    <TextField
                                        fullWidth
                                        label="Custo (R$)"
                                        type="number"
                                        value={formData.custo}
                                        onChange={(e) => setFormData(prev => ({ ...prev, custo: e.target.value }))}
                                        inputProps={{ min: 0, step: 0.01 }}
                                    />
                                </Grid>
                                <Grid item xs={12}>
                                    <TextField
                                        fullWidth
                                        label="Responsável pela Aplicação"
                                        value={formData.responsavelAplicacao}
                                        onChange={(e) => setFormData(prev => ({ ...prev, responsavelAplicacao: e.target.value }))}
                                    />
                                </Grid>
                                {formData.tipoEvento === 'Doenca' && (
                                    <Grid item xs={12}>
                                        <TextField
                                            fullWidth
                                            label="Sintomas Observados"
                                            multiline
                                            rows={3}
                                            value={formData.sintomas}
                                            onChange={(e) => setFormData(prev => ({ ...prev, sintomas: e.target.value }))}
                                            placeholder="Descreva os sintomas observados nas aves..."
                                        />
                                    </Grid>
                                )}
                                <Grid item xs={12}>
                                    <TextField
                                        fullWidth
                                        label="Observações"
                                        multiline
                                        rows={3}
                                        value={formData.observacoes}
                                        onChange={(e) => setFormData(prev => ({ ...prev, observacoes: e.target.value }))}
                                        placeholder="Observações adicionais sobre o evento..."
                                    />
                                </Grid>
                            </Grid>
                        </DialogContent>
                        <DialogActions>
                            <Button onClick={() => setOpen(false)}>Cancelar</Button>
                            <Button 
                                onClick={handleSubmit} 
                                variant="contained"
                                disabled={!formData.produto}
                            >
                                Registrar Evento
                            </Button>
                        </DialogActions>
                    </Dialog>
                </>
            )}
        </PageContainer>
    );
}

export default SanitarioPage;
