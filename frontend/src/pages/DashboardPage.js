import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';
import PageContainer from '../components/PageContainer';
import LoadingSpinner from '../components/LoadingSpinner';
import { Grid, Paper, Typography, Box, Tooltip, Card, CardContent } from '@mui/material';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, Legend, ResponsiveContainer } from 'recharts';
import { 
  TrendingUp as TrendingUpIcon,
  TrendingDown as TrendingDownIcon,
  AccountBalance as AccountBalanceIcon,
  Pets as PetsIcon
} from '@mui/icons-material';

const KpiCard = ({ title, value, color, description, icon: Icon }) => (
    <Tooltip title={description} placement="top" arrow>
        <Card 
            sx={{ 
                height: 160,
                background: `linear-gradient(135deg, ${color}, ${color}dd)`,
                color: 'white',
                cursor: 'pointer',
                transition: 'all 0.3s ease-in-out',
                '&:hover': {
                    transform: 'translateY(-4px)',
                    boxShadow: '0px 12px 32px rgba(0,0,0,0.2)',
                },
            }}
        >
            <CardContent sx={{ p: 3, height: '100%', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                    <Typography component="h3" variant="h6" sx={{ fontWeight: 600 }}>
                        {title}
                    </Typography>
                    {Icon && <Icon sx={{ fontSize: 32, opacity: 0.8 }} />}
                </Box>
                <Typography component="p" variant="h4" sx={{ fontWeight: 700 }}>
                    {value}
                </Typography>
            </CardContent>
        </Card>
    </Tooltip>
);

function DashboardPage() {
    const [kpis, setKpis] = useState(null);
    const [monthlyData, setMonthlyData] = useState([]);
    const [loading, setLoading] = useState(true);

    const fetchData = useCallback(async () => {
        try {
            setLoading(true);
            const [kpisRes, monthlyRes] = await Promise.all([
                apiService.getKpis(),
                apiService.getMonthlySummary()
            ]);
            setKpis(kpisRes.data);
            setMonthlyData(monthlyRes.data);
        } catch (error) {
            console.error("Erro ao buscar dados do dashboard:", error);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    const formatCurrency = (value) => 
        value?.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) || 'R$ 0,00';

    if (loading) {
        return <LoadingSpinner message="Carregando dashboard..." />;
    }

    return (
        <PageContainer 
            title="Dashboard" 
            subtitle="Visão geral do seu sistema de gestão agropecuária"
            showBreadcrumbs={false}
        >
            {kpis && (
                <Grid container spacing={3} sx={{ mb: 4 }}>
                    <Grid item xs={12} sm={6} lg={3}>
                        <KpiCard 
                            title="Total de Entradas" 
                            value={formatCurrency(kpis.totalEntradas)} 
                            color="#4caf50" 
                            description="Soma de todos os valores registrados como entradas financeiras."
                            icon={TrendingUpIcon}
                        />
                    </Grid>
                    <Grid item xs={12} sm={6} lg={3}>
                        <KpiCard 
                            title="Total de Saídas" 
                            value={formatCurrency(kpis.totalSaidas)} 
                            color="#f44336"
                            description="Soma de todos os valores registrados como saídas financeiras (despesas)."
                            icon={TrendingDownIcon}
                        />
                    </Grid>
                    <Grid item xs={12} sm={6} lg={3}>
                        <KpiCard 
                            title="Lucro Total" 
                            value={formatCurrency(kpis.lucroTotal)} 
                            color={kpis.lucroTotal >= 0 ? '#2196f3' : '#f44336'}
                            description="Resultado da subtração: Total de Entradas - Total de Saídas."
                            icon={AccountBalanceIcon}
                        />
                    </Grid>
                    <Grid item xs={12} sm={6} lg={3}>
                        <KpiCard 
                            title="Lotes Ativos" 
                            value={kpis.lotesAtivos} 
                            color="#ff9800"
                            description="Número de lotes que estão atualmente em produção (ainda não possuem uma data de saída registrada)."
                            icon={PetsIcon}
                        />
                    </Grid>
                </Grid>
            )}

            <Card sx={{ p: 0, mb: 3 }}>
                <CardContent sx={{ p: 3 }}>
                    <Typography variant="h5" gutterBottom sx={{ fontWeight: 600, mb: 3 }}>
                        Resumo Mensal
                    </Typography>
                    <Box sx={{ height: 400, width: '100%' }}>
                        <ResponsiveContainer width="100%" height="100%">
                            <BarChart
                                data={monthlyData}
                                margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                            >
                                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                                <XAxis 
                                    dataKey="mes" 
                                    tick={{ fontSize: 12 }}
                                    tickLine={{ stroke: '#e0e0e0' }}
                                />
                                <YAxis 
                                    tickFormatter={(value) => `R$${(value/1000).toFixed(0)}k`}
                                    tick={{ fontSize: 12 }}
                                    tickLine={{ stroke: '#e0e0e0' }}
                                />
                                <RechartsTooltip 
                                    formatter={(value) => formatCurrency(value)}
                                    labelStyle={{ color: '#666' }}
                                    contentStyle={{ 
                                        backgroundColor: '#fff',
                                        border: '1px solid #e0e0e0',
                                        borderRadius: '8px',
                                        boxShadow: '0px 4px 12px rgba(0,0,0,0.1)'
                                    }}
                                />
                                <Legend />
                                <Bar 
                                    dataKey="entradas" 
                                    fill="#4caf50" 
                                    name="Entradas"
                                    radius={[4, 4, 0, 0]}
                                />
                                <Bar 
                                    dataKey="saidas" 
                                    fill="#f44336" 
                                    name="Saídas"
                                    radius={[4, 4, 0, 0]}
                                />
                            </BarChart>
                        </ResponsiveContainer>
                    </Box>
                </CardContent>
            </Card>
        </PageContainer>
    );
}

export default DashboardPage;