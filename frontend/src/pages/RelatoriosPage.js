// src/pages/RelatoriosPage.js
import React, { useState, useEffect, useCallback } from 'react';
import { getRelatorioAvancado, listarGranjas, testarConectividade, obterInfoUsuario, debugApiService } from '../services/relatoriosApi';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import * as XLSX from 'xlsx';
import PageContainer from '../components/PageContainer';
import apiService from '../services/apiService';

import { 
    Typography, Box, Paper, Grid, TextField, Button, Select, 
    MenuItem, FormControl, InputLabel, CircularProgress,
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
    Alert, Card, CardContent, Chip, Tabs, Tab, Accordion, AccordionSummary, AccordionDetails
} from '@mui/material';
import {
    Download as DownloadIcon,
    ExpandMore as ExpandMoreIcon,
    Assessment as AssessmentIcon,
    AttachMoney as MoneyIcon,
    Agriculture as AgricultureIcon,
    Science as ScienceIcon,
    MonitorHeart as MonitorIcon,
    RestaurantMenu as FoodIcon,
    Scale as ScaleIcon,
    BugReport as BugIcon,
    Refresh as RefreshIcon
} from '@mui/icons-material';

// Helpers numéricos seguros
const n = (v) => {
    const x = Number(v);
    return Number.isFinite(x) ? x : 0;
};
const nfix = (v, d) => n(v).toFixed(d);

