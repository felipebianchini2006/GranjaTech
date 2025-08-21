import axios from 'axios';

const API_URL = 'https://localhost:7135/api';

const apiClient = axios.create({
    baseURL: API_URL,
});

apiClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

const apiService = {
    // Funções de Autenticação e Utilizadores
    login: (credenciais) => apiClient.post('/auth/login', credenciais),
    registrar: (dadosDeRegisto) => apiClient.post('/auth/registrar', dadosDeRegisto),
    getUsers: () => apiClient.get('/auth/usuarios'),
    getUserById: (id) => apiClient.get(`/auth/usuarios/${id}`),
    updateUser: (id, userData) => apiClient.put(`/auth/usuarios/${id}`, userData),
    deleteUser: (id) => apiClient.delete(`/auth/usuarios/${id}`),
    
    // Funções de Perfil do Utilizador
    getProfile: () => apiClient.get('/profile'),
    updateProfile: (profileData) => apiClient.put('/profile', profileData),
    changePassword: (passwordData) => apiClient.post('/profile/change-password', passwordData),

    // Funções de Granjas
    getGranjas: () => apiClient.get('/granjas'),
    createGranja: (novaGranja) => apiClient.post('/granjas', novaGranja),
    updateGranja: (id, granjaAtualizada) => apiClient.put(`/granjas/${id}`, granjaAtualizada),
    deleteGranja: (id) => apiClient.delete(`/granjas/${id}`),

    // Funções de Lotes
    getLotes: () => apiClient.get('/lotes'),
    createLote: (novoLote) => apiClient.post('/lotes', novoLote),
    updateLote: (id, loteAtualizado) => apiClient.put(`/lotes/${id}`, loteAtualizado),
    deleteLote: (id) => apiClient.delete(`/lotes/${id}`),

    // Funções Financeiras
    getTransacoes: () => apiClient.get('/financas'),
    createTransacao: (novaTransacao) => apiClient.post('/financas', novaTransacao),
    updateTransacao: (id, transacaoData) => apiClient.put(`/financas/${id}`, transacaoData),
    deleteTransacao: (id) => apiClient.delete(`/financas/${id}`),

    // Funções do Dashboard
    getKpis: () => apiClient.get('/dashboard/kpis'),
    getMonthlySummary: () => apiClient.get('/dashboard/resumo-mensal'),

    // Função de Auditoria
    getAuditLogs: () => apiClient.get('/auditoria'),

    // Funções de Estoque
    getEstoque: () => apiClient.get('/estoque'),
    createProduto: (novoProduto) => apiClient.post('/estoque', novoProduto),
    updateProduto: (id, produtoAtualizado) => apiClient.put(`/estoque/${id}`, produtoAtualizado),
    deleteProduto: (id) => apiClient.delete(`/estoque/${id}`),

    // Funções de Sensores
    getSensores: () => apiClient.get('/sensores'),
    createSensor: (novoSensor) => apiClient.post('/sensores', novoSensor),
    deleteSensor: (id) => apiClient.delete(`/sensores/${id}`),
    getLeituras: (sensorId) => apiClient.get(`/sensores/${sensorId}/leituras`),

    // Funções de Relatórios
    getRelatorioFinanceiro: (params) => apiClient.get('/relatorios/financeiro', { params }),
    getRelatorioProducao: (params) => apiClient.get('/relatorios/producao', { params }),
};

export default apiService;
