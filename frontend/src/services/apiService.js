import axios from 'axios';

const API_URL = 'https://localhost:7135/api';

const apiClient = axios.create({
    baseURL: API_URL,
    timeout: 30000, // 30 segundos timeout
    headers: {
        'Content-Type': 'application/json',
    },
});

// Interceptor para adicionar token e logs de requisição
apiClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        
        // Log detalhado para debug
        console.log('🚀 Requisição enviada:', {
            method: config.method?.toUpperCase(),
            url: config.url,
            baseURL: config.baseURL,
            fullURL: `${config.baseURL}${config.url}`,
            params: config.params,
            data: config.data,
            headers: config.headers
        });
        
        return config;
    },
    (error) => {
        console.error('❌ Erro na configuração da requisição:', error);
        return Promise.reject(error);
    }
);

// Interceptor para logs de resposta e tratamento de erros
apiClient.interceptors.response.use(
    (response) => {
        console.log('✅ Resposta recebida:', {
            status: response.status,
            statusText: response.statusText,
            url: response.config.url,
            data: response.data
        });
        return response;
    },
    (error) => {
        console.error('❌ Erro na resposta:', {
            message: error.message,
            status: error.response?.status,
            statusText: error.response?.statusText,
            url: error.config?.url,
            data: error.response?.data,
            headers: error.response?.headers
        });
        
        // Tratamento específico para erro 401 (não autorizado)
        if (error.response?.status === 401) {
            console.warn('🔐 Token expirado ou inválido, redirecionando para login...');
            localStorage.removeItem('token');
            window.location.href = '/login';
        }
        
        return Promise.reject(error);
    }
);

const apiService = {
    // Funções de Autenticação e Utilizadores
    login: (credenciais) => apiClient.post('/auth/login', credenciais),
    registrar: (dadosDeRegisto) => apiClient.post('/auth/registrar', dadosDeRegisto),
    createUser: (novoUsuario) => apiClient.post('/auth/registrar', novoUsuario),
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

    // Funções de Relatórios - VERSÃO CORRIGIDA
    getRelatorioFinanceiro: (params) => {
        console.log('📊 Solicitando relatório financeiro com parâmetros:', params);
        
        // Se params é um objeto simples, usar diretamente
        if (params && typeof params === 'object' && !(params instanceof URLSearchParams)) {
            return apiClient.get('/relatorios/financeiro', { params });
        }
        
        // Se params é URLSearchParams, converter para objeto
        if (params instanceof URLSearchParams) {
            const paramsObj = {};
            for (const [key, value] of params.entries()) {
                paramsObj[key] = value;
            }
            return apiClient.get('/relatorios/financeiro', { params: paramsObj });
        }
        
        // Fallback: usar params diretamente
        return apiClient.get('/relatorios/financeiro', { params });
    },
    
    getRelatorioProducao: (params) => {
        console.log('📊 Solicitando relatório de produção com parâmetros:', params);
        
        // Se params é um objeto simples, usar diretamente
        if (params && typeof params === 'object' && !(params instanceof URLSearchParams)) {
            return apiClient.get('/relatorios/producao', { params });
        }
        
        // Se params é URLSearchParams, converter para objeto
        if (params instanceof URLSearchParams) {
            const paramsObj = {};
            for (const [key, value] of params.entries()) {
                paramsObj[key] = value;
            }
            return apiClient.get('/relatorios/producao', { params: paramsObj });
        }
        
        // Fallback: usar params diretamente
        return apiClient.get('/relatorios/producao', { params });
    },

    // Função para testar conectividade da API de relatórios
    testRelatoriosHealth: () => {
        console.log('🩺 Testando conectividade da API de relatórios...');
        return apiClient.get('/relatorios/health');
    },

    // Função para debug - verificar token atual
    debugToken: () => {
        const token = localStorage.getItem('token');
        if (token) {
            try {
                // Decodifica o JWT (apenas a parte do payload, sem verificar assinatura)
                const payload = JSON.parse(atob(token.split('.')[1]));
                console.log('🔑 Token atual:', {
                    userId: payload.nameid || payload.sub,
                    role: payload.role,
                    exp: new Date(payload.exp * 1000).toLocaleString(),
                    isExpired: payload.exp * 1000 < Date.now()
                });
                return payload;
            } catch (error) {
                console.error('❌ Erro ao decodificar token:', error);
                return null;
            }
        } else {
            console.warn('⚠️ Nenhum token encontrado no localStorage');
            return null;
        }
    }
};

export default apiService;