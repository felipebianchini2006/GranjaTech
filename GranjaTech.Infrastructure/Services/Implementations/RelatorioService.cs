using GranjaTech.Application.DTOs;
using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GranjaTech.Infrastructure.Services.Implementations
{
    public class RelatorioService : IRelatorioService
    {
        private readonly GranjaTechDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RelatorioService> _logger;

        public RelatorioService(
            GranjaTechDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<RelatorioService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private (int userId, string userRole) GetCurrentUser()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !user.Identity.IsAuthenticated)
                {
                    _logger.LogWarning("Usuário não autenticado tentando acessar relatórios");
                    throw new UnauthorizedAccessException("Usuário não autenticado.");
                }

                var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRoleClaim = user.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(userRoleClaim))
                {
                    _logger.LogWarning("Claims de usuário não encontradas. UserIdClaim: {UserIdClaim}, UserRoleClaim: {UserRoleClaim}",
                        userIdClaim, userRoleClaim);
                    throw new UnauthorizedAccessException("Claims de usuário não encontradas no token.");
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("ID do usuário inválido: {UserIdClaim}", userIdClaim);
                    throw new ArgumentException("ID do usuário inválido.");
                }

                _logger.LogInformation("Usuário autenticado: ID={UserId}, Role={UserRole}", userId, userRoleClaim);
                return (userId, userRoleClaim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações do usuário atual");
                throw;
            }
        }

        public async Task<RelatorioFinanceiroDto> GetRelatorioFinanceiroAsync(DateTime dataInicio, DateTime dataFim, int? granjaId)
        {
            try
            {
                _logger.LogInformation("Iniciando geração de relatório financeiro. DataInicio: {DataInicio}, DataFim: {DataFim}, GranjaId: {GranjaId}",
                    dataInicio, dataFim, granjaId);

                // Validação de datas
                if (dataInicio > dataFim)
                {
                    throw new ArgumentException("A data de início não pode ser posterior à data de fim.");
                }

                if (dataInicio > DateTime.Now)
                {
                    throw new ArgumentException("A data de início não pode ser futura.");
                }

                var (userId, userRole) = GetCurrentUser();

                // Query base para transações de lotes
                var transacoesDeLotesQuery = _context.TransacoesFinanceiras
                    .Where(t => t.LoteId != null &&
                                t.Data.Date >= dataInicio.Date &&
                                t.Data.Date <= dataFim.Date);

                // Aplicar filtros de permissão
                switch (userRole)
                {
                    case "Produtor":
                        transacoesDeLotesQuery = transacoesDeLotesQuery
                            .Where(t => t.Lote != null && t.Lote.Granja.UsuarioId == userId);
                        break;

                    case "Financeiro":
                        var produtorIds = await _context.FinanceiroProdutor
                            .Where(fp => fp.FinanceiroId == userId)
                            .Select(fp => fp.ProdutorId)
                            .ToListAsync();

                        if (!produtorIds.Any())
                        {
                            _logger.LogWarning("Usuário financeiro {UserId} não possui produtores associados", userId);
                            transacoesDeLotesQuery = transacoesDeLotesQuery.Where(t => false);
                        }
                        else
                        {
                            transacoesDeLotesQuery = transacoesDeLotesQuery
                                .Where(t => t.Lote != null && produtorIds.Contains(t.Lote.Granja.UsuarioId));
                        }
                        break;

                    case "Administrador":
                        // Administrador pode ver tudo, não precisa filtrar
                        break;

                    default:
                        _logger.LogWarning("Role não reconhecida: {UserRole}", userRole);
                        throw new UnauthorizedAccessException("Permissão insuficiente para acessar relatórios.");
                }

                // Filtro por granja se especificado
                if (granjaId.HasValue && granjaId.Value > 0)
                {
                    // Verificar se a granja existe e se o usuário tem acesso
                    var granja = await _context.Granjas
                        .FirstOrDefaultAsync(g => g.Id == granjaId.Value);

                    if (granja == null)
                    {
                        throw new ArgumentException("Granja não encontrada.");
                    }

                    // Verificar permissão na granja específica
                    if (userRole == "Produtor" && granja.UsuarioId != userId)
                    {
                        throw new UnauthorizedAccessException("Você não tem permissão para acessar dados desta granja.");
                    }

                    transacoesDeLotesQuery = transacoesDeLotesQuery
                        .Where(t => t.Lote != null && t.Lote.GranjaId == granjaId.Value);
                }

                // Query para transações gerais (sem lote)
                var transacoesGeraisQuery = _context.TransacoesFinanceiras
                    .Where(t => t.LoteId == null &&
                                t.Data.Date >= dataInicio.Date &&
                                t.Data.Date <= dataFim.Date);

                // Transações gerais são visíveis apenas para o criador ou Admin
                if (userRole != "Administrador")
                {
                    transacoesGeraisQuery = transacoesGeraisQuery.Where(t => t.UsuarioId == userId);
                }

                // Executar queries com includes necessários
                _logger.LogDebug("Executando query de transações de lotes...");
                var transacoesDeLotes = await transacoesDeLotesQuery
                    .Include(t => t.Lote)
                        .ThenInclude(l => l.Granja)
                    .Include(t => t.Usuario)
                    .AsNoTracking()
                    .Take(1000) // Limitar resultados para evitar problemas de memória
                    .ToListAsync();

                _logger.LogDebug("Executando query de transações gerais...");
                var transacoesGerais = await transacoesGeraisQuery
                    .Include(t => t.Usuario)
                    .AsNoTracking()
                    .Take(1000) // Limitar resultados para evitar problemas de memória
                    .ToListAsync();

                _logger.LogDebug("Queries executadas. Transações de lotes: {LotesCount}, Transações gerais: {GeraisCount}", 
                    transacoesDeLotes.Count, transacoesGerais.Count);

                // Combinar e ordenar resultados
                _logger.LogDebug("Combinando e ordenando resultados...");
                var transacoesFinais = transacoesGerais
                    .Concat(transacoesDeLotes)
                    .OrderByDescending(t => t.Data)
                    .ToList();

                _logger.LogDebug("Calculando totais...");
                // Calcular totais
                var totalEntradas = transacoesFinais
                    .Where(t => string.Equals(t.Tipo, "Entrada", StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Valor);

                var totalSaidas = transacoesFinais
                    .Where(t => string.Equals(t.Tipo, "Saida", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(t.Tipo, "Saída", StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Valor);

                _logger.LogDebug("Criando resultado final...");
                var resultado = new RelatorioFinanceiroDto
                {
                    TotalEntradas = totalEntradas,
                    TotalSaidas = totalSaidas,
                    Saldo = totalEntradas - totalSaidas,
                    Transacoes = transacoesFinais
                };

                _logger.LogInformation("Relatório financeiro gerado com sucesso. Total transações: {Count}, Total entradas: {TotalEntradas}, Total saídas: {TotalSaidas}",
                    transacoesFinais.Count, totalEntradas, totalSaidas);

                var transacoesCount = resultado.Transacoes?.Count() ?? 0;
                _logger.LogInformation("Retornando resultado do relatório financeiro com {TransacoesCount} transações", transacoesCount);
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório financeiro. DataInicio: {DataInicio}, DataFim: {DataFim}, GranjaId: {GranjaId}",
                    dataInicio, dataFim, granjaId);
                throw;
            }
        }

        public async Task<RelatorioProducaoDto> GetRelatorioProducaoAsync(DateTime dataInicio, DateTime dataFim, int? granjaId)
        {
            try
            {
                _logger.LogInformation("Iniciando geração de relatório de produção. DataInicio: {DataInicio}, DataFim: {DataFim}, GranjaId: {GranjaId}",
                    dataInicio, dataFim, granjaId);

                // Validação de datas
                if (dataInicio > dataFim)
                {
                    throw new ArgumentException("A data de início não pode ser posterior à data de fim.");
                }

                var (userId, userRole) = GetCurrentUser();

                // Query base para lotes
                IQueryable<Lote> query = _context.Lotes
                    .Include(l => l.Granja)
                    .Where(l => l.DataEntrada.Date >= dataInicio.Date &&
                                l.DataEntrada.Date <= dataFim.Date);

                // Aplicar filtros de permissão
                switch (userRole)
                {
                    case "Produtor":
                        query = query.Where(l => l.Granja.UsuarioId == userId);
                        break;

                    case "Financeiro":
                        var produtorIds = await _context.FinanceiroProdutor
                            .Where(fp => fp.FinanceiroId == userId)
                            .Select(fp => fp.ProdutorId)
                            .ToListAsync();

                        if (!produtorIds.Any())
                        {
                            _logger.LogWarning("Usuário financeiro {UserId} não possui produtores associados", userId);
                            return new RelatorioProducaoDto
                            {
                                TotalLotes = 0,
                                TotalAvesInicial = 0,
                                Lotes = new List<Lote>()
                            };
                        }

                        query = query.Where(l => produtorIds.Contains(l.Granja.UsuarioId));
                        break;

                    case "Administrador":
                        // Administrador pode ver tudo
                        break;

                    default:
                        _logger.LogWarning("Role não reconhecida: {UserRole}", userRole);
                        throw new UnauthorizedAccessException("Permissão insuficiente para acessar relatórios.");
                }

                // Filtro por granja se especificado
                if (granjaId.HasValue && granjaId.Value > 0)
                {
                    // Verificar se a granja existe e se o usuário tem acesso
                    var granja = await _context.Granjas
                        .FirstOrDefaultAsync(g => g.Id == granjaId.Value);

                    if (granja == null)
                    {
                        throw new ArgumentException("Granja não encontrada.");
                    }

                    // Verificar permissão na granja específica
                    if (userRole == "Produtor" && granja.UsuarioId != userId)
                    {
                        throw new UnauthorizedAccessException("Você não tem permissão para acessar dados desta granja.");
                    }

                    query = query.Where(l => l.GranjaId == granjaId.Value);
                }

                // Executar query
                var lotes = await query
                    .AsNoTracking()
                    .OrderByDescending(l => l.DataEntrada)
                    .Take(1000) // Limitar resultados para evitar problemas de memória
                    .ToListAsync();

                var resultado = new RelatorioProducaoDto
                {
                    TotalLotes = lotes.Count,
                    TotalAvesInicial = lotes.Sum(l => l.QuantidadeAvesInicial),
                    Lotes = lotes
                };

                _logger.LogInformation("Relatório de produção gerado com sucesso. Total lotes: {Count}, Total aves: {TotalAves}",
                    lotes.Count, resultado.TotalAvesInicial);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório de produção. DataInicio: {DataInicio}, DataFim: {DataFim}, GranjaId: {GranjaId}",
                    dataInicio, dataFim, granjaId);
                throw;
            }
        }

        public async Task<RelatorioFinanceiroSimplificadoDto> GetRelatorioFinanceiroSimplificadoAsync(DateTime dataInicio, DateTime dataFim, int? granjaId)
        {
            try
            {
                _logger.LogInformation("Iniciando geração de relatório financeiro SIMPLIFICADO. DataInicio: {DataInicio}, DataFim: {DataFim}, GranjaId: {GranjaId}",
                    dataInicio, dataFim, granjaId);

                var (userId, userRole) = GetCurrentUser();

                // Query simplificada para transações
                var query = _context.TransacoesFinanceiras
                    .Where(t => t.Data.Date >= dataInicio.Date && t.Data.Date <= dataFim.Date);

                // Aplicar filtros de permissão básicos
                if (userRole == "Produtor")
                {
                    var granjaIds = await _context.Granjas
                        .Where(g => g.UsuarioId == userId)
                        .Select(g => g.Id)
                        .ToListAsync();

                    query = query.Where(t => t.LoteId == null || 
                        _context.Lotes.Any(l => l.Id == t.LoteId && granjaIds.Contains(l.GranjaId)));
                }

                // Executar query simplificada com projeção direta
                var transacoes = await query
                    .Select(t => new TransacaoSimplificadaDto
                    {
                        Id = t.Id,
                        Descricao = t.Descricao,
                        Valor = t.Valor,
                        Tipo = t.Tipo,
                        Data = t.Data,
                        LoteIdentificador = t.Lote != null ? t.Lote.Identificador : null,
                        UsuarioNome = t.Usuario.Nome,
                        GranjaNome = t.Lote != null ? t.Lote.Granja.Nome : null
                    })
                    .Take(1000)
                    .AsNoTracking()
                    .ToListAsync();

                var totalEntradas = transacoes
                    .Where(t => string.Equals(t.Tipo, "Entrada", StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Valor);

                var totalSaidas = transacoes
                    .Where(t => string.Equals(t.Tipo, "Saida", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(t.Tipo, "Saída", StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Valor);

                var resultado = new RelatorioFinanceiroSimplificadoDto
                {
                    TotalEntradas = totalEntradas,
                    TotalSaidas = totalSaidas,
                    Saldo = totalEntradas - totalSaidas,
                    Transacoes = transacoes.OrderByDescending(t => t.Data).ToList()
                };

                _logger.LogInformation("Relatório financeiro SIMPLIFICADO gerado com sucesso. Total transações: {Count}",
                    transacoes.Count);

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório financeiro SIMPLIFICADO. DataInicio: {DataInicio}, DataFim: {DataFim}, GranjaId: {GranjaId}",
                    dataInicio, dataFim, granjaId);
                throw;
            }
        }
    }
}