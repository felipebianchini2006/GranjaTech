using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GranjaTech.Infrastructure;
using GranjaTech.Domain;
using GranjaTech.Application.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [Authorize(Roles = "Administrador,Produtor")]
    [ApiController]
    [Route("api/[controller]")]
    public class PesagemController : ControllerBase
    {
        private readonly GranjaTechDbContext _context;

        public PesagemController(GranjaTechDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Registra uma nova pesagem semanal
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RegistrarPesagem(CreatePesagemSemanalDto dto)
        {
            try
            {
                // Validar se o lote existe e está ativo
                var lote = await _context.Lotes.FindAsync(dto.LoteId);
                if (lote == null || lote.Status != "Ativo")
                {
                    return BadRequest(new { message = "Lote não encontrado ou inativo" });
                }

                var pesagem = new PesagemSemanal
                {
                    LoteId = dto.LoteId,
                    DataPesagem = dto.DataPesagem,
                    IdadeDias = dto.IdadeDias,
                    SemanaVida = dto.SemanaVida,
                    PesoMedioGramas = dto.PesoMedioGramas,
                    QuantidadeAmostrada = dto.QuantidadeAmostrada,
                    PesoMinimo = dto.PesoMinimo,
                    PesoMaximo = dto.PesoMaximo,
                    DesvioPadrao = dto.DesvioPadrao,
                    CoeficienteVariacao = dto.CoeficienteVariacao,
                    GanhoSemanal = dto.GanhoSemanal,
                    Observacoes = dto.Observacoes
                };

                _context.PesagensSemanais.Add(pesagem);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Pesagem registrada com sucesso", id = pesagem.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao registrar pesagem", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém pesagens de um lote
        /// </summary>
        [HttpGet("{loteId}")]
        public async Task<IActionResult> GetPesagens(int loteId)
        {
            try
            {
                var pesagens = await _context.PesagensSemanais
                    .Where(p => p.LoteId == loteId)
                    .Include(p => p.Lote)
                    .OrderBy(p => p.SemanaVida)
                    .Select(p => new
                    {
                        p.Id,
                        p.DataPesagem,
                        p.IdadeDias,
                        p.SemanaVida,
                        p.PesoMedioGramas,
                        p.QuantidadeAmostrada,
                        p.PesoMinimo,
                        p.PesoMaximo,
                        p.DesvioPadrao,
                        p.CoeficienteVariacao,
                        p.GanhoSemanal,
                        GanhoMedioDiario = p.GanhoMedioDiario,
                        p.Observacoes
                    })
                    .ToListAsync();

                return Ok(pesagens);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao obter pesagens", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém resumo de crescimento de um lote
        /// </summary>
        [HttpGet("{loteId}/resumo")]
        public async Task<IActionResult> GetResumoCrescimento(int loteId)
        {
            try
            {
                var lote = await _context.Lotes
                    .Include(l => l.PesagensSemanais)
                    .FirstOrDefaultAsync(l => l.Id == loteId);

                if (lote == null)
                {
                    return NotFound(new { message = "Lote não encontrado" });
                }

                var pesagens = lote.PesagensSemanais.OrderBy(p => p.SemanaVida).ToList();
                
                var resumo = new
                {
                    LoteId = lote.Id,
                    LoteIdentificador = lote.Identificador,
                    IdadeAtualDias = lote.IdadeAtualDias,
                    
                    PesoAtual = pesagens.Any() ? pesagens.Last().PesoMedioGramas : 0,
                    PesoInicial = pesagens.Any() ? pesagens.First().PesoMedioGramas : 45, // Peso padrão inicial
                    
                    GanhoTotal = pesagens.Any() ? pesagens.Last().PesoMedioGramas - (pesagens.First().PesoMedioGramas) : 0,
                    
                    GanhoMedioDiario = pesagens.Count > 1 ? 
                        pesagens.Skip(1).Average(p => p.GanhoMedioDiario) : 0,
                    
                    UniformidadeAtual = pesagens.Any() && pesagens.Last().CoeficienteVariacao.HasValue ?
                        100 - pesagens.Last().CoeficienteVariacao.Value : 0,
                    
                    TotalPesagens = pesagens.Count,
                    
                    CurvaCrescimento = pesagens.Select(p => new
                    {
                        Semana = p.SemanaVida,
                        Peso = p.PesoMedioGramas,
                        Ganho = p.GanhoSemanal ?? 0,
                        Uniformidade = p.CoeficienteVariacao.HasValue ? 100 - p.CoeficienteVariacao.Value : 0
                    }).ToList()
                };

                return Ok(resumo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao obter resumo de crescimento", error = ex.Message });
            }
        }
    }
}
