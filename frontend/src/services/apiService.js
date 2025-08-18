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
    // Funções de Autenticação
    login: (credenciais) => apiClient.post('/auth/login', credenciais),
    registrar: (dadosDeRegistro) => apiClient.post('/auth/registrar', dadosDeRegistro),
    getUsers: () => apiClient.get('/auth/usuarios'),
    updateUser: (id, userData) => apiClient.put(`/auth/usuarios/${id}`, userData),
    deleteUser: (id) => apiClient.delete(`/auth/usuarios/${id}`),
    
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

    // ADICIONE AS FUNÇÕES FINANCEIRAS ABAIXO
    getTransacoes: () => apiClient.get('/financas'),
    createTransacao: (novaTransacao) => apiClient.post('/financas', novaTransacao),
};

export default apiService;