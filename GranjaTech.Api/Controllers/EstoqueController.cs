using GranjaTech.Application.DTOs;
using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [Authorize(Roles = "Administrador,Produtor")]
    [ApiController]
    [Route("api/[controller]")]
    public class EstoqueController : ControllerBase
    {
        private readonly IEstoqueService _estoqueService;

        public EstoqueController(IEstoqueService estoqueService)
        {
            _estoqueService = estoqueService;
        }

        [HttpGet]
        public async Task<IActionResult> GetEstoque()
        {
            var produtos = await _estoqueService.GetAllAsync();
            return Ok(produtos);
        }

        [HttpPost]
        public async Task<IActionResult> PostProduto(CreateProdutoDto produtoDto)
        {
            try
            {
                // CORREÇÃO: Passamos o DTO diretamente para o serviço.
                // O serviço é que tem a responsabilidade de o converter para uma entidade.
                await _estoqueService.AddAsync(produtoDto);
                return Ok(new { message = "Produto adicionado ao estoque com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, Produto produto)
        {
            if (id != produto.Id)
            {
                return BadRequest();
            }
            try
            {
                var sucesso = await _estoqueService.UpdateAsync(id, produto);
                if (!sucesso) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            try
            {
                var sucesso = await _estoqueService.DeleteAsync(id);
                if (!sucesso) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
