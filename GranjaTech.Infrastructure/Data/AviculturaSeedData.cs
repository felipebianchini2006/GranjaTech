using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GranjaTech.Domain;

namespace GranjaTech.Infrastructure.Data
{
    public static class AviculturaSeedData
    {
        public static void SeedAviculturaData(GranjaTechDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Iniciando seed de dados de avicultura...");

                // Verificar se já existem dados
                if (context.ConsumosRacao.Any() || context.PesagensSemanais.Any())
                {
                    logger.LogInformation("Dados de avicultura já existem. Pulando seed.");
                    return;
                }

                // Obter lotes existentes para popular dados
                var lotes = context.Lotes.Include(l => l.Granja).ToList();
                if (!lotes.Any())
                {
                    logger.LogWarning("Nenhum lote encontrado para popular dados de avicultura.");
                    return;
                }

                var random = new Random(42); // Seed fixo para consistência

                foreach (var lote in lotes.Take(3)) // Popular apenas os primeiros 3 lotes
                {
                    PopularDadosLote(context, lote, random, logger);
                }

                context.SaveChanges();
                logger.LogInformation("Seed de dados de avicultura concluído com sucesso!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro durante o seed de dados de avicultura");
                throw;
            }
        }

        private static void PopularDadosLote(GranjaTechDbContext context, Lote lote, Random random, ILogger logger)
        {
            logger.LogInformation("Populando dados para lote {LoteId} - {Identificador}", lote.Id, lote.Identificador);

            // Atualizar lote com dados realistas
            lote.QuantidadeAvesAtual = random.Next(1800, 2000); // Simular algumas mortes
            lote.AreaGalpao = 120m; // 120 m²
            lote.Linhagem = "Cobb 500";
            lote.OrigemPintinhos = "Granja Matriz São Paulo";
            lote.Status = "Ativo";

            var dataInicio = lote.DataEntrada;
            var idadeAtual = Math.Min((DateTime.Now - dataInicio).Days, 42); // Máximo 42 dias

            // 1. Pesagens Semanais (dados realistas de crescimento)
            PopularPesagensSemanais(context, lote, dataInicio, idadeAtual, random, logger);

            // 2. Consumo de Ração (dados realistas por fase)
            PopularConsumoRacao(context, lote, dataInicio, idadeAtual, random, logger);

            // 3. Consumo de Água
            PopularConsumoAgua(context, lote, dataInicio, idadeAtual, random, logger);

            // 4. Mortalidade
            PopularRegistrosMortalidade(context, lote, dataInicio, idadeAtual, random, logger);

            // 5. Eventos Sanitários
            PopularEventosSanitarios(context, lote, dataInicio, idadeAtual, random, logger);

            // 6. Qualidade do Ar
            PopularQualidadeAr(context, lote, dataInicio, idadeAtual, random, logger);
        }

        private static void PopularPesagensSemanais(GranjaTechDbContext context, Lote lote, DateTime dataInicio, int idadeAtual, Random random, ILogger logger)
        {
            // Curva de crescimento realística (Cobb 500)
            var pesosPadrao = new Dictionary<int, decimal>
            {
                [7] = 180m,   // 1 semana
                [14] = 410m,  // 2 semanas
                [21] = 950m,  // 3 semanas
                [28] = 1500m, // 4 semanas
                [35] = 2100m, // 5 semanas
                [42] = 2700m  // 6 semanas
            };

            var semanaAtual = Math.Min(idadeAtual / 7, 6);

            for (int semana = 1; semana <= semanaAtual; semana++)
            {
                var diaPesagem = semana * 7;
                var dataPesagem = dataInicio.AddDays(diaPesagem);

                if (!pesosPadrao.ContainsKey(diaPesagem)) continue;

                var pesoMedio = pesosPadrao[diaPesagem];
                // Adicionar variação de ±5%
                pesoMedio += pesoMedio * (decimal)(random.NextDouble() * 0.1 - 0.05);

                var pesagem = new PesagemSemanal
                {
                    LoteId = lote.Id,
                    DataPesagem = dataPesagem,
                    IdadeDias = diaPesagem,
                    SemanaVida = semana,
                    PesoMedioGramas = Math.Round(pesoMedio, 0),
                    QuantidadeAmostrada = 50,
                    PesoMinimo = Math.Round(pesoMedio * 0.8m, 0),
                    PesoMaximo = Math.Round(pesoMedio * 1.2m, 0),
                    DesvioPadrao = Math.Round(pesoMedio * 0.1m, 2),
                    CoeficienteVariacao = random.Next(8, 15), // 8-15% é normal
                    GanhoSemanal = semana > 1 ? Math.Round(pesoMedio - pesosPadrao[(semana - 1) * 7], 0) : pesoMedio - 45,
                    Observacoes = $"Pesagem da {semana}ª semana - desenvolvimento normal"
                };

                context.PesagensSemanais.Add(pesagem);
            }

            logger.LogInformation("Adicionadas {Count} pesagens semanais para lote {LoteId}", semanaAtual, lote.Id);
        }

