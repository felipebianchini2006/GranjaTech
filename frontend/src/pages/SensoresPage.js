import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';
import { 
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    Typography, Box, Button, Dialog, DialogActions, DialogContent, 
    DialogTitle, TextField, Select, MenuItem, FormControl, InputLabel, IconButton, CircularProgress, Tooltip
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import RefreshIcon from '@mui/icons-material/Refresh';
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

    const fetchData = useCallback(async () => {
        try {
            const [sensoresRes, granjasRes] = await Promise.all([
                apiService.getSensores(),
                apiService.getGranjas()
            ]);
            setSensores(sensoresRes.data);
            setGranjas(granjasRes.data);
        } catch (error) {
            console.error("Erro ao buscar dados:", error);
        }
    }, []);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    const fetchLeituras = useCallback(async (sensor) => {
        if (!sensor) return;
        setLoadingLeituras(true);
        try {
            const leiturasRes = await apiService.getLeituras(sensor.id);
            setLeituras(leiturasRes.data.reverse());
        } catch (error) {
            console.error(`Erro ao buscar leituras do sensor ${sensor.id}:`, error);
            setLeituras([]);
        } finally {
            setLoadingLeituras(false);
        }
    }, []);

    useEffect(() => {
        if (!selectedSensor) return;
        const intervalId = setInterval(() => fetchLeituras(selectedSensor), 10000);
        return () => clearInterval(intervalId);
    }, [selectedSensor, fetchLeituras]);

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
            await apiService.createSensor(formData);
            handleClose();
            fetchData();
        } catch (error) {
            console.error("Erro ao salvar sensor:", error);
            alert(error.response?.data?.message || "Falha ao salvar sensor.");
        }
    };

    const handleDelete = async (id) => {
        if (window.confirm('Tem certeza que deseja excluir este sensor? Todas as suas leituras serão perdidas.')) {
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

    const handleSensorSelect = (sensor) => {
        // Se clicar no mesmo sensor, deseleciona-o
        if (selectedSensor?.id === sensor.id) {
            setSelectedSensor(null);
            setLeituras([]);
        } else {
            setSelectedSensor(sensor);
            fetchLeituras(sensor);
        }
    };

    return (
        <Box sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h4" component="h1">
                    Gestão de Sensores
                </Typography>
                <Button variant="contained" onClick={handleOpen}>
                    Adicionar Sensor
                </Button>
            </Box>

            <Dialog open={open} onClose={handleClose}>
                <DialogTitle>Adicionar Novo Sensor</DialogTitle>
                <DialogContent>
                    <TextField autoFocus margin="dense" name="identificadorUnico" label="Identificador Único (ex: TEMP-GRJ001-SALA1)" type="text" fullWidth variant="standard" value={formData.identificadorUnico} onChange={handleInputChange} />
                    <FormControl fullWidth margin="dense" variant="standard" required>
                        <InputLabel>Tipo</InputLabel>
                        <Select name="tipo" value={formData.tipo} onChange={handleInputChange}>
                            <MenuItem value="Temperatura">Temperatura</MenuItem>
                            <MenuItem value="Humidade">Humidade</MenuItem>
                        </Select>
                    </FormControl>
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

            <Box sx={{ mb: 3 }}>
                <Typography variant="h6" gutterBottom>Sensores Registados (Clique num sensor para ver as leituras)</Typography>
                <TableContainer component={Paper}>
                    <Table size="small">
                        <TableHead>
                            <TableRow>
                                <TableCell>Identificador</TableCell>
                                <TableCell>Granja</TableCell>
                                <TableCell align="right">Ações</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {sensores.map((sensor) => (
                                <TableRow 
                                    key={sensor.id} 
                                    hover 
                                    onClick={() => handleSensorSelect(sensor)} 
                                    style={{ cursor: 'pointer' }}
                                    selected={selectedSensor?.id === sensor.id}
                                >
                                    <TableCell component="th" scope="row">
                                        <Typography variant="body2">{sensor.identificadorUnico}</Typography>
                                        <Typography variant="caption" color="textSecondary">{sensor.tipo}</Typography>
                                    </TableCell>
                                    <TableCell>{sensor.granja?.nome || 'N/A'}</TableCell>
                                    <TableCell align="right">
                                        <IconButton size="small" onClick={(e) => { e.stopPropagation(); handleDelete(sensor.id); }}><DeleteIcon /></IconButton>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Box>
            
            {selectedSensor && (
                <Box sx={{ width: '100%' }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                        <Typography variant="h6">
                            {`Leituras - ${selectedSensor.identificadorUnico}`}
                        </Typography>
                        <Tooltip title="Atualizar Leituras">
                            <IconButton onClick={() => fetchLeituras(selectedSensor)} size="small">
                                <RefreshIcon />
                            </IconButton>
                        </Tooltip>
                    </Box>
                    <Paper sx={{ 
                        p: 2, 
                        height: '400px',
                        width: '100%', 
                        display: 'flex', 
                        alignItems: 'center', 
                        justifyContent: 'center' 
                    }}>
                        {loadingLeituras ? (
                            <CircularProgress />
                        ) : leituras.length > 0 ? (
                            <ResponsiveContainer width="100%" height="100%">
                                <LineChart data={leituras} margin={{ top: 20, right: 30, left: 20, bottom: 60 }}>
                                    <CartesianGrid strokeDasharray="3 3" />
                                    <XAxis dataKey="timestamp" tickFormatter={(time) => new Date(time).toLocaleTimeString('pt-BR')} />
                                    <YAxis />
                                    <RechartsTooltip labelFormatter={(time) => new Date(time).toLocaleString('pt-BR')} />
                                    <Legend />
                                    <Line type="monotone" dataKey="valor" name={selectedSensor.tipo} stroke="#8884d8" activeDot={{ r: 8 }} />
                                </LineChart>
                            </ResponsiveContainer>
                        ) : (
                            <Typography>Nenhuma leitura registada para este sensor.</Typography>
                        )}
                    </Paper>
                </Box>
            )}
        </Box>
    );
}

export default SensoresPage;