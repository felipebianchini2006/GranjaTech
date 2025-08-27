import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';
import PageContainer from '../components/PageContainer';
import LoadingSpinner from '../components/LoadingSpinner';
import { 
    Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, 
    Typography, Box, Card, Alert, Chip
} from '@mui/material';
import {
    Security as SecurityIcon,
    Person as PersonIcon,
    AccessTime as AccessTimeIcon,
    Info as InfoIcon
} from '@mui/icons-material';

function AuditoriaPage() {
    const [logs, setLogs] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const fetchLogs = useCallback(async () => {
        try {
            setLoading(true);
            setError('');
            const response = await apiService.getAuditLogs();
            setLogs(response.data);
        } catch (error) {
            console.error("Erro ao buscar logs de auditoria:", error);
            setError('Erro ao carregar logs de auditoria. Tente novamente.');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchLogs();
    }, [fetchLogs]);

    const getActionColor = (action) => {
        if (action?.toLowerCase().includes('criar')) return 'success';
        if (action?.toLowerCase().includes('editar') || action?.toLowerCase().includes('atualizar')) return 'warning';
        if (action?.toLowerCase().includes('excluir') || action?.toLowerCase().includes('deletar')) return 'error';
        if (action?.toLowerCase().includes('login')) return 'info';
        return 'default';
    };

    if (loading) {
        return <LoadingSpinner message="Carregando logs de auditoria..." />;
    }

    return (
        <PageContainer
            title="Auditoria"
            subtitle="Registro de ações do sistema para compliance e segurança"
        >
            {error && (
                <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError('')}>
                    {error}
                </Alert>
            )}

            <Card>
                <TableContainer>
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableCell sx={{ fontWeight: 600 }}>Data/Hora</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Usuário</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Ação</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Detalhes</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {logs.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={4} align="center" sx={{ py: 8 }}>
                                        <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
                                            <SecurityIcon sx={{ fontSize: 48, color: 'text.secondary' }} />
                                            <Typography color="text.secondary">
                                                Nenhum log de auditoria encontrado
                                            </Typography>
                                        </Box>
                                    </TableCell>
                                </TableRow>
                            ) : (
                                logs.map((log) => (
                                    <TableRow 
                                        key={log.id}
                                        sx={{ 
                                            '&:hover': { backgroundColor: 'action.hover' },
                                            transition: 'background-color 0.2s ease-in-out',
                                        }}
                                    >
                                        <TableCell>
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                <AccessTimeIcon color="action" sx={{ fontSize: 16 }} />
                                                <Typography variant="body2">
                                                    {new Date(log.timestamp).toLocaleString('pt-BR')}
                                                </Typography>
                                            </Box>
                                        </TableCell>
                                        <TableCell>
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                <PersonIcon color="action" sx={{ fontSize: 16 }} />
                                                <Typography variant="body2">
                                                    {log.usuarioEmail || 'Sistema'}
                                                </Typography>
                                            </Box>
                                        </TableCell>
                                        <TableCell>
                                            <Chip
                                                label={log.acao}
                                                color={getActionColor(log.acao)}
                                                variant="outlined"
                                                size="small"
                                            />
                                        </TableCell>
                                        <TableCell>
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                                <InfoIcon color="action" sx={{ fontSize: 16 }} />
                                                <Typography variant="body2" sx={{ maxWidth: 300, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                                                    {log.detalhes || 'Sem detalhes'}
                                                </Typography>
                                            </Box>
                                        </TableCell>
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Card>
        </PageContainer>
    );
}

export default AuditoriaPage;