function RelatoriosPage() {
    const [tabValue, setTabValue] = useState(0);
    const [filters, setFilters] = useState({
        dataInicio: '',
        dataFim: '',
        granjaId: '',
        setor: 'consumo'
    });
    const [granjas, setGranjas] = useState([]);
    const [reportData, setReportData] = useState(null);
    const [loading, setLoading] = useState(false);
    const [loadingGranjas, setLoadingGranjas] = useState(true);
    const [error, setError] = useState('');
    const [debugMode, setDebugMode] = useState(false);
    const [apiStatus, setApiStatus] = useState(null);
    const [debugInfo, setDebugInfo] = useState('');

    const setores = [
        { value: 'consumo', label: 'Consumo (Ração/Água)', icon: <FoodIcon /> },
        { value: 'pesagem', label: 'Pesagens', icon: <ScaleIcon /> },
        { value: 'sanitario', label: 'Eventos Sanitários', icon: <ScienceIcon /> },
        { value: 'sensores', label: 'Sensores', icon: <MonitorIcon /> }
    ];

    const runDiagnostico = async () => {
        setDebugInfo('Executando diagnóstico completo...\n');
        setDebugInfo(prev => prev + '\n=== VERIFICAÇÃO DO APISERVICE ===\n');
        const apiOk = debugApiService();
        if (!apiOk) {
            setDebugInfo(prev => prev + '❌ apiService não configurado corretamente\n');
            return;
        }
        setDebugInfo(prev => prev + '\n=== TESTE DE CONECTIVIDADE ===\n');
        try {
            const response = await fetch('/api/health');
            setDebugInfo(prev => prev + `✅ Fetch direto para /api/health: ${response.status}\n`);
        } catch (error) {
            setDebugInfo(prev => prev + `❌ Fetch direto falhou: ${error.message}\n`);
        }
        setDebugInfo(prev => prev + '\n=== TESTE DO APISERVICE ===\n');
        try {
            const testResponse = await apiService.get('/');
            setDebugInfo(prev => prev + `✅ apiService.get('/') funcionou\n`);
        } catch (error) {
            setDebugInfo(prev => prev + `❌ apiService.get('/') falhou: ${error.message}\n`);
            setDebugInfo(prev => prev + `   - URL tentativa: ${apiService.defaults?.baseURL || 'indefinida'}\n`);
            setDebugInfo(prev => prev + `   - Erro tipo: ${error.code || error.name}\n`);
        }
        setDebugInfo(prev => prev + '\n=== TESTE DE LOTES ===\n');
        try {
            const lotesResponse = await apiService.getLotes();
            setDebugInfo(prev => prev + `✅ apiService.getLotes() funcionou, ${lotesResponse.data?.length || 0} lotes\n`);
            if (lotesResponse.data?.length > 0) {
                const primeiroLote = lotesResponse.data[0];
                setDebugInfo(prev => prev + `   - Primeiro lote: ${JSON.stringify(primeiroLote, null, 2).substring(0, 200)}...\n`);
            }
        } catch (error) {
            setDebugInfo(prev => prev + `❌ apiService.getLotes() falhou: ${error.message}\n`);
        }
        setDebugInfo(prev => prev + '\n=== TESTE MANUAL DE ENDPOINTS DE GRANJA ===\n');
        const endpointsTestar = ['/granjas', '/granja', '/dashboard/granjas'];
        for (const endpoint of endpointsTestar) {
            try {
                const response = await apiService.get(endpoint);
                setDebugInfo(prev => prev + `✅ ${endpoint}: ${JSON.stringify(response.data).substring(0, 100)}...\n`);
            } catch (error) {
                setDebugInfo(prev => prev + `❌ ${endpoint}: ${error.message}\n`);
            }
        }
    };

    const testarAPI = async () => {
        try {
            const [conectividade, userInfo] = await Promise.all([
                testarConectividade(),
                obterInfoUsuario()
            ]);
            setApiStatus({
                conectividade,
                usuario: userInfo,
                timestamp: new Date().toISOString()
            });
            console.log('Teste de API completo:', { conectividade, userInfo });
        } catch (error) {
            console.error('Erro no teste de API:', error);
            setApiStatus({
                conectividade: { status: 'error', message: error.message },
                usuario: null,
                timestamp: new Date().toISOString()
            });
        }
    };

    const fetchGranjas = useCallback(async () => {
        setLoadingGranjas(true);
        setError('');
        try {
            console.log('Iniciando busca de granjas...');
            const granjasList = await listarGranjas();
            if (!granjasList || granjasList.length === 0) {
                throw new Error('Nenhuma granja encontrada. Verifique suas permissões.');
            }
            console.log('Granjas carregadas:', granjasList);
            setGranjas(granjasList);
            if (granjasList.length === 1) {
                setFilters(prev => ({ ...prev, granjaId: granjasList[0].id }));
            }
        } catch (error) {
            console.error("Erro ao buscar granjas:", error);
            setError(`Erro ao carregar granjas: ${error.message}`);
            await runDiagnostico();
            testarAPI();
        } finally {
            setLoadingGranjas(false);
        }
    }, []);

    useEffect(() => {
        fetchGranjas();
    }, [fetchGranjas]);

    const handleFilterChange = (e) => {
        setFilters({ ...filters, [e.target.name]: e.target.value });
        setError('');
    };

    const getCurrentTipo = () => {
        switch (tabValue) {
            case 0: return 'financeiro';
            case 1: return 'geral';
            case 2: return 'setor';
            default: return 'financeiro';
        }
    };

    const handleGenerateReport = async () => {
        if (!filters.dataInicio || !filters.dataFim) {
            setError("Por favor, selecione a data de início e a data de fim.");
            return;
        }
        if (!filters.granjaId) {
            setError("Por favor, selecione uma granja.");
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
                tipo: getCurrentTipo(),
                granjaId: parseInt(filters.granjaId),
                inicio: filters.dataInicio,
                fim: filters.dataFim
            };
            if (getCurrentTipo() === 'setor') {
                params.setor = filters.setor;
            }
            console.log('Gerando relatório com parâmetros:', params);
            const response = await getRelatorioAvancado(params);
            console.log('Resposta do relatório:', response);
            setReportData(response);
            const hasData = checkHasData(response);
            if (!hasData) {
                setError("Nenhum dado encontrado para o período selecionado.");
            }
        } catch (error) {
            console.error('Erro ao gerar relatório:', error);
            setError(`Falha ao gerar relatório: ${error.message}`);
        } finally {
            setLoading(false);
        }
    };

    const checkHasData = (data) => {
        if (!data) return false;
        switch (getCurrentTipo()) {
            case 'financeiro':
                return data.itens && data.itens.length > 0;
            case 'geral':
                return (data.consumo && data.consumo.length > 0) ||
                       (data.pesagens && data.pesagens.length > 0) ||
                       (data.sanitario && data.sanitario.length > 0) ||
                       (data.sensores && data.sensores.length > 0);
            case 'setor':
                return data.itens && data.itens.length > 0;
            default:
                return false;
        }
    };

    const exportToExcel = () => {
        if (!reportData) return;
        try {
            const workbook = XLSX.utils.book_new();
            switch (getCurrentTipo()) {
                case 'financeiro':
                    if (reportData.itens && reportData.itens.length > 0) {
                        const worksheet = XLSX.utils.json_to_sheet(
                            reportData.itens.map(i => ({
                                ...i,
                                valor: n(i.valor)
                            }))
                        );
                        XLSX.utils.book_append_sheet(workbook, worksheet, "Financeiro");
                    }
                    break;
                case 'geral':
                    if (reportData.consumo && reportData.consumo.length > 0) {
                        const wsConsumo = XLSX.utils.json_to_sheet(
                            reportData.consumo.map(i => ({
                                ...i,
                                racaoKg: n(i.racaoKg),
                                aguaLitros: n(i.aguaLitros),
                                avesVivas: n(i.avesVivas)
                            }))
                        );
                        XLSX.utils.book_append_sheet(workbook, wsConsumo, "Consumo");
                    }
                    if (reportData.pesagens && reportData.pesagens.length > 0) {
                        const wsPesagens = XLSX.utils.json_to_sheet(
                            reportData.pesagens.map(i => ({
                                ...i,
                                pesoMedioKg: n(i.pesoMedioKg),
                                amostra: n(i.amostra)
                            }))
                        );
                        XLSX.utils.book_append_sheet(workbook, wsPesagens, "Pesagens");
                    }
                    if (reportData.sanitario && reportData.sanitario.length > 0) {
                        const wsSanitario = XLSX.utils.json_to_sheet(reportData.sanitario);
                        XLSX.utils.book_append_sheet(workbook, wsSanitario, "Sanitario");
                    }
                    if (reportData.sensores && reportData.sensores.length > 0) {
                        const wsSensores = XLSX.utils.json_to_sheet(
                            reportData.sensores.map(i => ({ ...i, valor: n(i.valor) }))
                        );
                        XLSX.utils.book_append_sheet(workbook, wsSensores, "Sensores");
                    }
                    break;
                case 'setor':
                    if (reportData.itens && reportData.itens.length > 0) {
                        const norm = reportData.itens.map(i => {
                            switch (filters.setor) {
                                case 'consumo':
                                    return { ...i, racaoKg: n(i.racaoKg), aguaLitros: n(i.aguaLitros), avesVivas: n(i.avesVivas) };
                                case 'pesagem':
                                    return { ...i, pesoMedioKg: n(i.pesoMedioKg), amostra: n(i.amostra) };
                                case 'sensores':
                                    return { ...i, valor: n(i.valor) };
                                default:
                                    return i;
                            }
                        });
                        const worksheet = XLSX.utils.json_to_sheet(norm);
                        XLSX.utils.book_append_sheet(workbook, worksheet, filters.setor.charAt(0).toUpperCase() + filters.setor.slice(1));
                    }
                    break;
            }
            const fileName = `Relatorio_${getCurrentTipo()}_${filters.setor || ''}_${new Date().toLocaleDateString('pt-BR').replace(/\//g, '-')}.xlsx`;
            XLSX.writeFile(workbook, fileName);
        } catch (error) {
            console.error("Erro ao exportar para Excel:", error);
            setError("Erro ao exportar para Excel.");
        }
    };

    const exportToPdf = () => {
        try {
            if (!reportData) {
                setError("Nenhum dado disponível para exportação.");
                return;
            }
            const doc = new jsPDF();
            const title = `Relatório ${getCurrentTipo().charAt(0).toUpperCase() + getCurrentTipo().slice(1)}`;
            doc.text(title, 14, 16);
            let yPosition = 30;

            switch (getCurrentTipo()) {
                case 'financeiro':
                    if (reportData.itens && reportData.itens.length > 0) {
                        const head = [['Data', 'Categoria', 'Descrição', 'Valor']];
                        const body = reportData.itens.map(item => [
                            new Date(item.data).toLocaleDateString('pt-BR'),
                            item.categoria,
                            item.descricao,
                            `R$ ${nfix(item.valor, 2)}`
                        ]);
                        autoTable(doc, { head, body, startY: yPosition });
                    }
                    break;

                case 'geral':
                    if (reportData.consumo && reportData.consumo.length > 0) {
                        doc.text('Consumo', 14, yPosition);
                        const headConsumo = [['Data', 'Ração (Kg)', 'Água (L)', 'Aves Vivas']];
                        const bodyConsumo = reportData.consumo.map(item => [
                            new Date(item.data).toLocaleDateString('pt-BR'),
                            nfix(item.racaoKg, 2),
                            nfix(item.aguaLitros, 2),
                            String(n(item.avesVivas))
                        ]);
                        autoTable(doc, { head: headConsumo, body: bodyConsumo, startY: yPosition + 10 });
                        yPosition = doc.lastAutoTable.finalY + 20;
                    }
                    if (reportData.pesagens && reportData.pesagens.length > 0) {
                        doc.text('Pesagens', 14, yPosition);
                        const headPesagens = [['Data', 'Peso Médio (Kg)', 'Amostra']];
                        const bodyPesagens = reportData.pesagens.map(item => [
                            new Date(item.data).toLocaleDateString('pt-BR'),
                            nfix(item.pesoMedioKg, 3),
                            String(n(item.amostra))
                        ]);
                        autoTable(doc, { head: headPesagens, body: bodyPesagens, startY: yPosition + 10 });
                        yPosition = doc.lastAutoTable.finalY + 20;
                    }
                    if (reportData.sanitario && reportData.sanitario.length > 0) {
                        doc.text('Sanitário', 14, yPosition);
                        const headSan = [['Data', 'Tipo Evento', 'Produto', 'Via']];
                        const bodySan = reportData.sanitario.map(item => [
                            new Date(item.data).toLocaleDateString('pt-BR'),
                            item.tipoEvento,
                            item.produto,
                            item.via || '-'
                        ]);
                        autoTable(doc, { head: headSan, body: bodySan, startY: yPosition + 10 });
                        yPosition = doc.lastAutoTable.finalY + 20;
                    }
                    if (reportData.sensores && reportData.sensores.length > 0) {
                        doc.text('Sensores', 14, yPosition);
                        const headSen = [['Data', 'Tipo', 'Valor']];
                        const bodySen = reportData.sensores.map(item => [
                            new Date(item.data).toLocaleDateString('pt-BR'),
                            item.tipo,
                            nfix(item.valor, 2)
                        ]);
                        autoTable(doc, { head: headSen, body: bodySen, startY: yPosition + 10 });
                        yPosition = doc.lastAutoTable.finalY + 20;
                    }
                    break;

                case 'setor':
                    if (reportData.itens && reportData.itens.length > 0) {
                        let head, body;
                        switch (filters.setor) {
                            case 'consumo':
                                head = [['Data', 'Ração (Kg)', 'Água (L)', 'Aves Vivas']];
                                body = reportData.itens.map(item => [
                                    new Date(item.data).toLocaleDateString('pt-BR'),
                                    nfix(item.racaoKg, 2),
                                    nfix(item.aguaLitros, 2),
                                    String(n(item.avesVivas))
                                ]);
                                break;
                            case 'pesagem':
                                head = [['Data', 'Peso Médio (Kg)', 'Amostra']];
                                body = reportData.itens.map(item => [
                                    new Date(item.data).toLocaleDateString('pt-BR'),
                                    nfix(item.pesoMedioKg, 3),
                                    String(n(item.amostra))
                                ]);
                                break;
                            case 'sanitario':
                                head = [['Data', 'Tipo Evento', 'Produto', 'Via']];
                                body = reportData.itens.map(item => [
                                    new Date(item.data).toLocaleDateString('pt-BR'),
                                    item.tipoEvento,
                                    item.produto,
                                    item.via || '-'
                                ]);
                                break;
                            case 'sensores':
                                head = [['Data', 'Tipo', 'Valor']];
                                body = reportData.itens.map(item => [
                                    new Date(item.data).toLocaleDateString('pt-BR'),
                                    item.tipo,
                                    nfix(item.valor, 2)
                                ]);
                                break;
                            default:
                                head = []; body = [];
                        }
                        autoTable(doc, { head, body, startY: yPosition });
                    }
                    break;
            }
            const fileName = `Relatorio_${getCurrentTipo()}_${new Date().toLocaleDateString('pt-BR').replace(/\//g, '-')}.pdf`;
            doc.save(fileName);
        } catch (error) {
            console.error("Erro ao exportar para PDF:", error);
            setError("Erro ao exportar para PDF.");
        }
    };

    const renderFinanceiroSummary = () => {
        if (!reportData || getCurrentTipo() !== 'financeiro') return null;
        return (
            <Box sx={{ mb: 2, p: 2, backgroundColor: 'grey.100', borderRadius: 1 }}>
                <Grid container spacing={2}>
                    <Grid item xs={4}>
                        <Typography variant="body2" color="textSecondary">Total Entradas</Typography>
                        <Typography variant="h6" color="success.main">
                            {n(reportData.totalEntradas || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                        </Typography>
                    </Grid>
                    <Grid item xs={4}>
                        <Typography variant="body2" color="textSecondary">Total Saídas</Typography>
                        <Typography variant="h6" color="error.main">
                            {n(reportData.totalSaidas || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                        </Typography>
                    </Grid>
                    <Grid item xs={4}>
                        <Typography variant="body2" color="textSecondary">Saldo</Typography>
                        <Typography 
                            variant="h6" 
                            color={n(reportData.saldo) >= 0 ? 'success.main' : 'error.main'}
                        >
                            {n(reportData.saldo || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                        </Typography>
                    </Grid>
                </Grid>
            </Box>
        );
    };

    const renderFinanceiroTable = () => {
        if (!reportData.itens || reportData.itens.length === 0) return null;
        return (
            <TableContainer sx={{ maxHeight: 600 }}>
                <Table stickyHeader>
                    <TableHead>
                        <TableRow>
                            <TableCell>Data</TableCell>
                            <TableCell>Categoria</TableCell>
                            <TableCell>Descrição</TableCell>
                            <TableCell align="right">Valor</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {reportData.itens.map((item, index) => (
                            <TableRow key={index}>
                                <TableCell>
                                    {new Date(item.data).toLocaleDateString('pt-BR')}
                                </TableCell>
                                <TableCell>
                                    <Chip 
                                        label={item.categoria}
                                        color={item.categoria === 'entrada' ? 'success' : 'error'}
                                        size="small"
                                    />
                                </TableCell>
                                <TableCell>{item.descricao}</TableCell>
                                <TableCell align="right">
                                    {n(item.valor).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        );
    };

    const renderGeralAccordions = () => {
        return (
            <Box>
                {reportData.consumo && reportData.consumo.length > 0 && (
                    <Accordion defaultExpanded>
                        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                            <Typography variant="h6">Consumo ({reportData.consumo.length} registros)</Typography>
                        </AccordionSummary>
                        <AccordionDetails>
                            <TableContainer sx={{ maxHeight: 400 }}>
                                <Table>
                                    <TableHead>
                                        <TableRow>
                                            <TableCell>Data</TableCell>
                                            <TableCell>Ração (kg)</TableCell>
                                            <TableCell>Água (L)</TableCell>
                                            <TableCell>Aves Vivas</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {reportData.consumo.map((item, index) => (
                                            <TableRow key={index}>
                                                <TableCell>{new Date(item.data).toLocaleDateString('pt-BR')}</TableCell>
                                                <TableCell>{nfix(item.racaoKg, 2)}</TableCell>
                                                <TableCell>{nfix(item.aguaLitros, 2)}</TableCell>
                                                <TableCell>{n(item.avesVivas)}</TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            </TableContainer>
                        </AccordionDetails>
                    </Accordion>
                )}

                {reportData.pesagens && reportData.pesagens.length > 0 && (
                    <Accordion>
                        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                            <Typography variant="h6">Pesagens ({reportData.pesagens.length} registros)</Typography>
                        </AccordionSummary>
                        <AccordionDetails>
                            <TableContainer sx={{ maxHeight: 400 }}>
                                <Table>
                                    <TableHead>
                                        <TableRow>
                                            <TableCell>Data</TableCell>
                                            <TableCell>Peso Médio (kg)</TableCell>
                                            <TableCell>Amostra</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {reportData.pesagens.map((item, index) => (
                                            <TableRow key={index}>
                                                <TableCell>{new Date(item.data).toLocaleDateString('pt-BR')}</TableCell>
                                                <TableCell>{nfix(item.pesoMedioKg, 3)}</TableCell>
                                                <TableCell>{n(item.amostra)}</TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            </TableContainer>
                        </AccordionDetails>
                    </Accordion>
                )}

                {reportData.sanitario && reportData.sanitario.length > 0 && (
                    <Accordion>
                        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                            <Typography variant="h6">Sanitário ({reportData.sanitario.length} registros)</Typography>
                        </AccordionSummary>
                        <AccordionDetails>
                            <TableContainer sx={{ maxHeight: 400 }}>
                                <Table>
                                    <TableHead>
                                        <TableRow>
                                            <TableCell>Data</TableCell>
                                            <TableCell>Tipo Evento</TableCell>
                                            <TableCell>Produto</TableCell>
                                            <TableCell>Via</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {reportData.sanitario.map((item, index) => (
                                            <TableRow key={index}>
                                                <TableCell>{new Date(item.data).toLocaleDateString('pt-BR')}</TableCell>
                                                <TableCell>{item.tipoEvento}</TableCell>
                                                <TableCell>{item.produto}</TableCell>
                                                <TableCell>{item.via || '-'}</TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            </TableContainer>
                        </AccordionDetails>
                    </Accordion>
                )}

                {reportData.sensores && reportData.sensores.length > 0 && (
                    <Accordion>
                        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                            <Typography variant="h6">Sensores ({reportData.sensores.length} registros)</Typography>
                        </AccordionSummary>
                        <AccordionDetails>
                            <TableContainer sx={{ maxHeight: 400 }}>
                                <Table>
                                    <TableHead>
                                        <TableRow>
                                            <TableCell>Data</TableCell>
                                            <TableCell>Tipo</TableCell>
                                            <TableCell align="right">Valor</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {reportData.sensores.map((item, index) => (
                                            <TableRow key={index}>
                                                <TableCell>{new Date(item.data).toLocaleDateString('pt-BR')}</TableCell>
                                                <TableCell>{item.tipo}</TableCell>
                                                <TableCell align="right">{nfix(item.valor, 2)}</TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            </TableContainer>
                        </AccordionDetails>
                    </Accordion>
                )}
            </Box>
        );
    };

    const renderSetorTable = () => {
        if (!reportData?.itens || reportData.itens.length === 0) return null;
        let headers, renderRow;
        switch (filters.setor) {
            case 'consumo':
                headers = ['Data', 'Ração (Kg)', 'Água (L)', 'Aves Vivas'];
                renderRow = (item, index) => (
                    <TableRow key={index}>
                        <TableCell>{new Date(item.data).toLocaleDateString('pt-BR')}</TableCell>
                        <TableCell align="right">{nfix(item.racaoKg, 2)}</TableCell>
                        <TableCell align="right">{nfix(item.aguaLitros, 2)}</TableCell>
                        <TableCell align="right">{n(item.avesVivas)}</TableCell>
                    </TableRow>
                );
                break;
            case 'pesagem':
                headers = ['Data', 'Peso Médio (Kg)', 'Amostra'];
                renderRow = (item, index) => (
                    <TableRow key={index}>
                        <TableCell>{new Date(item.data).toLocaleDateString('pt-BR')}</TableCell>
                        <TableCell align="right">{nfix(item.pesoMedioKg, 3)}</TableCell>
                        <TableCell align="right">{n(item.amostra)}</TableCell>
                    </TableRow>
                );
                break;
            case 'sanitario':
                headers = ['Data', 'Tipo Evento', 'Produto', 'Via'];
                renderRow = (item, index) => (
                    <TableRow key={index}>
                        <TableCell>{new Date(item.data).toLocaleDateString('pt-BR')}</TableCell>
                        <TableCell>{item.tipoEvento}</TableCell>
                        <TableCell>{item.produto}</TableCell>
                        <TableCell>{item.via || '-'}</TableCell>
                    </TableRow>
                );
                break;
            case 'sensores':
                headers = ['Data', 'Tipo', 'Valor'];
                renderRow = (item, index) => (
                    <TableRow key={index}>
                        <TableCell>{new Date(item.data).toLocaleDateString('pt-BR')}</TableCell>
                        <TableCell>{item.tipo}</TableCell>
                        <TableCell align="right">{nfix(item.valor, 2)}</TableCell>
                    </TableRow>
                );
                break;
            default:
                return null;
        }
        return (
            <TableContainer sx={{ maxHeight: 600 }}>
                <Table stickyHeader>
                    <TableHead>
                        <TableRow>
                            {headers.map((header, index) => (
                                <TableCell key={index} align={index > 0 ? 'right' : 'left'}>
                                    {header}
                                </TableCell>
                            ))}
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {reportData.itens.map(renderRow)}
                    </TableBody>
                </Table>
            </TableContainer>
        );
    };

    const renderContent = () => {
        if (!reportData) return null;
        switch (getCurrentTipo()) {
            case 'financeiro':
                return (
                    <>
                        {renderFinanceiroSummary()}
                        {renderFinanceiroTable()}
                    </>
                );
            case 'geral':
                return renderGeralAccordions();
            case 'setor':
                return renderSetorTable();
            default:
                return null;
        }
    };

    const renderDebugPanel = () => {
        if (!debugMode) return null;
        return (
            <Paper sx={{ p: 2, mb: 2, backgroundColor: 'grey.50' }}>
                <Typography variant="h6" gutterBottom>
                    Debug Information
                </Typography>
                <Grid container spacing={2}>
                    <Grid item xs={12} md={6}>
                        <Typography variant="subtitle2">Status das Granjas:</Typography>
                        <Typography variant="body2" color={granjas.length > 0 ? 'success.main' : 'error.main'}>
                            {granjas.length > 0 ? `${granjas.length} granjas carregadas` : 'Nenhuma granja encontrada'}
                        </Typography>
                        {granjas.length > 0 && (
                            <Box sx={{ mt: 1 }}>
                                {granjas.map(g => (
                                    <Typography key={g.id} variant="caption" display="block">
                                        ID: {g.id} - Nome: {g.nome}
                                    </Typography>
                                ))}
                            </Box>
                        )}
                    </Grid>
                    <Grid item xs={12} md={6}>
                        <Typography variant="subtitle2">Status da API:</Typography>
                        {apiStatus ? (
                            <Box>
                                <Typography variant="body2" color={apiStatus.conectividade.status === 'ok' ? 'success.main' : 'error.main'}>
                                    Conectividade: {apiStatus.conectividade.status}
                                </Typography>
                                <Typography variant="body2">
                                    Usuário: {apiStatus.usuario?.name || 'Não identificado'}
                                </Typography>
                                <Typography variant="caption">
                                    Testado em: {new Date(apiStatus.timestamp).toLocaleString('pt-BR')}
                                </Typography>
                            </Box>
                        ) : (
                            <Typography variant="body2" color="warning.main">
                                Teste não executado
                            </Typography>
                        )}
                    </Grid>
                </Grid>
                {debugInfo && (
                    <Box sx={{ mt: 2 }}>
                        <Typography variant="subtitle2">Log de Diagnóstico:</Typography>
                        <Paper sx={{ p: 2, backgroundColor: 'grey.100', maxHeight: 300, overflow: 'auto' }}>
                            <Typography variant="caption" component="pre" style={{ whiteSpace: 'pre-wrap', fontFamily: 'monospace' }}>
                                {debugInfo}
                            </Typography>
                        </Paper>
                    </Box>
                )}
                <Box sx={{ mt: 2 }}>
                    <Button variant="outlined" size="small" onClick={testarAPI} sx={{ mr: 1 }}>
                        Testar Conectividade
                    </Button>
                    <Button variant="outlined" size="small" onClick={fetchGranjas} sx={{ mr: 1 }}>
                        Recarregar Granjas
                    </Button>
                    <Button variant="outlined" size="small" onClick={runDiagnostico}>
                        Diagnóstico Completo
                    </Button>
                </Box>
            </Paper>
        );
    };

    return (
        <PageContainer
            title="Relatórios Avançados"
            subtitle="Gere relatórios detalhados do sistema"
        >
            {error && (
                <Alert 
                    severity="error" 
                    sx={{ mb: 2 }} 
                    onClose={() => setError('')}
                    action={
                        <Button 
                            startIcon={<BugIcon />}
                            size="small" 
                            onClick={() => setDebugMode(!debugMode)}
                        >
                            Debug
                        </Button>
                    }
                >
                    {error}
                </Alert>
            )}
            
            {renderDebugPanel()}
            
            <Paper sx={{ p: 2, mb: 3 }}>
                <Tabs 
                    value={tabValue} 
                    onChange={(e, newValue) => setTabValue(newValue)}
                    sx={{ mb: 2 }}
                >
                    <Tab icon={<MoneyIcon />} label="Financeiro" />
                    <Tab icon={<AssessmentIcon />} label="Geral" />
                    <Tab icon={<AgricultureIcon />} label="Setorial" />
                </Tabs>

                <Grid container spacing={2} alignItems="center">
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
                        <FormControl fullWidth required>
                            <InputLabel>Granja</InputLabel>
                            <Select 
                                name="granjaId" 
                                value={filters.granjaId} 
                                onChange={handleFilterChange}
                                label="Granja"
                                disabled={loadingGranjas || granjas.length === 0}
                            >
                                {granjas.map(g => (
                                    <MenuItem key={g.id} value={g.id}>
                                        {g.nome}
                                    </MenuItem>
                                ))}
                                {granjas.length === 0 && !loadingGranjas && (
                                    <MenuItem value="" disabled>
                                        Nenhuma granja disponível
                                    </MenuItem>
                                )}
                            </Select>
                            {loadingGranjas && (
                                <Box sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
                                    <CircularProgress size={16} sx={{ mr: 1 }} />
                                    <Typography variant="caption">Carregando granjas...</Typography>
                                </Box>
                            )}
                        </FormControl>
                    </Grid>
                    
                    {getCurrentTipo() === 'setor' && (
                        <Grid item xs={12} sm={3}>
                            <FormControl fullWidth>
                                <InputLabel>Setor</InputLabel>
                                <Select 
                                    name="setor" 
                                    value={filters.setor} 
                                    onChange={handleFilterChange}
                                    label="Setor"
                                >
                                    {setores.map(setor => (
                                        <MenuItem key={setor.value} value={setor.value}>
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                {setor.icon}
                                                {setor.label}
                                            </Box>
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Grid>
                    )}
                    
                    <Grid item xs={12}>
                        <Button 
                            variant="contained" 
                            onClick={handleGenerateReport} 
                            disabled={loading || loadingGranjas || granjas.length === 0}
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
                            Resultados - {getCurrentTipo().charAt(0).toUpperCase() + getCurrentTipo().slice(1)}
                            {getCurrentTipo() === 'setor' && ` (${filters.setor})`}
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

                    {renderContent()}

                    {!checkHasData(reportData) && (
                        <Box sx={{ textAlign: 'center', py: 4 }}>
                            <Typography variant="body1" color="textSecondary">
                                Nenhum dado encontrado para o período selecionado.
                            </Typography>
                        </Box>
                    )}
                </Paper>
            )}
        </PageContainer>
    );
}

export default RelatoriosPage;
