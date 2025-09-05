using GranjaTech.Application.DTOs.Relatorios;
using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GranjaTech.Infrastructure.Services;

public sealed class RelatorioAvancadoService : IRelatorioAvancadoService
{
    private readonly GranjaTechDbContext _db;
    public RelatorioAvancadoService(GranjaTechDbContext db) => _db = db;

    public async Task<FinanceReportDto> FinanceiroAsync(int granjaId, DateTime inicioUtc, DateTime fimUtc, CancellationToken ct)
    {
        // TransacaoFinanceira filtra via Lote.GranjaId (quando houver Lote)
        var q = _db.TransacoesFinanceiras
            .AsNoTracking()
            .Include(t => t.Lote)
            .Where(t =>
                t.Data >= inicioUtc && t.Data < fimUtc &&
                (t.Lote != null ? t.Lote.GranjaId == granjaId : true) // se não houver lote, permita; ajuste se necessário
            );

        var itens = await q
            .OrderBy(t => t.Data)
            .Select(t => new FinanceReportItemDto
            {
                Data = t.Data,
                Categoria = t.Tipo.ToLower(), // "entrada" | "saida"
                Descricao = t.Descricao,
                Valor = t.Valor
            })
            .ToListAsync(ct);

        var entradas = itens.Where(i => i.Categoria == "entrada").Sum(i => i.Valor);
        var saidas = itens.Where(i => i.Categoria == "saida").Sum(i => i.Valor);

        return new FinanceReportDto
        {
            GranjaId = granjaId,
            Inicio = inicioUtc,
            Fim = fimUtc,
            TotalEntradas = entradas,
            TotalSaidas = saidas,
            Saldo = entradas - saidas,
            Itens = itens
        };
    }

    public async Task<GeralReportDto> GeralAsync(int granjaId, DateTime inicioUtc, DateTime fimUtc, CancellationToken ct)
    {
        var lotes = await _db.Lotes
            .AsNoTracking()
            .Where(l => l.GranjaId == granjaId)
            .Select(l => new
            {
                l.Id,
                ConsumoRacao = l.ConsumosRacao
                    .Where(c => c.Data >= inicioUtc && c.Data < fimUtc)
                    .Select(c => new { Dia = c.Data.Date, Racao = c.QuantidadeKg, Aves = c.AvesVivas }),
                ConsumoAgua = l.ConsumosAgua
                    .Where(c => c.Data >= inicioUtc && c.Data < fimUtc)
                    .Select(c => new { Dia = c.Data.Date, Agua = c.QuantidadeLitros, Aves = c.AvesVivas }),
                Pesagens = l.PesagensSemanais
                    .Where(p => p.DataPesagem >= inicioUtc && p.DataPesagem < fimUtc)
                    .Select(p => new { p.DataPesagem, p.PesoMedioGramas, p.QuantidadeAmostrada }),
                Sanitarios = l.EventosSanitarios
                    .Where(e => e.Data >= inicioUtc && e.Data < fimUtc)
                    .Select(e => new { e.Data, e.TipoEvento, e.Produto, e.ViaAdministracao })
            })
            .ToListAsync(ct);

        var geral = new GeralReportDto
        {
            GranjaId = granjaId,
            Inicio = inicioUtc,
            Fim = fimUtc
        };

        // Consumo: padroniza como decimal para evitar conflitos e faz Concat no IEnumerable
        var racao = lotes.SelectMany(l => l.ConsumoRacao)
            .Select(x => new { x.Dia, R = x.Racao, A = 0m, V = (decimal)Math.Max(0, x.Aves) })
            .AsEnumerable();

        var agua = lotes.SelectMany(l => l.ConsumoAgua)
            .Select(x => new { x.Dia, R = 0m, A = x.Agua, V = (decimal)Math.Max(0, x.Aves) })
            .AsEnumerable();

        geral.Consumo = racao
            .Concat(agua)
            .GroupBy(x => x.Dia)
            .OrderBy(g => g.Key)
            .Select(g => new ConsumoResumoDto
            {
                Data = g.Key,
                RacaoKg = (double)g.Sum(x => x.R),
                AguaLitros = (double)g.Sum(x => x.A),
                AvesVivas = (int)Math.Round(g.Any() ? g.Average(x => x.V) : 0m)
            })
            .ToList();

        // Pesagens: gramas -> kg
        geral.Pesagens = lotes.SelectMany(l => l.Pesagens)
            .OrderBy(p => p.DataPesagem)
            .Select(p => new PesagemResumoDto
            {
                Data = p.DataPesagem,
                PesoMedioKg = Convert.ToDouble(p.PesoMedioGramas) / 1000.0,
                Amostra = p.QuantidadeAmostrada
            })
            .ToList();

        // Sanitário
        geral.Sanitario = lotes.SelectMany(l => l.Sanitarios)
            .OrderBy(e => e.Data)
            .Select(e => new SanitarioResumoDto
            {
                Data = e.Data,
                TipoEvento = e.TipoEvento,
                Produto = e.Produto,
                Via = e.ViaAdministracao
            })
            .ToList();

        // Sensores: usar LeituraSensor.Timestamp e Valor, via Sensor.GranjaId
        geral.Sensores = await _db.LeiturasSensores
            .AsNoTracking()
            .Include(ls => ls.Sensor)
            .Where(ls => ls.Sensor.GranjaId == granjaId
                      && ls.Timestamp >= inicioUtc && ls.Timestamp < fimUtc)
            .OrderBy(ls => ls.Timestamp)
            .Select(ls => new SensorResumoDto
            {
                Data = ls.Timestamp,
                Tipo = ls.Sensor.Tipo,
                Valor = Convert.ToDouble(ls.Valor)
            })
            .ToListAsync(ct);

        return geral;
    }

