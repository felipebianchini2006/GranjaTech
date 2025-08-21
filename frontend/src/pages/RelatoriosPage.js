import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';
import { jsPDF } from 'jspdf';
import 'jspdf-autotable';
import * as XLSX from 'xlsx';

import { 
    Typography, Box, Paper, Grid, TextField, Button, Select, 
    MenuItem, FormControl, InputLabel, CircularProgress,
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow
} from '@mui/material';
import DownloadIcon from '@mui/icons-material/Download';

function RelatoriosPage() {
    const [reportType, setReportType] = useState('financeiro');
    const [filters, setFilters] = useState({
        dataInicio: '',
        dataFim: '',
        granjaId: ''
    });
    const [granjas, setGranjas] = useState([]);
    const [reportData, setReportData] = useState(null);
    const [loading, setLoading] = useState(false);

    const fetchGranjas = useCallback(async () => {
        try {
            const res = await apiService.getGranjas();
            setGranjas(res.data);
        } catch (error) {
            console.error("Erro ao buscar granjas:", error);
        }
    }, []);

    useEffect(() => {
        fetchGranjas();
    }, [fetchGranjas]);

    const handleFilterChange = (e) => {
        setFilters({ ...filters, [e.target.name]: e.target.value });
    };

    const handleGenerateReport = async () => {
        if (!filters.dataInicio || !filters.dataFim) {
            alert("Por favor, selecione a data de início e a data de fim.");
            return;
        }
        setLoading(true);
        setReportData(null);
        try {
            const params = {
                dataInicio: filters.dataInicio,
                dataFim: filters.dataFim,
                granjaId: filters.granjaId || null
            };
            const response = reportType === 'financeiro'
                ? await apiService.getRelatorioFinanceiro(params)
                : await apiService.getRelatorioProducao(params);
            setReportData(response.data);
        } catch (error) {
            console.error("Erro ao gerar relatório:", error);
            alert("Falha ao gerar relatório.");
        } finally {
            setLoading(false);
        }
    };

    const exportToExcel = () => {
        if (!reportData) return;
        const dataToExport = reportType === 'financeiro' ? reportData.transacoes : reportData.lotes;
        if (dataToExport.length === 0) {
            alert("Não há dados para exportar.");
            return;
        }
        const worksheet = XLSX.utils.json_to_sheet(dataToExport);
        const workbook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(workbook, worksheet, "Relatorio");
        XLSX.writeFile(workbook, `Relatorio_${reportType}_${new Date().toLocaleDateString('pt-BR')}.xlsx`);
    };

    const exportToPdf = () => {
        if (!reportData) return;
        const dataToExport = reportType === 'financeiro' ? reportData.transacoes : reportData.lotes;
        if (dataToExport.length === 0) {
            alert("Não há dados para exportar.");
            return;
        }
        const doc = new jsPDF();
        doc.text(`Relatório ${reportType === 'financeiro' ? 'Financeiro' : 'de Produção'}`, 14, 16);
        
        const head = reportType === 'financeiro' 
            ? [['Data', 'Descrição', 'Tipo', 'Valor', 'Lote', 'Registado Por']]
            : [['Código', 'Identificador', 'Aves', 'Data Entrada', 'Data Saída']];
            
        const body = reportType === 'financeiro'
            ? dataToExport.map(t => [new Date(t.data).toLocaleDateString('pt-BR'), t.descricao, t.tipo, t.valor.toFixed(2), t.lote?.identificador || '-', t.usuario?.nome || '-'])
            : dataToExport.map(l => [l.codigo, l.identificador, l.quantidadeAvesInicial, new Date(l.dataEntrada).toLocaleDateString('pt-BR'), l.dataSaida ? new Date(l.dataSaida).toLocaleDateString('pt-BR') : '-']);

        doc.autoTable({ head, body, startY: 25 });
        doc.save(`Relatorio_${reportType}_${new Date().toLocaleDateString('pt-BR')}.pdf`);
    };

    return (
        <Box sx={{ p: 3 }}>
            <Typography variant="h4" gutterBottom>Geração de Relatórios</Typography>
            
            <Paper sx={{ p: 2, mb: 3 }}>
                <Grid container spacing={2} alignItems="center">
                    <Grid item xs={12} sm={3}>
                        <FormControl fullWidth>
                            <InputLabel>Tipo de Relatório</InputLabel>
                            <Select value={reportType} onChange={(e) => setReportType(e.target.value)}>
                                <MenuItem value="financeiro">Financeiro</MenuItem>
                                <MenuItem value="producao">Produção</MenuItem>
                            </Select>
                        </FormControl>
                    </Grid>
                    <Grid item xs={12} sm={3}>
                        <TextField name="dataInicio" label="Data de Início" type="date" fullWidth InputLabelProps={{ shrink: true }} value={filters.dataInicio} onChange={handleFilterChange} />
                    </Grid>
                    <Grid item xs={12} sm={3}>
                        <TextField name="dataFim" label="Data de Fim" type="date" fullWidth InputLabelProps={{ shrink: true }} value={filters.dataFim} onChange={handleFilterChange} />
                    </Grid>
                    <Grid item xs={12} sm={3}>
                        <FormControl fullWidth>
                            <InputLabel>Granja (Opcional)</InputLabel>
                            <Select name="granjaId" value={filters.granjaId} onChange={handleFilterChange}>
                                <MenuItem value="">Todas</MenuItem>
                                {granjas.map(g => <MenuItem key={g.id} value={g.id}>{g.nome}</MenuItem>)}
                            </Select>
                        </FormControl>
                    </Grid>
                    <Grid item xs={12}>
                        <Button variant="contained" onClick={handleGenerateReport} disabled={loading}>
                            {loading ? <CircularProgress size={24} /> : "Gerar Relatório"}
                        </Button>
                    </Grid>
                </Grid>
            </Paper>

            {reportData && (
                <Paper sx={{ p: 2 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                        <Typography variant="h6">Resultados</Typography>
                        <Box>
                            <Button startIcon={<DownloadIcon />} onClick={exportToExcel} sx={{ mr: 1 }}>Excel</Button>
                            <Button startIcon={<DownloadIcon />} onClick={exportToPdf}>PDF</Button>
                        </Box>
                    </Box>
                    
                    {reportType === 'financeiro' ? (
                        <TableContainer>
                            <Table>
                                <TableHead>
                                    <TableRow>
                                        <TableCell>Data</TableCell>
                                        <TableCell>Descrição</TableCell>
                                        <TableCell>Tipo</TableCell>
                                        <TableCell align="right">Valor</TableCell>
                                        <TableCell>Lote</TableCell>
                                        <TableCell>Registado Por</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {reportData.transacoes.map(t => (
                                        <TableRow key={t.id}>
                                            <TableCell>{new Date(t.data).toLocaleDateString('pt-BR')}</TableCell>
                                            <TableCell>{t.descricao}</TableCell>
                                            <TableCell>{t.tipo}</TableCell>
                                            <TableCell align="right">{t.valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</TableCell>
                                            <TableCell>{t.lote?.identificador || '-'}</TableCell>
                                            <TableCell>{t.usuario?.nome || '-'}</TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    ) : (
                        <TableContainer>
                            <Table>
                                <TableHead>
                                    <TableRow>
                                        <TableCell>Código</TableCell>
                                        <TableCell>Identificador</TableCell>
                                        <TableCell>Aves Iniciais</TableCell>
                                        <TableCell>Data Entrada</TableCell>
                                        <TableCell>Data Saída</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {reportData.lotes.map(l => (
                                        <TableRow key={l.id}>
                                            <TableCell>{l.codigo}</TableCell>
                                            <TableCell>{l.identificador}</TableCell>
                                            <TableCell>{l.quantidadeAvesInicial}</TableCell>
                                            <TableCell>{new Date(l.dataEntrada).toLocaleDateString('pt-BR')}</TableCell>
                                            <TableCell>{l.dataSaida ? new Date(l.dataSaida).toLocaleDateString('pt-BR') : '-'}</TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    )}
                </Paper>
            )}
        </Box>
    );
}

export default RelatoriosPage;
