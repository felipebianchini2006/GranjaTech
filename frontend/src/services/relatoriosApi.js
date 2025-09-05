// Arquivo: frontend/src/services/relatoriosApi.js

import apiService from './apiService';

// Função helper para debug detalhado
const debugRequest = (endpoint, method = 'GET') => {
  console.log(`[DEBUG] Tentando ${method} ${endpoint}`);
  console.log(`[DEBUG] apiService configurado:`, !!apiService);
  
  // Verificar se apiService tem as funções esperadas
  if (apiService) {
    console.log('[DEBUG] apiService.get exists:', typeof apiService.get === 'function');
    console.log('[DEBUG] apiService.post exists:', typeof apiService.post === 'function');
    console.log('[DEBUG] apiService.defaults:', apiService.defaults);
  }
};

// Função para obter relatório avançado
export async function getRelatorioAvancado({ tipo, granjaId, inicio, fim, setor }) {
  try {
    const params = new URLSearchParams({
      tipo,
      granjaId: String(granjaId),
      inicio: new Date(inicio).toISOString(),
      fim: new Date(fim).toISOString()
    });
    
    if (tipo === "setor" && setor) {
      params.set("setor", setor);
    }

    const endpoint = `/relatorios/avancado?${params.toString()}`;
    debugRequest(endpoint);

    const response = await apiService.get(endpoint);
    console.log('Dados recebidos do relatório:', response.data);
    return response.data;
    
  } catch (error) {
    console.error('Erro ao buscar relatório avançado:', error);
    
    if (error.response?.status === 401) {
      throw new Error("Não autorizado. Faça login novamente.");
    } else if (error.response?.status === 404) {
      throw new Error("Granja não encontrada.");
    } else if (error.response?.status === 400) {
      throw new Error(error.response?.data?.message || "Parâmetros inválidos.");
    } else {
      throw new Error(error.response?.data?.message || error.message || "Erro desconhecido");
    }
  }
}

// Função para testar múltiplos endpoints de granjas
async function tentarEndpointGranjas(endpoint) {
  try {
    debugRequest(endpoint);
    const response = await apiService.get(endpoint);
    console.log(`[SUCCESS] ${endpoint} retornou:`, response.data);
    return response.data;
  } catch (error) {
    console.log(`[FAIL] ${endpoint} falhou:`, error.message);
    throw error;
  }
}

// Função para listar granjas com debug melhorado
export async function listarGranjas() {
  // Lista de endpoints possíveis baseada na estrutura do sistema
  const endpointsGranjas = [
    '/granjas',
    '/granja',
    '/dashboard/granjas',
    '/lotes/granjas', // Endpoint que pode existir para granjas via lotes
  ];

  console.log('[DEBUG] Iniciando busca de granjas...');
  
  // Tentar cada endpoint de granjas
  for (const endpoint of endpointsGranjas) {
    try {
      const data = await tentarEndpointGranjas(endpoint);
      const granjas = normalizeGranjasData(data);
      
      if (granjas.length > 0) {
        console.log(`[SUCCESS] Granjas encontradas via ${endpoint}:`, granjas);
        return granjas;
      }
    } catch (error) {
      // Continua para o próximo endpoint
      continue;
    }
  }

  // Se todos falharam, tentar via lotes (sabemos que funciona no ConsumoPage)
  console.log('[DEBUG] Tentando obter granjas via lotes...');
  try {
    debugRequest('/lotes', 'GET');
    const lotesResponse = await apiService.getLotes();
    console.log('[DEBUG] Resposta dos lotes:', lotesResponse);
    
    const lotes = lotesResponse.data || lotesResponse;
    if (!Array.isArray(lotes)) {
      throw new Error('Resposta de lotes inválida');
    }

    const granjasMap = new Map();
    
    lotes.forEach(lote => {
      console.log('[DEBUG] Processando lote:', lote);
      
      // Diferentes formas de encontrar a granja no lote
      let granja = null;
      
      if (lote.granja) {
        granja = {
          id: lote.granja.id,
          nome: lote.granja.nome || lote.granja.name || `Granja ${lote.granja.id}`
        };
      } else if (lote.granjaId) {
        granja = {
          id: lote.granjaId,
          nome: lote.granjaNome || `Granja ${lote.granjaId}`
        };
      } else if (lote.GranjaId) {
        granja = {
          id: lote.GranjaId,
          nome: lote.GranjaNome || `Granja ${lote.GranjaId}`
        };
      }
      
      if (granja && granja.id) {
        granjasMap.set(granja.id, granja);
      }
    });
    
    const granjasDoLotes = Array.from(granjasMap.values());
    console.log('[DEBUG] Granjas extraídas dos lotes:', granjasDoLotes);
    
    if (granjasDoLotes.length === 0) {
      throw new Error("Nenhuma granja encontrada nos lotes disponíveis");
    }
    
    return granjasDoLotes;
    
  } catch (error) {
    console.error('[ERROR] Falha ao buscar granjas via lotes:', error);
    
    // Último recurso: verificar se é problema de configuração do apiService
    if (error.message.includes('Network Error') || !apiService) {
      throw new Error(
        `Problema de conectividade ou configuração do apiService. ` +
        `Verifique: 1) Se o backend está rodando, ` +
        `2) Se a URL base está correta, ` +
        `3) Se o token de autenticação é válido`
      );
    }
    
    throw new Error(`Não foi possível carregar as granjas: ${error.message}`);
  }
}

