import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import * as XLSX from 'xlsx';
import PageContainer from '../components/PageContainer';
import LoadingSpinner from '../components/LoadingSpinner';

import { 
    Typography, Box, Paper, Grid, TextField, Button, Select, 
    MenuItem, FormControl, InputLabel, CircularProgress,
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
    Alert, Card, CardContent, Chip
} from '@mui/material';
import {
    Download as DownloadIcon,
    Assessment as AssessmentIcon,
    PictureAsPdf as PdfIcon,
    TableChart as ExcelIcon,
    FilterList as FilterIcon,
    AttachMoney as MoneyIcon,
    Agriculture as AgricultureIcon
} from '@mui/icons-material';

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
    const [error, setError] = useState('');

    const fetchGranjas = useCallback(async () => {
        try {
            const res = await apiService.getGranjas();
            setGranjas(res.data || []);
        } catch (error) {
            console.error("Erro ao buscar granjas:", error);
            setError("Erro ao carregar granjas");
        }
    }, []);

    useEffect(() => {
        fetchGranjas();
    }, [fetchGranjas]);

    const handleFilterChange = (e) => {
        setFilters({ ...filters, [e.target.name]: e.target.value });
        setError('');
    };

    const handleGenerateReport = async () => {
        if (!filters.dataInicio || !filters.dataFim) {
            setError("Por favor, selecione a data de início e a data de fim.");
            return;
        }

        const dataInicio = new Date(filters.dataInicio);
        const dataFim = new Date(filters.dataFim);
        
        if (dataInicio > dataFim) {
            setError("A data de início não pode ser posterior à data de fim.");
            return;
        }

        const diasDiferenca = Math.ceil((dataFim - dataInicio) / (1000 * 60 * 60 * 24));
        if (diasDiferenca > 365) {
            setError("O período do relatório não pode exceder 365 dias.");
            return;
        }

        setLoading(true);
        setReportData(null);
        setError('');
        
        try {
            const params = {
                dataInicio: filters.dataInicio,
                dataFim: filters.dataFim
            };
            
            if (filters.granjaId && filters.granjaId !== '') {
                params.granjaId = parseInt(filters.granjaId);
            }

            const response = reportType === 'financeiro'
                ? await apiService.getRelatorioFinanceiro(params)
                : await apiService.getRelatorioProducao(params);

            if (response && response.data) {
                setReportData(response.data);
                const hasData = reportType === 'financeiro' 
                    ? response.data.transacoes && response.data.transacoes.length > 0
                    : response.data.lotes && response.data.lotes.length > 0;
                
                if (!hasData) {
                    setError("Nenhum dado encontrado para o período selecionado.");
                }
            } else {
                setError("Resposta inválida do servidor.");
            }
        } catch (error) {
            let errorMessage = "Falha ao gerar relatório.";
            if (error.response) {
                errorMessage = error.response.data?.message || `Erro do servidor: ${error.response.status}`;
            }
            setError(errorMessage);
        } finally {
            setLoading(false);
        }
    };

    const exportToExcel = () => {
        if (!reportData) return;
        const dataToExport = reportType === 'financeiro' ? reportData.transacoes : reportData.lotes;
        if (dataToExport.length === 0) {
            setError("Não há dados para exportar.");
            return;
        }
        const worksheet = XLSX.utils.json_to_sheet(dataToExport);
        const workbook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(workbook, worksheet, "Relatorio");
        XLSX.writeFile(workbook, `Relatorio_${reportType}_${new Date().toLocaleDateString('pt-BR').replace(/\//g, '-')}.xlsx`);
    };

    const exportToPdf = () => {
        try {
            if (!reportData) {
                setError("Nenhum dado disponível para exportação.");
                return;
            }

            const dataToExport = reportType === 'financeiro' 
                ? reportData.transacoes || [] 
                : reportData.lotes || [];
                
            if (dataToExport.length === 0) {
                setError("Não há dados para exportar.");
                return;
            }

            const doc = new jsPDF();
            const title = `Relatório ${reportType === 'financeiro' ? 'Financeiro' : 'de Produção'}`;
            doc.text(title, 14, 16);
            
            const head = reportType === 'financeiro' 
                ? [['Data', 'Descrição', 'Tipo', 'Valor', 'Lote', 'Registado Por']]
                : [['Código', 'Identificador', 'Aves', 'Data Entrada', 'Data Saída']];
                
            const body = reportType === 'financeiro'
                ? dataToExport.map(t => [
                    new Date(t.data).toLocaleDateString('pt-BR'), 
                    t.descricao || '', 
                    t.tipo || '', 
                    `R$ ${(t.valor || 0).toFixed(2)}`, 
                    t.lote?.identificador || '-', 
                    t.usuario?.nome || '-'
                  ])
                : dataToExport.map(l => [
                    l.codigo || '', 
                    l.identificador || '', 
                    l.quantidadeAvesInicial || 0, 
                    new Date(l.dataEntrada).toLocaleDateString('pt-BR'), 
                    l.dataSaida ? new Date(l.dataSaida).toLocaleDateString('pt-BR') : '-'
                  ]);

            // CORREÇÃO: Usar a função autoTable importada
            autoTable(doc, { head, body, startY: 25 });
            
            const fileName = `Relatorio_${reportType}_${new Date().toLocaleDateString('pt-BR').replace(/\//g, '-')}.pdf`;
            doc.save(fileName);
            
        } catch (error) {
            console.error("Erro ao exportar para PDF:", error);
            setError("Erro ao exportar para PDF.");
        }
    };

    return (
        <PageContainer
            title="Relatórios"
            subtitle="Gere relatórios detalhados do sistema"
        >
            
            {error && (
                <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
                    {error}
                </Alert>
            )}
            
            <Paper sx={{ p: 2, mb: 3 }}>
                <Grid container spacing={2} alignItems="center">
                    <Grid item xs={12} sm={3}>
                        <FormControl fullWidth>
                            <InputLabel>Tipo de Relatório</InputLabel>
                            <Select 
                                value={reportType} 
                                onChange={(e) => setReportType(e.target.value)}
                                label="Tipo de Relatório"
                            >
                                <MenuItem value="financeiro">Financeiro</MenuItem>
                                <MenuItem value="producao">Produção</MenuItem>
                            </Select>
                        </FormControl>
                    </Grid>
                    <Grid item xs={12} sm={3}>
                        <TextField 
                            name="dataInicio" 
                            label="Data de Início" 
                            type="date" 
                            fullWidth 
                            InputLabelProps={{ shrink: true }} 
                            value={filters.dataInicio} 
                            onChange={handleFilterChange}
                            required
                        />
                    </Grid>
                    <Grid item xs={12} sm={3}>
                        <TextField 
                            name="dataFim" 
                            label="Data de Fim" 
                            type="date" 
                            fullWidth 
                            InputLabelProps={{ shrink: true }} 
                            value={filters.dataFim} 
                            onChange={handleFilterChange}
                            required
                        />
                    </Grid>
                    <Grid item xs={12} sm={3}>
                        <FormControl fullWidth>
                            <InputLabel>Granja (Opcional)</InputLabel>
                            <Select 
                                name="granjaId" 
                                value={filters.granjaId} 
                                onChange={handleFilterChange}
                                label="Granja (Opcional)"
                            >
                                <MenuItem value="">Todas</MenuItem>
                                {granjas.map(g => (
                                    <MenuItem key={g.id} value={g.id}>
                                        {g.nome}
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                    </Grid>
                    <Grid item xs={12}>
                        <Button 
                            variant="contained" 
                            onClick={handleGenerateReport} 
                            disabled={loading}
                            size="large"
                        >
                            {loading ? (
                                <>
                                    <CircularProgress size={20} sx={{ mr: 1 }} />
                                    Gerando...
                                </>
                            ) : (
                                "Gerar Relatório"
                            )}
                        </Button>
                    </Grid>
                </Grid>
            </Paper>

            {reportData && (
                <Paper sx={{ p: 2 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                        <Typography variant="h6">
                            Resultados
                            {reportType === 'financeiro' && reportData.transacoes && (
                                <Typography variant="body2" color="textSecondary">
                                    Total: {reportData.transacoes.length} transações
                                </Typography>
                            )}
                            {reportType === 'producao' && reportData.lotes && (
                                <Typography variant="body2" color="textSecondary">
                                    Total: {reportData.lotes.length} lotes
                                </Typography>
                            )}
                        </Typography>
                        <Box>
                            <Button 
                                startIcon={<DownloadIcon />} 
                                onClick={exportToExcel} 
                                sx={{ mr: 1 }}
                                disabled={!reportData}
                            >
                                Excel
                            </Button>
                            <Button 
                                startIcon={<DownloadIcon />} 
                                onClick={exportToPdf}
                                disabled={!reportData}
                            >
                                PDF
                            </Button>
                        </Box>
                    </Box>

                    {/* Resumo Financeiro */}
                    {reportType === 'financeiro' && reportData && (
                        <Box sx={{ mb: 2, p: 2, backgroundColor: 'grey.100', borderRadius: 1 }}>
                            <Grid container spacing={2}>
                                <Grid item xs={4}>
                                    <Typography variant="body2" color="textSecondary">Total Entradas</Typography>
                                    <Typography variant="h6" color="success.main">
                                        {(reportData.totalEntradas || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                                    </Typography>
                                </Grid>
                                <Grid item xs={4}>
                                    <Typography variant="body2" color="textSecondary">Total Saídas</Typography>
                                    <Typography variant="h6" color="error.main">
                                        {(reportData.totalSaidas || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                                    </Typography>
                                </Grid>
                                <Grid item xs={4}>
                                    <Typography variant="body2" color="textSecondary">Saldo</Typography>
                                    <Typography 
                                        variant="h6" 
                                        color={reportData.saldo >= 0 ? 'success.main' : 'error.main'}
                                    >
                                        {(reportData.saldo || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                                    </Typography>
                                </Grid>
                            </Grid>
                        </Box>
                    )}
                    
                    {reportType === 'financeiro' ? (
                        <TableContainer sx={{ maxHeight: 600 }}>
                            <Table stickyHeader>
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
                                    {(reportData.transacoes || []).map(t => (
                                        <TableRow key={t.id}>
                                            <TableCell>
                                                {new Date(t.data).toLocaleDateString('pt-BR')}
                                            </TableCell>
                                            <TableCell>{t.descricao || '-'}</TableCell>
                                            <TableCell>
                                                <Box 
                                                    sx={{ 
                                                        px: 1, 
                                                        py: 0.5, 
                                                        borderRadius: 1,
                                                        backgroundColor: t.tipo === 'Entrada' ? 'success.light' : 'error.light',
                                                        color: t.tipo === 'Entrada' ? 'success.dark' : 'error.dark',
                                                        display: 'inline-block'
                                                    }}
                                                >
                                                    {t.tipo}
                                                </Box>
                                            </TableCell>
                                            <TableCell align="right">
                                                {(t.valor || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                                            </TableCell>
                                            <TableCell>{t.lote?.identificador || '-'}</TableCell>
                                            <TableCell>{t.usuario?.nome || '-'}</TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    ) : (
                        <TableContainer sx={{ maxHeight: 600 }}>
                            <Table stickyHeader>
                                <TableHead>
                                    <TableRow>
                                        <TableCell>Código</TableCell>
                                        <TableCell>Identificador</TableCell>
                                        <TableCell align="right">Aves Iniciais</TableCell>
                                        <TableCell>Data Entrada</TableCell>
                                        <TableCell>Data Saída</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {(reportData.lotes || []).map(l => (
                                        <TableRow key={l.id}>
                                            <TableCell>{l.codigo || '-'}</TableCell>
                                            <TableCell>{l.identificador || '-'}</TableCell>
                                            <TableCell align="right">{l.quantidadeAvesInicial || 0}</TableCell>
                                            <TableCell>
                                                {new Date(l.dataEntrada).toLocaleDateString('pt-BR')}
                                            </TableCell>
                                            <TableCell>
                                                {l.dataSaida ? new Date(l.dataSaida).toLocaleDateString('pt-BR') : 'Em andamento'}
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    )}

                    {/* Mensagem quando não há dados */}
                    {reportType === 'financeiro' && (!reportData.transacoes || reportData.transacoes.length === 0) && (
                        <Box sx={{ textAlign: 'center', py: 4 }}>
                            <Typography variant="body1" color="textSecondary">
                                Nenhuma transação encontrada para o período selecionado.
                            </Typography>
                        </Box>
                    )}

                    {reportType === 'producao' && (!reportData.lotes || reportData.lotes.length === 0) && (
                        <Box sx={{ textAlign: 'center', py: 4 }}>
                            <Typography variant="body1" color="textSecondary">
                                Nenhum lote encontrado para o período selecionado.
                            </Typography>
                        </Box>
                    )}
                </Paper>
            )}
        </PageContainer>
    );
}

export default RelatoriosPage;
