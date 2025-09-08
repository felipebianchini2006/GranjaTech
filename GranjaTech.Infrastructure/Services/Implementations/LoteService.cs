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

            IQueryable<Lote> query = _context.Lotes
                .Include(l => l.Granja)
                    .ThenInclude(g => g.Usuario);

            if (userRole == "Administrador")
            {
                // sem filtro
            }
            else if (userRole == "Produtor")
            {
                query = query.Where(l => l.Granja.UsuarioId == userId);
            }
            else if (userRole == "Financeiro")
            {
                var produtorIds = await _context.FinanceiroProdutor
                    .Where(fp => fp.FinanceiroId == userId)
                    .Select(fp => fp.ProdutorId)
                    .ToListAsync();

                if (produtorIds.Any())
                    query = query.Where(l => produtorIds.Contains(l.Granja.UsuarioId));
                else
                    return new List<Lote>();
            }
            else
            {
                return new List<Lote>();
            }

            return await query.ToListAsync();
        }

        public async Task<Lote?> GetByIdAsync(int id)
        {
            var lote = await _context.Lotes
                .Include(l => l.Granja)
                    .ThenInclude(g => g.Usuario)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lote == null) return null;

            var (userId, userRole) = GetCurrentUser();

            if (userRole == "Administrador" || lote.Granja.UsuarioId == userId) return lote;

            if (userRole == "Financeiro")
            {
                var produtorIds = await _context.FinanceiroProdutor
                    .Where(fp => fp.FinanceiroId == userId)
                    .Select(fp => fp.ProdutorId)
                    .ToListAsync();

                if (produtorIds.Contains(lote.Granja.UsuarioId)) return lote;
            }

            return null;
        }

        public async Task AddAsync(CreateLoteDto loteDto)
        {
            var (userId, userRole) = GetCurrentUser();
            if (userRole == "Financeiro")
                throw new InvalidOperationException("Utilizadores do perfil Financeiro não podem criar lotes.");

            var granjaAlvo = await _context.Granjas.FindAsync(loteDto.GranjaId);
            if (granjaAlvo == null) return;

            bool temPermissao = userRole == "Administrador" || granjaAlvo.UsuarioId == userId;

            if (temPermissao)
            {
                var ultimoId = await _context.Lotes
                    .OrderByDescending(l => l.Id)
                    .Select(l => l.Id)
                    .FirstOrDefaultAsync();

                var novoCodigo = $"LT-{(ultimoId + 1):D3}";

                var lote = new Lote
                {
                    Codigo = novoCodigo,
                    Identificador = loteDto.Identificador,
                    QuantidadeAvesInicial = loteDto.QuantidadeAvesInicial,
                    QuantidadeAvesAtual = loteDto.QuantidadeAvesInicial, // inicia aves vivas = aves iniciais
                    DataEntrada = loteDto.DataEntrada,
                    DataSaida = loteDto.DataSaida,
                    GranjaId = loteDto.GranjaId
                };

                await _context.Lotes.AddAsync(lote);
                await _context.SaveChangesAsync();

                await _auditoriaService.RegistrarLog(
                    "CRIACAO_LOTE",
                    $"Lote '{lote.Identificador}' (Código: {lote.Codigo}) criado na Granja ID: {lote.GranjaId}.");
            }
        }

        public async Task<bool> UpdateAsync(int id, UpdateLoteDto loteDto)
        {
            var (userId, userRole) = GetCurrentUser();
            if (userRole == "Financeiro")
                throw new InvalidOperationException("Utilizadores do perfil Financeiro não podem editar lotes.");

            var loteExistente = await GetByIdAsync(id);
            if (loteExistente == null) return false;

            loteExistente.Identificador = loteDto.Identificador;
            loteExistente.DataEntrada = loteDto.DataEntrada;
            loteExistente.DataSaida = loteDto.DataSaida;
            loteExistente.GranjaId = loteDto.GranjaId;

            // ajusta atual quando o inicial mudar
            if (loteDto.QuantidadeAvesInicial > 0 && loteDto.QuantidadeAvesInicial != loteExistente.QuantidadeAvesInicial)
            {
                var delta = loteDto.QuantidadeAvesInicial - loteExistente.QuantidadeAvesInicial;
                loteExistente.QuantidadeAvesInicial = loteDto.QuantidadeAvesInicial;

                if (loteExistente.QuantidadeAvesAtual + delta >= 0)
                    loteExistente.QuantidadeAvesAtual += delta;
            }

            _context.Lotes.Update(loteExistente);
            await _context.SaveChangesAsync();

            await _auditoriaService.RegistrarLog(
                "ATUALIZACAO_LOTE",
                $"Lote '{loteExistente.Identificador}' (ID: {loteExistente.Id}) atualizado.");

            return true;
        }

        public async Task DeleteAsync(int id)
        {
            var (userId, userRole) = GetCurrentUser();
            if (userRole == "Financeiro")
                throw new InvalidOperationException("Utilizadores do perfil Financeiro não podem deletar lotes.");

            var loteParaDeletar = await GetByIdAsync(id);
            if (loteParaDeletar != null)
            {
                _context.Lotes.Remove(loteParaDeletar);
                await _context.SaveChangesAsync();

                await _auditoriaService.RegistrarLog(
                    "DELECAO_LOTE",
                    $"Lote '{loteParaDeletar.Identificador}' (ID: {id}) deletado.");
            }
        }

        // ===================== MORTALIDADE =====================

        public async Task<RegistroMortalidade> RegistrarMortalidadeAsync(CreateRegistroMortalidadeDto dto, int loteIdFromRoute)
        {
            var loteId = dto.LoteId ?? loteIdFromRoute;

            var lote = await _context.Lotes.FirstOrDefaultAsync(l => l.Id == loteId);
            if (lote == null) throw new InvalidOperationException("Lote não encontrado.");

            if (dto.Quantidade <= 0) throw new InvalidOperationException("Quantidade inválida.");

            // trava para não ficar negativo
            var qtd = Math.Min(dto.Quantidade, lote.QuantidadeAvesAtual);

            var registro = new RegistroMortalidade
            {
                LoteId = lote.Id,
                Data = dto.Data,
                Quantidade = qtd,
                Motivo = dto.Motivo,
                Setor = dto.Setor,
                Observacoes = dto.Observacoes,
                ResponsavelRegistro = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email)
            };

            _context.RegistrosMortalidade.Add(registro);

            // baixa no lote
            lote.QuantidadeAvesAtual -= qtd;
            lote.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditoriaService.RegistrarLog(
                "REGISTRO_MORTALIDADE",
                $"Lote '{lote.Identificador}' (ID {lote.Id}) - {qtd} aves registradas como mortas em {dto.Data:yyyy-MM-dd}."
            );

            return registro;
        }

        public async Task<IEnumerable<RegistroMortalidade>> ListarMortalidadesAsync(int loteId)
        {
            return await _context.RegistrosMortalidade
                .Where(r => r.LoteId == loteId)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }
    }
}
