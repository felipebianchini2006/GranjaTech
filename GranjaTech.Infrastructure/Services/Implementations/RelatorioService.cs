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

namespace GranjaTech.Infrastructure.Services.Implementations
{
    public class RelatorioService : IRelatorioService
    {
        private readonly GranjaTechDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RelatorioService(GranjaTechDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private (int userId, string userRole) GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) throw new InvalidOperationException("Contexto de utilizador não encontrado.");

            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoleClaim = user.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(userRoleClaim))
            {
                throw new InvalidOperationException("Não foi possível identificar o utilizador logado (claims não encontradas no token).");
            }

            return (int.Parse(userIdClaim), userRoleClaim);
        }

        public async Task<RelatorioFinanceiroDto> GetRelatorioFinanceiroAsync(DateTime dataInicio, DateTime dataFim, int? granjaId)
        {
            var (userId, userRole) = GetCurrentUser();

            // Query para transações de lotes, que têm permissões complexas
            var transacoesDeLotesQuery = _context.TransacoesFinanceiras
                .Where(t => t.LoteId != null && t.Data.Date >= dataInicio.Date && t.Data.Date <= dataFim.Date);

            if (userRole == "Produtor")
            {
                transacoesDeLotesQuery = transacoesDeLotesQuery.Where(t => t.Lote.Granja.UsuarioId == userId);
            }
            else if (userRole == "Financeiro")
            {
                var produtorIds = await _context.FinanceiroProdutor.Where(fp => fp.FinanceiroId == userId).Select(fp => fp.ProdutorId).ToListAsync();
                if (produtorIds.Any())
                {
                    transacoesDeLotesQuery = transacoesDeLotesQuery.Where(t => produtorIds.Contains(t.Lote.Granja.UsuarioId));
                }
                else
                {
                    transacoesDeLotesQuery = transacoesDeLotesQuery.Where(t => false); // Zera a query
                }
            }

            if (granjaId.HasValue)
            {
                transacoesDeLotesQuery = transacoesDeLotesQuery.Where(t => t.Lote.GranjaId == granjaId.Value);
            }

            // Query para transações gerais (sem lote), visíveis apenas para o criador ou Admin
            var transacoesGeraisQuery = _context.TransacoesFinanceiras
                .Where(t => t.LoteId == null && t.Data.Date >= dataInicio.Date && t.Data.Date <= dataFim.Date);

            if (userRole != "Administrador")
            {
                transacoesGeraisQuery = transacoesGeraisQuery.Where(t => t.UsuarioId == userId);
            }

            // Executa as queries e junta os resultados
            var transacoesDeLotes = await transacoesDeLotesQuery
                .Include(t => t.Lote.Granja)
                .Include(t => t.Usuario)
                .ToListAsync();

            var transacoesGerais = await transacoesGeraisQuery
                .Include(t => t.Usuario)
                .ToListAsync();

            var transacoesFinais = transacoesGerais.Concat(transacoesDeLotes).OrderByDescending(t => t.Data).ToList();

            var totalEntradas = transacoesFinais.Where(t => t.Tipo == "Entrada").Sum(t => t.Valor);
            var totalSaidas = transacoesFinais.Where(t => t.Tipo == "Saida").Sum(t => t.Valor);

            return new RelatorioFinanceiroDto
            {
                TotalEntradas = totalEntradas,
                TotalSaidas = totalSaidas,
                Saldo = totalEntradas - totalSaidas,
                Transacoes = transacoesFinais
            };
        }

        public async Task<RelatorioProducaoDto> GetRelatorioProducaoAsync(DateTime dataInicio, DateTime dataFim, int? granjaId)
        {
            var (userId, userRole) = GetCurrentUser();
            IQueryable<Lote> query = _context.Lotes
                .Include(l => l.Granja)
                .Where(l => l.DataEntrada.Date >= dataInicio.Date && l.DataEntrada.Date <= dataFim.Date);

            if (userRole == "Produtor")
            {
                query = query.Where(l => l.Granja.UsuarioId == userId);
            }
            else if (userRole == "Financeiro")
            {
                var produtorIds = await _context.FinanceiroProdutor.Where(fp => fp.FinanceiroId == userId).Select(fp => fp.ProdutorId).ToListAsync();
                if (!produtorIds.Any()) return new RelatorioProducaoDto { Lotes = new List<Lote>() };
                query = query.Where(l => produtorIds.Contains(l.Granja.UsuarioId));
            }

            if (granjaId.HasValue)
            {
                query = query.Where(l => l.GranjaId == granjaId.Value);
            }

            var lotes = await query.ToListAsync();
            return new RelatorioProducaoDto
            {
                TotalLotes = lotes.Count,
                TotalAvesInicial = lotes.Sum(l => l.QuantidadeAvesInicial),
                Lotes = lotes
            };
        }
    }
}
