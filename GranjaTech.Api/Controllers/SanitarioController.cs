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
    public class SanitarioController : ControllerBase
    {
        private readonly GranjaTechDbContext _context;

        public SanitarioController(GranjaTechDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Registra um novo evento sanitário
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RegistrarEvento(CreateEventoSanitarioDto dto)
        {
            try
            {
                // Validar se o lote existe e está ativo
                var lote = await _context.Lotes.FindAsync(dto.LoteId);
                if (lote == null || lote.Status != "Ativo")
                {
                    return BadRequest(new { message = "Lote não encontrado ou inativo" });
                }

                var evento = new EventoSanitario
                {
                    LoteId = dto.LoteId,
                    Data = dto.Data,
                    TipoEvento = dto.TipoEvento,
                    Produto = dto.Produto,
                    LoteProduto = dto.LoteProduto,
                    Dosagem = dto.Dosagem,
                    ViaAdministracao = dto.ViaAdministracao,
                    AvesTratadas = dto.AvesTratadas,
                    DuracaoTratamentoDias = dto.DuracaoTratamentoDias,
                    PeriodoCarenciaDias = dto.PeriodoCarenciaDias,
                    ResponsavelAplicacao = dto.ResponsavelAplicacao,
                    Sintomas = dto.Sintomas,
                    Observacoes = dto.Observacoes,
                    Custo = dto.Custo
                };

                _context.EventosSanitarios.Add(evento);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Evento sanitário registrado com sucesso", id = evento.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao registrar evento sanitário", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém eventos sanitários de um lote
        /// </summary>
        [HttpGet("{loteId}")]
        public async Task<IActionResult> GetEventos(int loteId, [FromQuery] string? tipoEvento = null)
        {
            try
            {
                var query = _context.EventosSanitarios
                    .Where(e => e.LoteId == loteId)
                    .Include(e => e.Lote)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(tipoEvento))
                    query = query.Where(e => e.TipoEvento == tipoEvento);

                var eventos = await query
                    .OrderByDescending(e => e.Data)
                    .Select(e => new
                    {
                        e.Id,
                        e.Data,
                        e.TipoEvento,
                        e.Produto,
                        e.LoteProduto,
                        e.Dosagem,
                        e.ViaAdministracao,
                        e.AvesTratadas,
                        e.DuracaoTratamentoDias,
                        e.PeriodoCarenciaDias,
                        e.ResponsavelAplicacao,
                        e.Sintomas,
                        e.Observacoes,
                        e.Custo,
                        e.DataCriacao
                    })
                    .ToListAsync();

                return Ok(eventos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao obter eventos sanitários", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém resumo sanitário de um lote
        /// </summary>
        [HttpGet("{loteId}/resumo")]
        public async Task<IActionResult> GetResumoSanitario(int loteId)
        {
            try
            {
                var lote = await _context.Lotes
                    .Include(l => l.EventosSanitarios)
                    .FirstOrDefaultAsync(l => l.Id == loteId);

                if (lote == null)
                {
                    return NotFound(new { message = "Lote não encontrado" });
                }

                var eventos = lote.EventosSanitarios.ToList();
                
                var resumo = new
                {
                    LoteId = lote.Id,
                    LoteIdentificador = lote.Identificador,
                    
                    TotalEventos = eventos.Count,
                    TotalVacinacoes = eventos.Count(e => e.TipoEvento == "Vacinacao"),
                    TotalMedicacoes = eventos.Count(e => e.TipoEvento == "Medicacao"),
                    TotalDoencas = eventos.Count(e => e.TipoEvento == "Doenca"),
                    
                    CustoTotalSanitario = eventos.Sum(e => e.Custo ?? 0),
                    CustoPorAve = lote.QuantidadeAvesAtual > 0 ? 
                        eventos.Sum(e => e.Custo ?? 0) / lote.QuantidadeAvesAtual : 0,
                    
                    EventosPorTipo = eventos
                        .GroupBy(e => e.TipoEvento)
                        .Select(g => new
                        {
                            TipoEvento = g.Key,
                            Quantidade = g.Count(),
                            CustoTotal = g.Sum(e => e.Custo ?? 0),
                            UltimaOcorrencia = g.Max(e => e.Data)
                        })
                        .ToList(),
                    
                    UltimosEventos = eventos
                        .OrderByDescending(e => e.Data)
                        .Take(5)
                        .Select(e => new
                        {
                            e.Data,
                            e.TipoEvento,
                            e.Produto,
                            e.AvesTratadas
                        })
                        .ToList(),
                    
                    ProximasAcoes = eventos
                        .Where(e => e.PeriodoCarenciaDias.HasValue && e.PeriodoCarenciaDias > 0)
                        .Select(e => new
                        {
                            Acao = $"Fim carência - {e.Produto}",
                            DataPrevista = e.Data.AddDays(e.PeriodoCarenciaDias.Value),
                            Prioridade = e.Data.AddDays(e.PeriodoCarenciaDias.Value) <= DateTime.Now.AddDays(3) ? "Alta" : "Media",
                            Descricao = $"Período de carência do {e.Produto} termina"
                        })
                        .Where(a => a.DataPrevista >= DateTime.Now)
                        .OrderBy(a => a.DataPrevista)
                        .ToList()
                };

                return Ok(resumo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao obter resumo sanitário", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém cronograma de vacinação padrão
        /// </summary>
        [HttpGet("cronograma-vacinacao")]
        public IActionResult GetCronogramaVacinacao()
        {
            var cronogramaPadrao = new[]
            {
                new { Dia = 1, Produto = "Marek + Gumboro", Via = "Subcutânea", Obrigatoria = true },
                new { Dia = 7, Produto = "Newcastle + Bronquite", Via = "Spray", Obrigatoria = true },
                new { Dia = 14, Produto = "Gumboro", Via = "Água", Obrigatoria = true },
                new { Dia = 21, Produto = "Newcastle", Via = "Água", Obrigatoria = true },
                new { Dia = 28, Produto = "Coccidiostático", Via = "Ração", Obrigatoria = false }
            };

            return Ok(cronogramaPadrao);
        }
    }
}
