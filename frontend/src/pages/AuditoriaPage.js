import React, { useState, useEffect, useCallback } from 'react';
import apiService from '../services/apiService';
import { Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Typography, Box } from '@mui/material';

function AuditoriaPage() {
    const [logs, setLogs] = useState([]);

    const fetchLogs = useCallback(async () => {
        try {
            const response = await apiService.getAuditLogs();
            setLogs(response.data);
        } catch (error) {
            console.error("Erro ao buscar logs de auditoria:", error);
        }
    }, []);

    useEffect(() => {
        fetchLogs();
    }, [fetchLogs]);

    return (
        <Box sx={{ p: 3 }}>
            <Typography variant="h4" gutterBottom>
                Registro de Auditoria
            </Typography>
            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Data/Hora (UTC)</TableCell>
                            <TableCell>Email do Usuário</TableCell>
                            <TableCell>Ação</TableCell>
                            <TableCell>Detalhes</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {logs.map((log) => (
                            <TableRow key={log.id}>
                                <TableCell>{new Date(log.timestamp).toLocaleString('pt-BR')}</TableCell>
                                <TableCell>{log.usuarioEmail}</TableCell>
                                <TableCell>{log.acao}</TableCell>
                                <TableCell>{log.detalhes}</TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </Box>
    );
}

export default AuditoriaPage;