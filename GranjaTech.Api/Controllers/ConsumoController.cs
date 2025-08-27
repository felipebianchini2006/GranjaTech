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
    public class ConsumoController : ControllerBase
    {
        private readonly GranjaTechDbContext _context;

        public ConsumoController(GranjaTechDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Registra consumo de ração
        /// </summary>
        [HttpPost("racao")]
        public async Task<IActionResult> RegistrarConsumoRacao(CreateConsumoRacaoDto dto)
        {
            try
            {
                // Validar se o lote existe e está ativo
                var lote = await _context.Lotes.FindAsync(dto.LoteId);
                if (lote == null || lote.Status != "Ativo")
                {
                    return BadRequest(new { message = "Lote não encontrado ou inativo" });
                }

                // Validar se aves vivas não excede quantidade atual
                if (dto.AvesVivas > lote.QuantidadeAvesAtual)
                {
                    return BadRequest(new { message = "Número de aves vivas não pode exceder quantidade atual do lote" });
                }

                var consumo = new ConsumoRacao
                {
                    LoteId = dto.LoteId,
                    Data = dto.Data,
                    QuantidadeKg = dto.QuantidadeKg,
                    TipoRacao = dto.TipoRacao,
                    AvesVivas = dto.AvesVivas,
                    Observacoes = dto.Observacoes
                };

                _context.ConsumosRacao.Add(consumo);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Consumo de ração registrado com sucesso", id = consumo.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao registrar consumo de ração", error = ex.Message });
            }
        }

        /// <summary>
        /// Registra consumo de água
        /// </summary>
        [HttpPost("agua")]
        public async Task<IActionResult> RegistrarConsumoAgua(CreateConsumoAguaDto dto)
        {
            try
            {
                var lote = await _context.Lotes.FindAsync(dto.LoteId);
                if (lote == null || lote.Status != "Ativo")
                {
                    return BadRequest(new { message = "Lote não encontrado ou inativo" });
                }

                if (dto.AvesVivas > lote.QuantidadeAvesAtual)
                {
                    return BadRequest(new { message = "Número de aves vivas não pode exceder quantidade atual do lote" });
                }

                var consumo = new ConsumoAgua
                {
                    LoteId = dto.LoteId,
                    Data = dto.Data,
                    QuantidadeLitros = dto.QuantidadeLitros,
                    AvesVivas = dto.AvesVivas,
                    TemperaturaAmbiente = dto.TemperaturaAmbiente,
                    Observacoes = dto.Observacoes
                };

                _context.ConsumosAgua.Add(consumo);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Consumo de água registrado com sucesso", id = consumo.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao registrar consumo de água", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém consumos de ração de um lote
        /// </summary>
        [HttpGet("racao/{loteId}")]
        public async Task<IActionResult> GetConsumosRacao(int loteId, [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
        {
            try
            {
                var query = _context.ConsumosRacao
                    .Where(c => c.LoteId == loteId)
                    .Include(c => c.Lote)
                    .AsQueryable();

                if (dataInicio.HasValue)
                    query = query.Where(c => c.Data >= dataInicio.Value);

                if (dataFim.HasValue)
                    query = query.Where(c => c.Data <= dataFim.Value);

                var consumos = await query
                    .OrderBy(c => c.Data)
                    .Select(c => new
                    {
                        c.Id,
                        c.Data,
                        c.QuantidadeKg,
                        c.TipoRacao,
                        c.AvesVivas,
                        ConsumoPorAveGramas = c.ConsumoPorAveGramas,
                        c.Observacoes
                    })
                    .ToListAsync();

                return Ok(consumos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao obter consumos de ração", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém consumos de água de um lote
        /// </summary>
        [HttpGet("agua/{loteId}")]
        public async Task<IActionResult> GetConsumosAgua(int loteId, [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
        {
            try
            {
                var query = _context.ConsumosAgua
                    .Where(c => c.LoteId == loteId)
                    .Include(c => c.Lote)
                    .AsQueryable();

                if (dataInicio.HasValue)
                    query = query.Where(c => c.Data >= dataInicio.Value);

                if (dataFim.HasValue)
                    query = query.Where(c => c.Data <= dataFim.Value);

                var consumos = await query
                    .OrderBy(c => c.Data)
                    .Select(c => new
                    {
                        c.Id,
                        c.Data,
                        c.QuantidadeLitros,
                        c.AvesVivas,
                        ConsumoPorAveMl = c.ConsumoPorAveMl,
                        c.TemperaturaAmbiente,
                        c.Observacoes
                    })
                    .ToListAsync();

                return Ok(consumos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao obter consumos de água", error = ex.Message });
            }
        }

        /// <summary>
        /// Resumo de consumo de um lote
        /// </summary>
        [HttpGet("resumo/{loteId}")]
        public async Task<IActionResult> GetResumoConsumo(int loteId)
        {
            try
            {
                var lote = await _context.Lotes
                    .Include(l => l.ConsumosRacao)
                    .Include(l => l.ConsumosAgua)
                    .FirstOrDefaultAsync(l => l.Id == loteId);

                if (lote == null)
                {
                    return NotFound(new { message = "Lote não encontrado" });
                }

                var resumo = new
                {
                    LoteId = lote.Id,
                    LoteIdentificador = lote.Identificador,
                    IdadeDias = lote.IdadeAtualDias,
                    
                    ConsumoRacao = new
                    {
                        TotalKg = lote.ConsumosRacao.Sum(c => c.QuantidadeKg),
                        MediaPorAve = lote.ConsumosRacao.Any() ? 
                            lote.ConsumosRacao.Average(c => c.ConsumoPorAveGramas) : 0,
                        UltimoRegistro = lote.ConsumosRacao.Any() ?
                            lote.ConsumosRacao.OrderByDescending(c => c.Data).First().Data : (DateTime?)null
                    },
                    
                    ConsumoAgua = new
                    {
                        TotalLitros = lote.ConsumosAgua.Sum(c => c.QuantidadeLitros),
                        MediaPorAve = lote.ConsumosAgua.Any() ?
                            lote.ConsumosAgua.Average(c => c.ConsumoPorAveMl) : 0,
                        UltimoRegistro = lote.ConsumosAgua.Any() ?
                            lote.ConsumosAgua.OrderByDescending(c => c.Data).First().Data : (DateTime?)null
                    },
                    
                    RelacaoAguaRacao = lote.ConsumosRacao.Sum(c => c.QuantidadeKg) > 0 ?
                        lote.ConsumosAgua.Sum(c => c.QuantidadeLitros) / lote.ConsumosRacao.Sum(c => c.QuantidadeKg) : 0
                };

                return Ok(resumo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao obter resumo de consumo", error = ex.Message });
            }
        }
    }

    // DTO para consumo de água
    public class CreateConsumoAguaDto
    {
        public int LoteId { get; set; }
        public DateTime Data { get; set; }
        public decimal QuantidadeLitros { get; set; }
        public int AvesVivas { get; set; }
        public decimal? TemperaturaAmbiente { get; set; }
        public string? Observacoes { get; set; }
    }
}