        private static void PopularConsumoRacao(GranjaTechDbContext context, Lote lote, DateTime dataInicio, int idadeAtual, Random random, ILogger logger)
        {
            var consumosPorDia = new Dictionary<int, (string fase, decimal consumoPorAve)>
            {
                [1] = ("Inicial", 15m),    // 15g/ave/dia
                [7] = ("Inicial", 30m),    // 30g/ave/dia
                [14] = ("Crescimento", 65m), // 65g/ave/dia
                [21] = ("Crescimento", 110m), // 110g/ave/dia
                [28] = ("Terminação", 160m),  // 160g/ave/dia
                [35] = ("Terminação", 180m),  // 180g/ave/dia
                [42] = ("Terminação", 190m)   // 190g/ave/dia
            };

            for (int dia = 1; dia <= Math.Min(idadeAtual, 42); dia++)
            {
                var dataConsumo = dataInicio.AddDays(dia);
                
                // Encontrar o consumo base para o dia
                var (fase, consumoBase) = consumosPorDia
                    .Where(kvp => kvp.Key <= dia)
                    .OrderByDescending(kvp => kvp.Key)
                    .First().Value;

                // Interpolar entre pontos para suavizar
                var proximoPonto = consumosPorDia.Where(kvp => kvp.Key > dia).FirstOrDefault();
                if (proximoPonto.Key != 0)
                {
                    var diasEntrePontos = proximoPonto.Key - dia;
                    var pontoAnterior = consumosPorDia.Where(kvp => kvp.Key <= dia).OrderByDescending(kvp => kvp.Key).First();
                    var diferenca = proximoPonto.Value.consumoPorAve - pontoAnterior.Value.consumoPorAve;
                    var incrementoDiario = diferenca / (proximoPonto.Key - pontoAnterior.Key);
                    consumoBase = pontoAnterior.Value.consumoPorAve + (incrementoDiario * (dia - pontoAnterior.Key));
                }

                // Adicionar variação de ±3%
                var variacao = (decimal)(random.NextDouble() * 0.06 - 0.03);
                var consumoPorAve = consumoBase * (1 + variacao);
                
                // Calcular aves vivas (diminuir gradualmente devido à mortalidade)
                var mortalidadePorDia = 0.15m / 100m; // 0.15% ao dia (total ~6% em 42 dias)
                var avesVivas = (int)(lote.QuantidadeAvesInicial * Math.Pow((double)(1 - mortalidadePorDia), dia));

                var quantidadeTotal = (consumoPorAve * avesVivas) / 1000m; // Converter para kg

                var consumo = new ConsumoRacao
                {
                    LoteId = lote.Id,
                    Data = dataConsumo,
                    QuantidadeKg = Math.Round(quantidadeTotal, 2),
                    TipoRacao = fase,
                    AvesVivas = avesVivas,
                    Observacoes = dia % 7 == 0 ? "Pesagem semanal realizada" : null
                };

                context.ConsumosRacao.Add(consumo);
            }

            logger.LogInformation("Adicionados {Count} registros de consumo de ração para lote {LoteId}", idadeAtual, lote.Id);
        }

