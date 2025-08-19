import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';
// 1. ADICIONE 'Tooltip' À LISTA DE IMPORTAÇÕES
import { Grid, Paper, Typography, Box, Tooltip } from '@mui/material';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, Legend, ResponsiveContainer } from 'recharts';

// 2. MODIFIQUE O COMPONENTE KpiCard
const KpiCard = ({ title, value, color, description }) => (
    // Envolvemos todo o card com o componente Tooltip
    <Tooltip title={description} placement="top" arrow>
        <Paper 
            sx={{ p: 2, display: 'flex', flexDirection: 'column', height: 140, backgroundColor: color, color: '#fff' }}
        >
            <Typography component="h2" variant="h6" gutterBottom>
                {title}
            </Typography>
            <Typography component="p" variant="h4">
                {value}
            </Typography>
        </Paper>
    </Tooltip>
);

function DashboardPage() {
    const [kpis, setKpis] = useState(null);
    const [monthlyData, setMonthlyData] = useState([]);

    const fetchData = useCallback(async () => {
        try {
            const [kpisRes, monthlyRes] = await Promise.all([
                apiService.getKpis(),
                apiService.getMonthlySummary()
            ]);
            setKpis(kpisRes.data);
            setMonthlyData(monthlyRes.data);
        } catch (error)
        {
            console.error("Erro ao buscar dados do dashboard:", error);
        }
    }, []);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    const formatCurrency = (value) => 
        value?.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) || 'R$ 0,00';

    return (
        <Box sx={{ p: 3 }}>
            <Typography variant="h4" gutterBottom>Dashboard</Typography>
            {kpis && (
                // A chamada dos KpiCards continua a mesma, passando a 'description'
                <Grid container spacing={3} sx={{ mb: 4 }}>
                    <Grid item xs={12} sm={6} md={3}>
                        <KpiCard 
                            title="Total de Entradas" 
                            value={formatCurrency(kpis.totalEntradas)} 
                            color="#4caf50" 
                            description="Soma de todos os valores registrados como entradas financeiras." 
                        />
                    </Grid>
                    <Grid item xs={12} sm={6} md={3}>
                        <KpiCard 
                            title="Total de Saídas" 
                            value={formatCurrency(kpis.totalSaidas)} 
                            color="#f44336"
                            description="Soma de todos os valores registrados como saídas financeiras (despesas)." 
                        />
                    </Grid>
                    <Grid item xs={12} sm={6} md={3}>
                        <KpiCard 
                            title="Lucro Total" 
                            value={formatCurrency(kpis.lucroTotal)} 
                            color={kpis.lucroTotal >= 0 ? '#2196f3' : '#f44336'}
                            description="Resultado da subtração: Total de Entradas - Total de Saídas." 
                        />
                    </Grid>
                    <Grid item xs={12} sm={6} md={3}>
                        <KpiCard 
                            title="Lotes Ativos" 
                            value={kpis.lotesAtivos} 
                            color="#ff9800"
                            description="Número de lotes que estão atualmente em produção (ainda não possuem uma data de saída registrada)." 
                        />
                    </Grid>
                </Grid>
            )}

            <Typography variant="h5" gutterBottom>Resumo Mensal</Typography>
            <Paper sx={{ p: 2, height: 400 }}>
                <ResponsiveContainer width="100%" height="100%">
                    <BarChart
                        data={monthlyData}
                        margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                    >
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="mes" />
                        <YAxis tickFormatter={(value) => `R$${value/1000}k`} />
                        {/* Renomeei o Tooltip do Recharts para evitar conflito de nome */}
                        <RechartsTooltip formatter={(value) => formatCurrency(value)} />
                        <Legend />
                        <Bar dataKey="entradas" fill="#4caf50" name="Entradas" />
                        <Bar dataKey="saidas" fill="#f44336" name="Saídas" />
                    </BarChart>
                </ResponsiveContainer>
            </Paper>
        </Box>
    );
}

export default DashboardPage;