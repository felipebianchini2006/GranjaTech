import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';
import PageContainer from '../components/PageContainer';
import LoadingSpinner from '../components/LoadingSpinner';
import { 
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    Typography, Box, Button, Dialog, DialogActions, DialogContent, 
    DialogTitle, TextField, Select, MenuItem, FormControl, InputLabel, IconButton, 
    CircularProgress, Tooltip, Card, CardContent, Alert, Grid
} from '@mui/material';
import {
    Delete as DeleteIcon,
    Refresh as RefreshIcon,
    Add as AddIcon,
    Sensors as SensorsIcon,
    Agriculture as AgricultureIcon,
    Thermostat as ThermostatIcon,
    Opacity as OpacityIcon,
    DeviceHub as DeviceHubIcon
} from '@mui/icons-material';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, Legend, ResponsiveContainer } from 'recharts';

const initialFormState = {
    tipo: '', identificadorUnico: '', granjaId: ''
};

function SensoresPage() {
    const [sensores, setSensores] = useState([]);
    const [granjas, setGranjas] = useState([]);
    const [leituras, setLeituras] = useState([]);
    const [selectedSensor, setSelectedSensor] = useState(null);
    const [loadingLeituras, setLoadingLeituras] = useState(false);
    const [open, setOpen] = useState(false);
    const [formData, setFormData] = useState(initialFormState);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [autoRefresh, setAutoRefresh] = useState(true);
    const [lastUpdate, setLastUpdate] = useState(null);

    const fetchData = useCallback(async () => {
        try {
            setLoading(true);
            setError('');
            const [sensoresRes, granjasRes] = await Promise.all([
                apiService.getSensores(),
                apiService.getGranjas()
            ]);
            setSensores(sensoresRes.data);
            setGranjas(granjasRes.data);
        } catch (error) {
            console.error("Erro ao buscar dados de sensores:", error);
            setError('Erro ao carregar sensores. Tente novamente.');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    // Auto-refresh das leituras quando um sensor está selecionado
    useEffect(() => {
        let interval;
        if (selectedSensor && autoRefresh) {
            interval = setInterval(() => {
                console.log('Auto-refresh: atualizando leituras...');
                fetchLeituras(selectedSensor.id);
            }, 30000); // 30 segundos
        }
        return () => {
            if (interval) {
                clearInterval(interval);
            }
        };
    }, [selectedSensor, autoRefresh]);

    const handleOpen = () => {
        setFormData(initialFormState);
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
            const sensorParaEnviar = {
                ...formData,
                granjaId: parseInt(formData.granjaId, 10)
            };

            await apiService.createSensor(sensorParaEnviar);
            handleClose();
            fetchData();
        } catch (error) {
            console.error("Erro ao criar sensor:", error);
            alert("Falha ao criar sensor.");
        }
    };

    const handleDelete = async (id) => {
        if (window.confirm('Tem certeza que deseja excluir este sensor?')) {
            try {
                await apiService.deleteSensor(id);
                fetchData();
                if (selectedSensor?.id === id) {
                    setSelectedSensor(null);
                    setLeituras([]);
                }
            } catch (error) {
                console.error("Erro ao excluir sensor:", error);
                alert("Falha ao excluir sensor.");
            }
        }
    };

    const fetchLeituras = async (sensorId) => {
        setLoadingLeituras(true);
        setError('');
        try {
            console.log('Buscando leituras para sensor ID:', sensorId);
            const response = await apiService.getLeituras(sensorId);
            console.log('Resposta da API:', response);
            
            const leiturасData = response.data || [];
            console.log('Dados das leituras:', leiturасData);
            
            // Garantir que os dados estão formatados corretamente para o gráfico
            const formattedLeituras = leiturасData.map(leitura => ({
                ...leitura,
                timestamp: leitura.timestamp || leitura.dataHora || new Date().toISOString(),
                valor: parseFloat(leitura.valor) || 0
            })).sort((a, b) => new Date(a.timestamp) - new Date(b.timestamp)); // Ordenar por timestamp
            
            setLeituras(formattedLeituras);
            setLastUpdate(new Date());
            console.log('Leituras formatadas e ordenadas:', formattedLeituras);
        } catch (error) {
            console.error("Erro ao buscar leituras:", error);
            setError('Erro ao carregar leituras do sensor. Tente novamente.');
            setLeituras([]);
        } finally {
            setLoadingLeituras(false);
        }
    };

    const handleSensorClick = (sensor) => {
        setSelectedSensor(sensor);
        setLeituras([]);
        setError('');
        setLastUpdate(null);
        fetchLeituras(sensor.id);
    };

    const getSensorIcon = (tipo) => {
        switch (tipo?.toLowerCase()) {
            case 'temperatura': return ThermostatIcon;
            case 'umidade': return OpacityIcon;
            default: return SensorsIcon;
        }
    };

    const getSensorColor = (tipo) => {
        switch (tipo?.toLowerCase()) {
            case 'temperatura': return 'error';
            case 'umidade': return 'info';
            default: return 'primary';
        }
    };

    if (loading) {
        return <LoadingSpinner message="Carregando sensores..." />;
    }

    return (
        <PageContainer
            title="Sensores"
            subtitle="Monitore sensores IoT e suas leituras"
            action={
                <Button 
                    variant="contained" 
                    startIcon={<AddIcon />}
                    onClick={handleOpen}
                    sx={{ 
                        borderRadius: 2,
                        px: 3,
                        py: 1.5,
                    }}
                >
                    Novo Sensor
                </Button>
            }
        >
            {error && (
                <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError('')}>
                    {error}
                </Alert>
            )}

            <Grid container spacing={3}>
                <Grid item xs={12} md={selectedSensor ? 6 : 12}>
                    <Card>
                        <CardContent>
                            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                                <Typography variant="h6" sx={{ fontWeight: 600 }}>
                                    Lista de Sensores
                                </Typography>
                                <Button
                                    startIcon={<RefreshIcon />}
                                    onClick={fetchData}
                                    size="small"
                                    variant="outlined"
                                >
                                    Atualizar
                                </Button>
                            </Box>
                            
                            <TableContainer>
                                <Table size="small">
                                    <TableHead>
                                        <TableRow>
                                            <TableCell sx={{ fontWeight: 600 }}>Tipo</TableCell>
                                            <TableCell sx={{ fontWeight: 600 }}>Identificador</TableCell>
                                            <TableCell sx={{ fontWeight: 600 }}>Granja</TableCell>
                                            <TableCell align="right" sx={{ fontWeight: 600 }}>Ações</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {sensores.length === 0 ? (
                                            <TableRow>
                                                <TableCell colSpan={4} align="center" sx={{ py: 4 }}>
                                                    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
                                                        <SensorsIcon sx={{ fontSize: 32, color: 'text.secondary' }} />
                                                        <Typography color="text.secondary" variant="body2">
                                                            Nenhum sensor cadastrado
                                                        </Typography>
                                                    </Box>
                                                </TableCell>
                                            </TableRow>
                                        ) : (
                                            sensores.map((sensor) => {
                                                const Icon = getSensorIcon(sensor.tipo);
                                                const isSelected = selectedSensor?.id === sensor.id;
                                                
                                                return (
                                                    <TableRow 
                                                        key={sensor.id}
                                                        onClick={() => handleSensorClick(sensor)}
                                                        sx={{ 
                                                            cursor: 'pointer',
                                                            backgroundColor: isSelected ? 'action.selected' : 'transparent',
                                                            '&:hover': { backgroundColor: 'action.hover' },
                                                            transition: 'background-color 0.2s ease-in-out',
                                                        }}
                                                    >
                                                        <TableCell>
                                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                                                                <Icon color={getSensorColor(sensor.tipo)} sx={{ fontSize: 20 }} />
                                                                <Typography variant="body2" fontWeight={500}>
                                                                    {sensor.tipo}
                                                                </Typography>
                                                            </Box>
                                                        </TableCell>
                                                        <TableCell>
                                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                                <DeviceHubIcon color="action" sx={{ fontSize: 16 }} />
                                                                <Typography variant="body2">
                                                                    {sensor.identificadorUnico}
                                                                </Typography>
                                                            </Box>
                                                        </TableCell>
                                                        <TableCell>
                                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                                <AgricultureIcon color="action" sx={{ fontSize: 16 }} />
                                                                <Typography variant="body2">
                                                                    {sensor.granja?.nome || 'N/A'}
                                                                </Typography>
                                                            </Box>
                                                        </TableCell>
                                                        <TableCell align="right">
                                                            <IconButton 
                                                                onClick={(e) => {
                                                                    e.stopPropagation();
                                                                    handleDelete(sensor.id);
                                                                }}
                                                                size="small"
                                                                color="error"
                                                            >
                                                                <DeleteIcon fontSize="small" />
                                                            </IconButton>
                                                        </TableCell>
                                                    </TableRow>
                                                );
                                            })
                                        )}
                                    </TableBody>
                                </Table>
                            </TableContainer>
                        </CardContent>
                    </Card>
                </Grid>

                {selectedSensor && (
                    <Grid item xs={12} md={6}>
                        <Card>
                            <CardContent>
                                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
                                    <Box>
                                        <Typography variant="h6" sx={{ fontWeight: 600 }}>
                                            Leituras do Sensor
                                        </Typography>
                                        <Typography variant="body2" color="text.secondary">
                                            {selectedSensor.identificadorUnico} - {selectedSensor.tipo}
                                        </Typography>
                                        {autoRefresh && (
                                            <Typography variant="caption" color="success.main" sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                                                🔄 Auto-atualização ativa (30s)
                                            </Typography>
                                        )}
                                        {lastUpdate && (
                                            <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5 }}>
                                                Última atualização: {lastUpdate.toLocaleTimeString('pt-BR')}
                                            </Typography>
                                        )}
                                    </Box>
                                    <Box sx={{ display: 'flex', gap: 1 }}>
                                        <Button
                                            size="small"
                                            variant={autoRefresh ? "contained" : "outlined"}
                                            color={autoRefresh ? "success" : "primary"}
                                            onClick={() => setAutoRefresh(!autoRefresh)}
                                        >
                                            {autoRefresh ? '🔄 ON' : '⏸️ OFF'}
                                        </Button>
                                        <Button
                                            startIcon={loadingLeituras ? <CircularProgress size={16} /> : <RefreshIcon />}
                                            onClick={() => fetchLeituras(selectedSensor.id)}
                                            size="small"
                                            variant="outlined"
                                            disabled={loadingLeituras}
                                        >
                                            {loadingLeituras ? 'Carregando...' : 'Atualizar'}
                                        </Button>
                                    </Box>
                                </Box>

                                {error && (
                                    <Alert severity="error" sx={{ mb: 2 }}>
                                        {error}
                                    </Alert>
                                )}

                                {loadingLeituras ? (
                                    <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                                        <CircularProgress />
                                    </Box>
                                ) : !leituras || leituras.length === 0 ? (
                                    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', py: 4, gap: 2 }}>
                                        <SensorsIcon sx={{ fontSize: 32, color: 'text.secondary' }} />
                                        <Typography color="text.secondary" variant="body2">
                                            Nenhuma leitura encontrada para este sensor
                                        </Typography>
                                        <Typography color="text.secondary" variant="caption">
                                            Clique em "Atualizar" para verificar novamente
                                        </Typography>
                                    </Box>
                                ) : (
                                    <Box sx={{ width: '100%', height: 300 }}>
                                        <ResponsiveContainer width="100%" height="100%">
                                            <LineChart 
                                                data={leituras}
                                                margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                                            >
                                                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                                                <XAxis 
                                                    dataKey="timestamp" 
                                                    tick={{ fontSize: 10 }}
                                                    tickFormatter={(value) => {
                                                        try {
                                                            return new Date(value).toLocaleTimeString('pt-BR', { 
                                                                hour: '2-digit', 
                                                                minute: '2-digit' 
                                                            });
                                                        } catch (e) {
                                                            return value;
                                                        }
                                                    }}
                                                />
                                                <YAxis 
                                                    tick={{ fontSize: 10 }}
                                                    domain={['dataMin - 1', 'dataMax + 1']}
                                                />
                                                <RechartsTooltip 
                                                    labelFormatter={(value) => {
                                                        try {
                                                            return new Date(value).toLocaleString('pt-BR');
                                                        } catch (e) {
                                                            return value;
                                                        }
                                                    }}
                                                    formatter={(value, name) => [
                                                        `${value} ${selectedSensor?.tipo === 'Temperatura' ? '°C' : '%'}`,
                                                        name
                                                    ]}
                                                    contentStyle={{ 
                                                        backgroundColor: '#fff',
                                                        border: '1px solid #e0e0e0',
                                                        borderRadius: '8px',
                                                        boxShadow: '0px 4px 12px rgba(0,0,0,0.1)'
                                                    }}
                                                />
                                                <Legend />
                                                <Line 
                                                    type="monotone" 
                                                    dataKey="valor" 
                                                    stroke="#2E7D32" 
                                                    strokeWidth={2}
                                                    dot={{ fill: '#2E7D32', strokeWidth: 2, r: 4 }}
                                                    activeDot={{ r: 6, fill: '#2E7D32' }}
                                                    name={selectedSensor ? `${selectedSensor.tipo} (${selectedSensor.tipo === 'Temperatura' ? '°C' : '%'})` : 'Sensor'}
                                                    connectNulls={false}
                                                />
                                            </LineChart>
                                        </ResponsiveContainer>
                                    </Box>
                                )}
                            </CardContent>
                        </Card>
                    </Grid>
                )}
            </Grid>

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
                        <SensorsIcon color="primary" />
                        <Typography variant="h6" component="div">
                            Novo Sensor
                        </Typography>
                    </Box>
                </DialogTitle>
                <DialogContent sx={{ pt: 2 }}>
                    <FormControl fullWidth margin="normal" sx={{ mb: 2 }}>
                        <InputLabel>Tipo de Sensor</InputLabel>
                        <Select name="tipo" value={formData.tipo} onChange={handleInputChange}>
                            <MenuItem value="Temperatura">Temperatura</MenuItem>
                            <MenuItem value="Umidade">Umidade</MenuItem>
                            <MenuItem value="Pressão">Pressão</MenuItem>
                            <MenuItem value="pH">pH</MenuItem>
                            <MenuItem value="Outros">Outros</MenuItem>
                        </Select>
                    </FormControl>
                    <TextField 
                        margin="normal" 
                        name="identificadorUnico" 
                        label="Identificador Único" 
                        fullWidth 
                        value={formData.identificadorUnico} 
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
                        Criar
                    </Button>
                </DialogActions>
            </Dialog>
        </PageContainer>
    );
}

export default SensoresPage;