        private static void PopularConsumoAgua(GranjaTechDbContext context, Lote lote, DateTime dataInicio, int idadeAtual, Random random, ILogger logger)
        {
            // Consumo de água baseado na idade e temperatura
            for (int dia = 1; dia <= Math.Min(idadeAtual, 42); dia++)
            {
                var dataConsumo = dataInicio.AddDays(dia);
                
                // Consumo base de água por idade (ml/ave/dia)
                var consumoBaseMl = dia switch
                {
                    <= 7 => 40m,   // 40ml na primeira semana
                    <= 14 => 120m,  // 120ml na segunda semana
                    <= 21 => 200m,  // 200ml na terceira semana
                    <= 28 => 280m,  // 280ml na quarta semana
                    <= 35 => 350m,  // 350ml na quinta semana
                    _ => 400m       // 400ml da sexta semana em diante
                };

                // Temperatura afeta o consumo
                var temperatura = 20 + (decimal)(random.NextDouble() * 15); // 20-35°C
                var fatorTemperatura = temperatura > 25 ? 1 + (temperatura - 25) * 0.03m : 1m;
                
                var consumoPorAve = consumoBaseMl * fatorTemperatura;
                
                // Variação de ±5%
                var variacao = (decimal)(random.NextDouble() * 0.1 - 0.05);
                consumoPorAve *= (1 + variacao);

                // Calcular aves vivas
                var mortalidadePorDia = 0.15m / 100m;
                var avesVivas = (int)(lote.QuantidadeAvesInicial * Math.Pow((double)(1 - mortalidadePorDia), dia));

                var quantidadeTotalLitros = (consumoPorAve * avesVivas) / 1000m;

                var consumo = new ConsumoAgua
                {
                    LoteId = lote.Id,
                    Data = dataConsumo,
                    QuantidadeLitros = Math.Round(quantidadeTotalLitros, 1),
                    AvesVivas = avesVivas,
                    TemperaturaAmbiente = Math.Round(temperatura, 1),
                    Observacoes = temperatura > 30 ? "Temperatura elevada - aumentar ventilação" : null
                };

                context.ConsumosAgua.Add(consumo);
            }

            logger.LogInformation("Adicionados {Count} registros de consumo de água para lote {LoteId}", idadeAtual, lote.Id);
        }

        private static void PopularRegistrosMortalidade(GranjaTechDbContext context, Lote lote, DateTime dataInicio, int idadeAtual, Random random, ILogger logger)
        {
            // Simular mortalidade realística
            var totalMortes = 0;
            var causas = new[] { "Ascite", "Síndrome da Morte Súbita", "Problemas Locomotores", "Causas Diversas", "Debilidade" };

            for (int dia = 1; dia <= Math.Min(idadeAtual, 42); dia += random.Next(1, 4)) // Nem todos os dias tem mortalidade
            {
                if (random.NextDouble() > 0.7) continue; // 70% de chance de ter mortalidade

                var dataRegistro = dataInicio.AddDays(dia);
                
                // Mortalidade mais alta nos primeiros dias
                var mortalidadeMaxima = dia <= 7 ? 15 : dia <= 21 ? 8 : 5;
                var quantidadeMortas = random.Next(1, mortalidadeMaxima + 1);
                
                if (totalMortes + quantidadeMortas > lote.QuantidadeAvesInicial * 0.08m) // Máximo 8% de mortalidade
                    continue;

                totalMortes += quantidadeMortas;
                var avesVivas = lote.QuantidadeAvesInicial - totalMortes;

                var registro = new RegistroMortalidade
                {
                    LoteId = lote.Id,
                    Data = dataRegistro,
                    QuantidadeMortas = quantidadeMortas,
                    AvesVivas = avesVivas,
                    IdadeDias = dia,
                    CausaPrincipal = causas[random.Next(causas.Length)],
                    Observacoes = quantidadeMortas > 5 ? "Investigar causa - mortalidade elevada" : "Mortalidade dentro do esperado",
                    ResponsavelRegistro = "Sistema Automático"
                };

                context.RegistrosMortalidade.Add(registro);
            }

            // Atualizar quantidade atual do lote
            lote.QuantidadeAvesAtual = lote.QuantidadeAvesInicial - totalMortes;

            logger.LogInformation("Adicionados registros de mortalidade para lote {LoteId}. Total de mortes: {TotalMortes}", lote.Id, totalMortes);
        }

