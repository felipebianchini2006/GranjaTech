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
    public class GranjaService : IGranjaService
    {
        private readonly GranjaTechDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditoriaService _auditoriaService;

        public GranjaService(GranjaTechDbContext context, IHttpContextAccessor httpContextAccessor, IAuditoriaService auditoriaService)
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

        public async Task<IEnumerable<Granja>> GetAllAsync()
        {
            var (userId, userRole) = GetCurrentUser();
            IQueryable<Granja> query = _context.Granjas.Include(g => g.Usuario);

            if (userRole == "Administrador") { }
            else if (userRole == "Produtor")
            {
                query = query.Where(g => g.UsuarioId == userId);
            }
            else if (userRole == "Financeiro")
            {
                var produtorIds = await _context.FinanceiroProdutor.Where(fp => fp.FinanceiroId == userId).Select(fp => fp.ProdutorId).ToListAsync();
                if (produtorIds.Any()) { query = query.Where(g => produtorIds.Contains(g.UsuarioId)); } else { return new List<Granja>(); }
            }
            else { return new List<Granja>(); }
            return await query.ToListAsync();
        }

        public async Task<Granja?> GetByIdAsync(int id)
        {
            var granja = await _context.Granjas.Include(g => g.Usuario).FirstOrDefaultAsync(g => g.Id == id);
            if (granja == null) return null;

            var (userId, userRole) = GetCurrentUser();

            if (userRole == "Administrador" || granja.UsuarioId == userId) return granja;

            if (userRole == "Financeiro")
            {
                var produtorIds = await _context.FinanceiroProdutor.Where(fp => fp.FinanceiroId == userId).Select(fp => fp.ProdutorId).ToListAsync();
                if (produtorIds.Contains(granja.UsuarioId)) return granja;
            }

            return null;
        }

        public async Task AddAsync(CreateGranjaDto granjaDto)
        {
            var (userId, userRole) = GetCurrentUser();
            if (userRole == "Financeiro") throw new InvalidOperationException("Usuários do perfil Financeiro não podem criar granjas.");

            var ultimoId = await _context.Granjas.OrderByDescending(g => g.Id).Select(g => g.Id).FirstOrDefaultAsync();
            var novoCodigo = $"GRJ-{(ultimoId + 1):D3}";

            var novaGranja = new Granja
            {
                Codigo = novoCodigo,
                Nome = granjaDto.Nome,
                Localizacao = granjaDto.Localizacao
            };

            if (userRole == "Administrador" && granjaDto.UsuarioId.HasValue)
            {
                novaGranja.UsuarioId = granjaDto.UsuarioId.Value;
            }
            else
            {
                novaGranja.UsuarioId = userId;
            }

            await _context.Granjas.AddAsync(novaGranja);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("CRIACAO_GRANJA", $"Granja '{novaGranja.Nome}' (Código: {novaGranja.Codigo}) criada.");
        }

        public async Task<bool> UpdateAsync(int id, UpdateGranjaDto granjaDto)
        {
            var (userId, userRole) = GetCurrentUser();
            if (userRole == "Financeiro") throw new InvalidOperationException("Usuários do perfil Financeiro não podem editar granjas.");

            var granjaExistente = await GetByIdAsync(id);
            if (granjaExistente == null) return false;

            granjaExistente.Nome = granjaDto.Nome;
            granjaExistente.Localizacao = granjaDto.Localizacao;

            if (userRole == "Administrador")
            {
                granjaExistente.UsuarioId = granjaDto.UsuarioId;
            }

            _context.Granjas.Update(granjaExistente);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("ATUALIZACAO_GRANJA", $"Granja '{granjaExistente.Nome}' (ID: {id}) atualizada.");
            return true;
        }

        public async Task DeleteAsync(int id)
        {
            var (userId, userRole) = GetCurrentUser();
            if (userRole == "Financeiro") throw new InvalidOperationException("Usuários do perfil Financeiro não podem deletar granjas.");

            var granjaParaDeletar = await GetByIdAsync(id);
            if (granjaParaDeletar != null)
            {
                _context.Granjas.Remove(granjaParaDeletar);
                await _context.SaveChangesAsync();
                await _auditoriaService.RegistrarLog("DELECAO_GRANJA", $"Granja '{granjaParaDeletar.Nome}' (ID: {id}) deletada.");
            }
        }
    }
}