// Função para normalizar dados de granjas
function normalizeGranjasData(data) {
  let granjas = [];
  
  if (Array.isArray(data)) {
    granjas = data;
  } else if (data.items && Array.isArray(data.items)) {
    granjas = data.items;
  } else if (data.data && Array.isArray(data.data)) {
    granjas = data.data;
  } else if (data.granjas && Array.isArray(data.granjas)) {
    granjas = data.granjas;
  } else {
    console.warn('[DEBUG] Formato de dados não reconhecido:', data);
    return [];
  }
  
  const granjasNormalizadas = granjas
    .map(g => ({
      id: g.id ?? g.Id ?? g.granjaId ?? g.ID,
      nome: g.nome ?? g.Nome ?? g.name ?? g.identificador ?? g.descricao ?? `Granja ${g.id || 'S/N'}`
    }))
    .filter(g => g.id != null);
  
  return granjasNormalizadas;
}

// Função para testar conectividade da API
export async function testarConectividade() {
  const testEndpoints = [
    '/relatorios/health',
    '/health',
    '/api/health',
    '/'
  ];
  
  for (const endpoint of testEndpoints) {
    try {
      console.log(`[DEBUG] Testando conectividade em: ${endpoint}`);
      const response = await apiService.get(endpoint);
      console.log(`[SUCCESS] Conectividade OK em ${endpoint}:`, response.data);
      return { 
        status: 'ok', 
        data: response.data, 
        endpoint: endpoint 
      };
    } catch (error) {
      console.log(`[FAIL] Conectividade falhou em ${endpoint}:`, error.message);
    }
  }
  
  return { 
    status: 'error', 
    message: 'Nenhum endpoint de health check disponível' 
  };
}

// Função para obter informações do usuário (para debug)
export async function obterInfoUsuario() {
  const userEndpoints = [
    '/auth/me',
    '/user/me',
    '/usuario/me',
    '/me'
  ];
  
  for (const endpoint of userEndpoints) {
    try {
      console.log(`[DEBUG] Testando info do usuário em: ${endpoint}`);
      const response = await apiService.get(endpoint);
      console.log(`[SUCCESS] Info do usuário obtida via ${endpoint}:`, response.data);
      return response.data;
    } catch (error) {
      console.log(`[FAIL] Info do usuário falhou em ${endpoint}:`, error.message);
    }
  }
  
  console.warn('[DEBUG] Nenhum endpoint de usuário funcionou');
  return null;
}

// Função de debug para verificar configuração do apiService
export function debugApiService() {
  console.group('[DEBUG] Verificação do apiService');
  
  if (!apiService) {
    console.error('❌ apiService não está definido');
    return false;
  }
  
  console.log('✅ apiService existe');
  console.log('Base URL:', apiService.defaults?.baseURL);
  console.log('Timeout:', apiService.defaults?.timeout);
  console.log('Headers:', apiService.defaults?.headers);
  
  // Verificar token
  const token = localStorage.getItem('token') || sessionStorage.getItem('token');
  console.log('Token presente:', !!token);
  console.log('Token length:', token?.length || 0);
  
  console.groupEnd();
  return true;
}