        private static void PopularEventosSanitarios(GranjaTechDbContext context, Lote lote, DateTime dataInicio, int idadeAtual, Random random, ILogger logger)
        {
            // Programa de vacinação padrão
            var vacinacoes = new[]
            {
                (dia: 1, produto: "Marek + Gumboro", via: "Subcutânea"),
                (dia: 7, produto: "Newcastle + Bronquite", via: "Spray"),
                (dia: 14, produto: "Gumboro", via: "Água"),
                (dia: 21, produto: "Newcastle", via: "Água"),
                (dia: 28, produto: "Coccidiostático", via: "Ração")
            };

            foreach (var (dia, produto, via) in vacinacoes)
            {
                if (dia > idadeAtual) continue;

                var evento = new EventoSanitario
                {
                    LoteId = lote.Id,
                    Data = dataInicio.AddDays(dia),
                    TipoEvento = "Vacinacao",
                    Produto = produto,
                    LoteProduto = $"LT{random.Next(1000, 9999)}",
                    ViaAdministracao = via,
                    AvesTratadas = lote.QuantidadeAvesInicial,
                    ResponsavelAplicacao = "Técnico Responsável",
                    Custo = random.Next(200, 800),
                    Observacoes = "Vacinação conforme protocolo sanitário"
                };

                context.EventosSanitarios.Add(evento);
            }

            // Alguns tratamentos pontuais
            if (idadeAtual > 15 && random.NextDouble() > 0.5)
            {
                var tratamento = new EventoSanitario
                {
                    LoteId = lote.Id,
                    Data = dataInicio.AddDays(random.Next(15, Math.Min(idadeAtual, 30))),
                    TipoEvento = "Medicacao",
                    Produto = "Antibiótico Respiratório",
                    Dosagem = "1g/L água",
                    ViaAdministracao = "Água",
                    AvesTratadas = lote.QuantidadeAvesAtual,
                    DuracaoTratamentoDias = 5,
                    PeriodoCarenciaDias = 7,
                    ResponsavelAplicacao = "Veterinário",
                    Sintomas = "Sinais respiratórios leves",
                    Custo = random.Next(300, 1200),
                    Observacoes = "Tratamento preventivo devido a mudanças climáticas"
                };

                context.EventosSanitarios.Add(tratamento);
            }

            logger.LogInformation("Adicionados eventos sanitários para lote {LoteId}", lote.Id);
        }

        private static void PopularQualidadeAr(GranjaTechDbContext context, Lote lote, DateTime dataInicio, int idadeAtual, Random random, ILogger logger)
        {
            // Medições de qualidade do ar (algumas por semana)
            for (int dia = 1; dia <= Math.Min(idadeAtual, 42); dia += random.Next(2, 5))
            {
                var horarios = new[] { 8, 14, 20 }; // 3 medições por dia selecionado
                
                foreach (var hora in horarios)
                {
                    var dataHora = dataInicio.AddDays(dia).AddHours(hora);
                    
                    // Valores que variam ao longo do dia e com a idade das aves
                    var temperatura = hora switch
                    {
                        8 => 22 + (decimal)(random.NextDouble() * 4),   // Manhã: 22-26°C
                        14 => 26 + (decimal)(random.NextDouble() * 6),  // Tarde: 26-32°C
                        _ => 24 + (decimal)(random.NextDouble() * 4)    // Noite: 24-28°C
                    };

                    var umidade = 55 + (decimal)(random.NextDouble() * 20); // 55-75%
                    var nh3 = (decimal)(random.NextDouble() * 20); // 0-20 ppm (bom se < 25)
                    var co2 = 400 + (decimal)(random.NextDouble() * 2000); // 400-2400 ppm
                    var o2 = 20.5m + (decimal)(random.NextDouble() * 0.5); // 20.5-21%

                    var medicao = new QualidadeAr
                    {
                        LoteId = lote.Id,
                        DataHora = dataHora,
                        TemperaturaAr = Math.Round(temperatura, 1),
                        UmidadeRelativa = Math.Round(umidade, 1),
                        NH3_ppm = Math.Round(nh3, 1),
                        CO2_ppm = Math.Round(co2, 0),
                        O2_percentual = Math.Round(o2, 1),
                        VelocidadeAr_ms = Math.Round((decimal)(random.NextDouble() * 2), 1), // 0-2 m/s
                        Luminosidade_lux = hora == 14 ? random.Next(30, 60) : random.Next(5, 20), // Mais luz à tarde
                        LocalMedicao = random.NextDouble() > 0.5 ? "Centro do galpão" : "Próximo aos comedouros",
                        EquipamentoMedicao = "Sensor Automático IoT",
                        Observacoes = temperatura > 30 ? "Temperatura elevada - verificar ventilação" : 
                                     nh3 > 20 ? "Amônia em nível de atenção" : null
                    };

                    context.MedicoesQualidadeAr.Add(medicao);
                }
            }

            logger.LogInformation("Adicionadas medições de qualidade do ar para lote {LoteId}", lote.Id);
        }
    }
}