    public async Task<SetorReportDto<ConsumoResumoDto>> ConsumoAsync(int granjaId, DateTime inicioUtc, DateTime fimUtc, CancellationToken ct)
    {
        var lotes = await _db.Lotes
            .AsNoTracking()
            .Where(l => l.GranjaId == granjaId)
            .Select(l => new
            {
                R = l.ConsumosRacao.Where(c => c.Data >= inicioUtc && c.Data < fimUtc)
                     .Select(c => new { Dia = c.Data.Date, R = c.QuantidadeKg, V = (decimal)Math.Max(0, c.AvesVivas) }),
                A = l.ConsumosAgua.Where(c => c.Data >= inicioUtc && c.Data < fimUtc)
                     .Select(c => new { Dia = c.Data.Date, A = c.QuantidadeLitros, V = (decimal)Math.Max(0, c.AvesVivas) })
            })
            .ToListAsync(ct);

        var racao = lotes.SelectMany(x => x.R).Select(x => new { x.Dia, R = x.R, A = 0m, x.V }).AsEnumerable();
        var agua = lotes.SelectMany(x => x.A).Select(x => new { x.Dia, R = 0m, A = x.A, x.V }).AsEnumerable();

        var itens = racao
            .Concat(agua)
            .GroupBy(x => x.Dia)
            .OrderBy(g => g.Key)
            .Select(g => new ConsumoResumoDto
            {
                Data = g.Key,
                RacaoKg = (double)g.Sum(x => x.R),
                AguaLitros = (double)g.Sum(x => x.A),
                AvesVivas = (int)Math.Round(g.Any() ? g.Average(x => x.V) : 0m)
            })
            .ToList();

        return new SetorReportDto<ConsumoResumoDto>
        {
            GranjaId = granjaId,
            Setor = "consumo",
            Inicio = inicioUtc,
            Fim = fimUtc,
            Itens = itens
        };
    }

    public async Task<SetorReportDto<PesagemResumoDto>> PesagemAsync(int granjaId, DateTime inicioUtc, DateTime fimUtc, CancellationToken ct)
    {
        var itens = await _db.Lotes
            .AsNoTracking()
            .Where(l => l.GranjaId == granjaId)
            .SelectMany(l => l.PesagensSemanais
                .Where(p => p.DataPesagem >= inicioUtc && p.DataPesagem < fimUtc))
            .OrderBy(p => p.DataPesagem)
            .Select(p => new PesagemResumoDto
            {
                Data = p.DataPesagem,
                PesoMedioKg = Convert.ToDouble(p.PesoMedioGramas) / 1000.0,
                Amostra = p.QuantidadeAmostrada
            })
            .ToListAsync(ct);

        return new SetorReportDto<PesagemResumoDto>
        {
            GranjaId = granjaId,
            Setor = "pesagem",
            Inicio = inicioUtc,
            Fim = fimUtc,
            Itens = itens
        };
    }

    public async Task<SetorReportDto<SanitarioResumoDto>> SanitarioAsync(int granjaId, DateTime inicioUtc, DateTime fimUtc, CancellationToken ct)
    {
        var itens = await _db.Lotes
            .AsNoTracking()
            .Where(l => l.GranjaId == granjaId)
            .SelectMany(l => l.EventosSanitarios
                .Where(e => e.Data >= inicioUtc && e.Data < fimUtc))
            .OrderBy(e => e.Data)
            .Select(e => new SanitarioResumoDto
            {
                Data = e.Data,
                TipoEvento = e.TipoEvento,
                Produto = e.Produto,
                Via = e.ViaAdministracao
            })
            .ToListAsync(ct);

        return new SetorReportDto<SanitarioResumoDto>
        {
            GranjaId = granjaId,
            Setor = "sanitario",
            Inicio = inicioUtc,
            Fim = fimUtc,
            Itens = itens
        };
    }

    public async Task<SetorReportDto<SensorResumoDto>> SensoresAsync(int granjaId, DateTime inicioUtc, DateTime fimUtc, CancellationToken ct)
    {
        var itens = await _db.LeiturasSensores
            .AsNoTracking()
            .Include(ls => ls.Sensor)
            .Where(ls => ls.Sensor.GranjaId == granjaId
                      && ls.Timestamp >= inicioUtc && ls.Timestamp < fimUtc)
            .OrderBy(ls => ls.Timestamp)
            .Select(ls => new SensorResumoDto
            {
                Data = ls.Timestamp,
                Tipo = ls.Sensor.Tipo,
                Valor = Convert.ToDouble(ls.Valor)
            })
            .ToListAsync(ct);

        return new SetorReportDto<SensorResumoDto>
        {
            GranjaId = granjaId,
            Setor = "sensores",
            Inicio = inicioUtc,
            Fim = fimUtc,
            Itens = itens
        };
    }
}
