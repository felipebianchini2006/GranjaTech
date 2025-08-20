using GranjaTech.Application.DTOs; // Adicione este using
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
    public class EstoqueService : IEstoqueService
    {
        private readonly GranjaTechDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditoriaService _auditoriaService;

        public EstoqueService(GranjaTechDbContext context, IHttpContextAccessor httpContextAccessor, IAuditoriaService auditoriaService)
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

        public async Task<IEnumerable<Produto>> GetAllAsync()
        {
            var (userId, userRole) = GetCurrentUser();
            IQueryable<Produto> query = _context.Produtos.Include(p => p.Granja).ThenInclude(g => g.Usuario);

            if (userRole == "Administrador") { }
            else if (userRole == "Produtor")
            {
                query = query.Where(p => p.Granja.UsuarioId == userId);
            }
            else
            {
                return new List<Produto>();
            }
            return await query.ToListAsync();
        }

        public async Task AddAsync(CreateProdutoDto produtoDto)
        {
            var (userId, userRole) = GetCurrentUser();
            var granja = await _context.Granjas.FindAsync(produtoDto.GranjaId);
            if (granja == null || (userRole == "Produtor" && granja.UsuarioId != userId))
            {
                throw new InvalidOperationException("Permissão negada ou granja inválida.");
            }

            var produto = new Produto
            {
                Nome = produtoDto.Nome,
                Tipo = produtoDto.Tipo,
                Quantidade = produtoDto.Quantidade,
                UnidadeDeMedida = produtoDto.UnidadeDeMedida,
                GranjaId = produtoDto.GranjaId
            };

            await _context.Produtos.AddAsync(produto);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("CRIACAO_PRODUTO_ESTOQUE", $"Produto '{produto.Nome}' (ID: {produto.Id}) adicionado à Granja ID: {produto.GranjaId}.");
        }

        public async Task<bool> UpdateAsync(int id, Produto produto)
        {
            var (userId, userRole) = GetCurrentUser();
            var produtoExistente = await _context.Produtos.Include(p => p.Granja).FirstOrDefaultAsync(p => p.Id == id);
            if (produtoExistente == null) return false;

            if (userRole == "Produtor" && produtoExistente.Granja.UsuarioId != userId)
            {
                throw new InvalidOperationException("Permissão negada.");
            }

            produtoExistente.Nome = produto.Nome;
            produtoExistente.Tipo = produto.Tipo;
            produtoExistente.Quantidade = produto.Quantidade;
            produtoExistente.UnidadeDeMedida = produto.UnidadeDeMedida;

            _context.Produtos.Update(produtoExistente);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("ATUALIZACAO_PRODUTO_ESTOQUE", $"Produto '{produto.Nome}' (ID: {id}) atualizado.");
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var (userId, userRole) = GetCurrentUser();
            var produto = await _context.Produtos.Include(p => p.Granja).FirstOrDefaultAsync(p => p.Id == id);
            if (produto == null) return false;

            if (userRole == "Produtor" && produto.Granja.UsuarioId != userId)
            {
                throw new InvalidOperationException("Permissão negada.");
            }

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("DELECAO_PRODUTO_ESTOQUE", $"Produto '{produto.Nome}' (ID: {id}) deletado.");
            return true;
        }
    }
}
