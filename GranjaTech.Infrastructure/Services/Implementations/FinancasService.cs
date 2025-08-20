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
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoleClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(userRoleClaim))
            {
                throw new InvalidOperationException("Não foi possível identificar o utilizador logado.");
            }
            return (int.Parse(userIdClaim), userRoleClaim);
        }

        public async Task<IEnumerable<TransacaoFinanceira>> GetAllAsync()
        {
            var (userId, userRole) = GetCurrentUser();

            IQueryable<TransacaoFinanceira> query = _context.TransacoesFinanceiras
                .Include(t => t.Lote).ThenInclude(l => l.Granja)
                .Include(t => t.Usuario);

            if (userRole == "Administrador") { }
            else if (userRole == "Produtor")
            {
                query = query.Where(t => t.Lote != null && t.Lote.Granja.UsuarioId == userId);
            }
            else if (userRole == "Financeiro")
            {
                var produtorIds = await _context.FinanceiroProdutor
                    .Where(fp => fp.FinanceiroId == userId)
                    .Select(fp => fp.ProdutorId)
                    .ToListAsync();

                if (produtorIds.Any())
                {
                    query = query.Where(t => t.Lote != null && produtorIds.Contains(t.Lote.Granja.UsuarioId));
                }
                else
                {
                    return new List<TransacaoFinanceira>();
                }
            }
            else
            {
                return new List<TransacaoFinanceira>();
            }

            return await query.OrderByDescending(t => t.Data).ToListAsync();
        }

        public async Task AddAsync(CreateTransacaoDto transacaoDto)
        {
            var transacao = new TransacaoFinanceira
            {
                Descricao = transacaoDto.Descricao,
                Valor = transacaoDto.Valor,
                Tipo = transacaoDto.Tipo,
                Data = transacaoDto.Data,
                LoteId = transacaoDto.LoteId == 0 ? null : transacaoDto.LoteId,
                UsuarioId = GetCurrentUser().userId,
                TimestampCriacao = DateTime.UtcNow
            };

            await _context.TransacoesFinanceiras.AddAsync(transacao);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("CRIACAO_TRANSACAO", $"Transação '{transacao.Descricao}' (ID: {transacao.Id}) criada.");
        }

        public async Task<bool> UpdateAsync(int id, UpdateTransacaoDto transacaoDto)
        {
            var (currentUserId, currentUserRole) = GetCurrentUser();

            var transacaoExistente = await _context.TransacoesFinanceiras
                .Include(t => t.Usuario)
                .ThenInclude(u => u.Perfil)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transacaoExistente == null) return false;

            // REGRA 1: Verifica o limite de 5 minutos, A MENOS QUE o utilizador seja Administrador
            if (currentUserRole != "Administrador" && DateTime.UtcNow - transacaoExistente.TimestampCriacao > TimeSpan.FromMinutes(5))
            {
                throw new InvalidOperationException("O tempo para edição expirou. A transação só pode ser editada nos primeiros 5 minutos.");
            }

            // REGRA 2: Verifica a hierarquia de permissão
            if (currentUserRole == "Financeiro" && transacaoExistente.Usuario.Perfil.Nome == "Administrador")
            {
                throw new InvalidOperationException("Permissão negada. Um utilizador Financeiro não pode editar uma transação criada por um Administrador.");
            }

            transacaoExistente.Descricao = transacaoDto.Descricao;
            transacaoExistente.Valor = transacaoDto.Valor;
            transacaoExistente.Tipo = transacaoDto.Tipo;
            transacaoExistente.Data = transacaoDto.Data;
            transacaoExistente.LoteId = transacaoDto.LoteId == 0 ? null : transacaoDto.LoteId;

            _context.TransacoesFinanceiras.Update(transacaoExistente);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("ATUALIZACAO_TRANSACAO", $"Transação (ID: {id}) atualizada.");
            return true;
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
