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
    public class DashboardService : IDashboardService
    {
        private readonly GranjaTechDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DashboardService(GranjaTechDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private (int userId, string userRole) GetCurrentUser()
        {
            var userId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            return (userId, userRole);
        }

        public async Task<DashboardKpiDto> GetKpisAsync()
        {
            var (userId, userRole) = GetCurrentUser();

            IQueryable<TransacaoFinanceira> transacoesQuery = _context.TransacoesFinanceiras.Include(t => t.Lote).ThenInclude(l => l.Granja);
            IQueryable<Lote> lotesQuery = _context.Lotes.Include(l => l.Granja);

            if (userRole == "Produtor")
            {
                transacoesQuery = transacoesQuery.Where(t => t.Lote.Granja.UsuarioId == userId);
                lotesQuery = lotesQuery.Where(l => l.Granja.UsuarioId == userId);
            }
            else if (userRole == "Financeiro")
            {
                var produtorIds = await _context.FinanceiroProdutor
                    .Where(fp => fp.FinanceiroId == userId)
                    .Select(fp => fp.ProdutorId)
                    .ToListAsync();

                if (!produtorIds.Any()) return new DashboardKpiDto();

                transacoesQuery = transacoesQuery.Where(t => t.Lote != null && produtorIds.Contains(t.Lote.Granja.UsuarioId));
                lotesQuery = lotesQuery.Where(l => produtorIds.Contains(l.Granja.UsuarioId));
            }

            var totalEntradas = await transacoesQuery.Where(t => t.Tipo == "Entrada").SumAsync(t => t.Valor);
            var totalSaidas = await transacoesQuery.Where(t => t.Tipo == "Saida").SumAsync(t => t.Valor);
            var lotesAtivos = await lotesQuery.CountAsync(l => l.DataSaida == null);

            return new DashboardKpiDto
            {
                TotalEntradas = totalEntradas,
                TotalSaidas = totalSaidas,
                LucroTotal = totalEntradas - totalSaidas,
                LotesAtivos = lotesAtivos
            };
        }

        public async Task<IEnumerable<MonthlySummaryDto>> GetMonthlySummaryAsync()
        {
            var (userId, userRole) = GetCurrentUser();
            IQueryable<TransacaoFinanceira> query = _context.TransacoesFinanceiras.Include(t => t.Lote).ThenInclude(l => l.Granja);

            if (userRole == "Produtor")
            {
                query = query.Where(t => t.Lote != null && t.Lote.Granja.UsuarioId == userId);
            }
            else if (userRole == "Financeiro")
            {
                var produtorIds = await _context.FinanceiroProdutor.Where(fp => fp.FinanceiroId == userId).Select(fp => fp.ProdutorId).ToListAsync();
                if (!produtorIds.Any()) return new List<MonthlySummaryDto>();
                query = query.Where(t => t.Lote != null && produtorIds.Contains(t.Lote.Granja.UsuarioId));
            }

            var summary = await query
                .GroupBy(t => new { t.Data.Year, t.Data.Month })
                .Select(g => new
                {
                    Ano = g.Key.Year,
                    Mes = g.Key.Month,
                    Entradas = g.Where(t => t.Tipo == "Entrada").Sum(t => t.Valor),
                    Saidas = g.Where(t => t.Tipo == "Saida").Sum(t => t.Valor)
                })
                .OrderBy(g => g.Ano).ThenBy(g => g.Mes)
                .ToListAsync();

            return summary.Select(s => new MonthlySummaryDto
            {
                Mes = new DateTime(s.Ano, s.Mes, 1).ToString("MMM/yy", CultureInfo.GetCultureInfo("pt-BR")),
                Entradas = s.Entradas,
                Saidas = s.Saidas
            });
        }
    }
}