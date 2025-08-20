using GranjaTech.Application.DTOs;
using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GranjaTech.Infrastructure.Services.Implementations
{
    public class LoteService : ILoteService
    {
        private readonly GranjaTechDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditoriaService _auditoriaService;

        public LoteService(GranjaTechDbContext context, IHttpContextAccessor httpContextAccessor, IAuditoriaService auditoriaService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _auditoriaService = auditoriaService;
        }

        private (int userId, string userRole) GetCurrentUser()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoleClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(userRoleClaim))
            {
                throw new InvalidOperationException("Não foi possível identificar o utilizador logado.");
            }

            return (int.Parse(userIdClaim), userRoleClaim);
        }

        public async Task<IEnumerable<Lote>> GetAllAsync()
        {
            var (userId, userRole) = GetCurrentUser();
            // CORREÇÃO AQUI: Adicionamos .ThenInclude(g => g.Usuario)
            IQueryable<Lote> query = _context.Lotes
                .Include(l => l.Granja)
                    .ThenInclude(g => g.Usuario);

            if (userRole == "Administrador") { }
            else if (userRole == "Produtor")
            {
                query = query.Where(l => l.Granja.UsuarioId == userId);
            }
            else if (userRole == "Financeiro")
            {
                var produtorIds = await _context.FinanceiroProdutor.Where(fp => fp.FinanceiroId == userId).Select(fp => fp.ProdutorId).ToListAsync();
                if (produtorIds.Any()) { query = query.Where(l => produtorIds.Contains(l.Granja.UsuarioId)); } else { return new List<Lote>(); }
            }
            else { return new List<Lote>(); }
            return await query.ToListAsync();
        }

        public async Task<Lote?> GetByIdAsync(int id)
        {
            // CORREÇÃO AQUI TAMBÉM
            var lote = await _context.Lotes
                .Include(l => l.Granja)
                    .ThenInclude(g => g.Usuario)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lote == null) return null;

            var (userId, userRole) = GetCurrentUser();

            if (userRole == "Administrador" || lote.Granja.UsuarioId == userId) return lote;

            if (userRole == "Financeiro")
            {
                var produtorIds = await _context.FinanceiroProdutor.Where(fp => fp.FinanceiroId == userId).Select(fp => fp.ProdutorId).ToListAsync();
                if (produtorIds.Contains(lote.Granja.UsuarioId)) return lote;
            }
            return null;
        }

        public async Task AddAsync(CreateLoteDto loteDto)
        {
            var (userId, userRole) = GetCurrentUser();
            if (userRole == "Financeiro") throw new InvalidOperationException("Utilizadores do perfil Financeiro não podem criar lotes.");

            var granjaAlvo = await _context.Granjas.FindAsync(loteDto.GranjaId);
            if (granjaAlvo == null) return;

            bool temPermissao = userRole == "Administrador" || granjaAlvo.UsuarioId == userId;

            if (temPermissao)
            {
                var ultimoId = await _context.Lotes.OrderByDescending(l => l.Id).Select(l => l.Id).FirstOrDefaultAsync();
                var novoCodigo = $"LT-{(ultimoId + 1):D3}";

                var lote = new Lote
                {
                    Codigo = novoCodigo,
                    Identificador = loteDto.Identificador,
                    QuantidadeAvesInicial = loteDto.QuantidadeAvesInicial,
                    DataEntrada = loteDto.DataEntrada,
                    DataSaida = loteDto.DataSaida,
                    GranjaId = loteDto.GranjaId
                };
                await _context.Lotes.AddAsync(lote);
                await _context.SaveChangesAsync();
                await _auditoriaService.RegistrarLog("CRIACAO_LOTE", $"Lote '{lote.Identificador}' (Código: {lote.Codigo}) criado na Granja ID: {lote.GranjaId}.");
            }
        }

        public async Task<bool> UpdateAsync(int id, UpdateLoteDto loteDto)
        {
            var (userId, userRole) = GetCurrentUser();
            if (userRole == "Financeiro") throw new InvalidOperationException("Utilizadores do perfil Financeiro não podem editar lotes.");

            var loteExistente = await GetByIdAsync(id);
            if (loteExistente == null) return false;

            loteExistente.Identificador = loteDto.Identificador;
            loteExistente.QuantidadeAvesInicial = loteDto.QuantidadeAvesInicial;
            loteExistente.DataEntrada = loteDto.DataEntrada;
            loteExistente.DataSaida = loteDto.DataSaida;
            loteExistente.GranjaId = loteDto.GranjaId;

            _context.Lotes.Update(loteExistente);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("ATUALIZACAO_LOTE", $"Lote '{loteExistente.Identificador}' (ID: {loteExistente.Id}) atualizado.");
            return true;
        }

        public async Task DeleteAsync(int id)
        {
            var (userId, userRole) = GetCurrentUser();
            if (userRole == "Financeiro") throw new InvalidOperationException("Utilizadores do perfil Financeiro não podem deletar lotes.");

            var loteParaDeletar = await GetByIdAsync(id);
            if (loteParaDeletar != null)
            {
                _context.Lotes.Remove(loteParaDeletar);
                await _context.SaveChangesAsync();
                await _auditoriaService.RegistrarLog("DELECAO_LOTE", $"Lote '{loteParaDeletar.Identificador}' (ID: {id}) deletado.");
            }
        }
    }
}
