import React, { useState, useEffect, useCallback, useContext } from 'react';
import { AuthContext } from '../context/AuthContext';
import apiService from '../services/apiService';
import { 
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
    Typography, Box, Button, Dialog, DialogActions, DialogContent, 
    DialogTitle, TextField, IconButton, Select, MenuItem, FormControl, InputLabel
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';

const initialLoteState = { id: 0, identificador: '', quantidadeAvesInicial: '', dataEntrada: new Date().toISOString().split('T')[0], dataSaida: null, granjaId: '' };

function LotesPage() {
    const { user } = useContext(AuthContext);
    const [lotes, setLotes] = useState([]);
    const [granjas, setGranjas] = useState([]);
    const [open, setOpen] = useState(false);
    const [isEditMode, setIsEditMode] = useState(false);
    const [currentLote, setCurrentLote] = useState(initialLoteState);

    const fetchData = useCallback(async () => {
        try {
            const [lotesResponse, granjasResponse] = await Promise.all([
                apiService.getLotes(),
                apiService.getGranjas()
            ]);
            setLotes(lotesResponse.data);
            setGranjas(granjasResponse.data);
        } catch (error) {
            console.error("Houve um erro ao buscar os dados:", error);
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
    
    const handleClose = () => setOpen(false);

    const handleInputChange = (event) => {
        const { name, value } = event.target;
        setCurrentLote({ ...currentLote, [name]: value });
    };

    const handleSubmit = async (event) => {
        event.preventDefault();
        if (currentLote.dataSaida && new Date(currentLote.dataSaida) < new Date(currentLote.dataEntrada)) {
            alert('Erro: A data de saída não pode ser anterior à data de entrada.');
            return;
        }

        try {
            if (isEditMode) {
                const loteParaEnviar = {
                    identificador: currentLote.identificador,
                    quantidadeAvesInicial: parseInt(currentLote.quantidadeAvesInicial, 10),
                    dataEntrada: new Date(`${currentLote.dataEntrada}T00:00:00.000Z`).toISOString(),
                    dataSaida: currentLote.dataSaida ? new Date(`${currentLote.dataSaida}T00:00:00.000Z`).toISOString() : null,
                    granjaId: currentLote.granjaId
                };
                await apiService.updateLote(currentLote.id, loteParaEnviar);
            } else {
                const loteParaEnviar = { 
                    ...currentLote, 
                    quantidadeAvesInicial: parseInt(currentLote.quantidadeAvesInicial, 10),
                    dataEntrada: new Date(`${currentLote.dataEntrada}T00:00:00.000Z`).toISOString(),
                    dataSaida: currentLote.dataSaida ? new Date(`${currentLote.dataSaida}T00:00:00.000Z`).toISOString() : null 
                };
                const { id, ...loteFinal } = loteParaEnviar;
                await apiService.createLote(loteFinal);
            }
            handleClose();
            fetchData();
        } catch (error) {
            console.error("Houve um erro ao salvar o lote:", error);
            if (error.response) {
                alert(`Erro do servidor: ${JSON.stringify(error.response.data)}`);
            }
        }
    };

    const handleDelete = async (id) => {
        if (window.confirm('Tem certeza que deseja excluir este lote?')) {
            try {
                await apiService.deleteLote(id);
                fetchData();
            } catch (error) {
                console.error("Houve um erro ao excluir o lote:", error);
            }
        }
    };

    return (
        <Box sx={{ margin: '20px', padding: '20px' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h4" component="h1">
                    Gerenciamento de Lotes
                </Typography>
                {user?.role !== 'Financeiro' && (
                    <Button variant="contained" onClick={() => handleClickOpen()}>
                        Adicionar Novo Lote
                    </Button>
                )}
            </Box>

            <Dialog open={open} onClose={handleClose}>
                <form onSubmit={handleSubmit}>
                    <DialogTitle>{isEditMode ? 'Editar Lote' : 'Cadastrar Novo Lote'}</DialogTitle>
                    <DialogContent>
                        <TextField autoFocus margin="dense" name="identificador" label="Identificador do Lote" type="text" fullWidth variant="standard" value={currentLote.identificador} onChange={handleInputChange}/>
                        <TextField margin="dense" name="quantidadeAvesInicial" label="Quantidade de Aves" type="number" fullWidth variant="standard" value={currentLote.quantidadeAvesInicial} onChange={handleInputChange}/>
                        <TextField margin="dense" name="dataEntrada" label="Data de Entrada" type="date" fullWidth variant="standard" InputLabelProps={{ shrink: true }} value={currentLote.dataEntrada} onChange={handleInputChange}/>
                        <TextField margin="dense" name="dataSaida" label="Data de Saída (opcional)" type="date" fullWidth variant="standard" InputLabelProps={{ shrink: true }} value={currentLote.dataSaida || ''} onChange={handleInputChange}/>
                        <FormControl fullWidth margin="dense" variant="standard">
                            <InputLabel id="granja-select-label">Granja</InputLabel>
                            <Select labelId="granja-select-label" name="granjaId" value={currentLote.granjaId} onChange={handleInputChange}>
                                {granjas.map((granja) => (
                                    <MenuItem key={granja.id} value={granja.id}>
                                        {granja.nome} ({granja.codigo})
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                    </DialogContent>
                    <DialogActions>
                        <Button onClick={handleClose}>Cancelar</Button>
                        <Button type="submit">Salvar</Button>
                    </DialogActions>
                </form>
            </Dialog>

            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Código</TableCell>
                            <TableCell>Identificador</TableCell>
                            <TableCell>Granja</TableCell>
                            {/* COLUNA CONDICIONAL PARA O DONO */}
                            {(user?.role === 'Administrador' || user?.role === 'Financeiro') && <TableCell>Dono (Produtor)</TableCell>}
                            <TableCell>Aves Iniciais</TableCell>
                            <TableCell>Data de Entrada</TableCell>
                            <TableCell>Data de Saída</TableCell>
                            {user?.role !== 'Financeiro' && <TableCell align="right">Ações</TableCell>}
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {lotes.map((lote) => (
                            <TableRow key={lote.id}>
                                <TableCell>{lote.codigo}</TableCell>
                                <TableCell>{lote.identificador}</TableCell>
                                <TableCell>{lote.granja?.nome || 'N/A'}</TableCell>
                                {/* CÉLULA CONDICIONAL PARA O DONO */}
                                {(user?.role === 'Administrador' || user?.role === 'Financeiro') && <TableCell>{lote.granja?.usuario?.nome || 'N/A'}</TableCell>}
                                <TableCell>{lote.quantidadeAvesInicial}</TableCell>
                                <TableCell>{new Date(lote.dataEntrada).toLocaleDateString('pt-BR', { timeZone: 'UTC' })}</TableCell>
                                <TableCell>
                                    {lote.dataSaida ? new Date(lote.dataSaida).toLocaleDateString('pt-BR', { timeZone: 'UTC' }) : '-'}
                                </TableCell>
                                {user?.role !== 'Financeiro' && (
                                    <TableCell align="right">
                                        <IconButton onClick={() => handleClickOpen(lote)}><EditIcon /></IconButton>
                                        <IconButton onClick={() => handleDelete(lote.id)}><DeleteIcon /></IconButton>
                                    </TableCell>
                                )}
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </Box>
    );
}

export default LotesPage;
