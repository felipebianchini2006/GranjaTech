using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GranjaTech.Infrastructure.Services.Implementations
{
    public class FinancasService : IFinancasService
    {
        private readonly GranjaTechDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditoriaService _auditoriaService;

        public FinancasService(GranjaTechDbContext context, IHttpContextAccessor httpContextAccessor, IAuditoriaService auditoriaService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _auditoriaService = auditoriaService;
        }

        private (int userId, string userRole) GetCurrentUser()
        {
            var userId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            return (userId, userRole);
        }

        public async Task<IEnumerable<TransacaoFinanceira>> GetAllAsync()
        {
            var (userId, userRole) = GetCurrentUser();
            IQueryable<TransacaoFinanceira> query = _context.TransacoesFinanceiras
                .Include(t => t.Lote)
                .ThenInclude(l => l.Granja);

            if (userRole == "Administrador") { }
            else if (userRole == "Produtor")
            {
                query = query.Where(t => t.Lote != null && t.Lote.Granja.UsuarioId == userId);
            }
            else if (userRole == "Financeiro")
            {
                var produtorIds = await _context.FinanceiroProdutor.Where(fp => fp.FinanceiroId == userId).Select(fp => fp.ProdutorId).ToListAsync();
                if (produtorIds.Any()) { query = query.Where(t => t.Lote != null && produtorIds.Contains(t.Lote.Granja.UsuarioId)); } else { return new List<TransacaoFinanceira>(); }
            }
            else { return new List<TransacaoFinanceira>(); }

            return await query.ToListAsync();
        }

        public async Task AddAsync(TransacaoFinanceira transacao)
        {
            if (transacao.LoteId == 0)
            {
                transacao.LoteId = null;
            }
            await _context.TransacoesFinanceiras.AddAsync(transacao);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("CRIACAO_TRANSACAO", $"Transação '{transacao.Descricao}' (ID: {transacao.Id}), Valor: {transacao.Valor}, Tipo: {transacao.Tipo}.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var transacao = await _context.TransacoesFinanceiras.FindAsync(id);
            if (transacao == null) return false;

            _context.TransacoesFinanceiras.Remove(transacao);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("DELECAO_TRANSACAO", $"Transação '{transacao.Descricao}' (ID: {id}) deletada.");
            return true;
        }
    }
}
