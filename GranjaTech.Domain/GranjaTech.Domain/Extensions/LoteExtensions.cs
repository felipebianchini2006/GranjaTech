using System.Linq;

namespace GranjaTech.Domain.Extensions
{
    public static class LoteExtensions
    {
        public static decimal CalcularConversaoAlimentar(this Lote lote)
        {
            if (lote.ConsumosRacao == null || !lote.ConsumosRacao.Any() ||
                lote.PesagensSemanais == null || !lote.PesagensSemanais.Any())
                return 0;

            var consumoTotal = lote.ConsumosRacao.Sum(c => c.QuantidadeKg);
            var pesagemInicial = lote.PesagensSemanais.OrderBy(p => p.SemanaVida).First();
            var pesagemFinal = lote.PesagensSemanais.OrderByDescending(p => p.SemanaVida).First();

            var ganhoTotal = pesagemFinal.PesoMedioGramas - pesagemInicial.PesoMedioGramas;
            var ganhoPorAveKg = (ganhoTotal * lote.QuantidadeAvesAtual) / 1000; // Converter para kg

            if (ganhoPorAveKg <= 0) return 0;

            return consumoTotal / ganhoPorAveKg;
        }

        public static decimal CalcularIEP(this Lote lote)
        {
            if (lote.IdadeAtualDias <= 0) return 0;

            var viabilidade = lote.Viabilidade / 100; // Converter para decimal
            var pesoMedio = lote.PesagensSemanais?.OrderByDescending(p => p.SemanaVida)
                                .FirstOrDefault()?.PesoMedioGramas ?? 0;
            var conversaoAlimentar = lote.CalcularConversaoAlimentar();

            if (conversaoAlimentar <= 0) return 0;

            // Fórmula IEP: (Viabilidade * Peso Médio * 100) / (Idade * CA)
            return (viabilidade * (decimal)(pesoMedio / 1000) * 100) / (lote.IdadeAtualDias * conversaoAlimentar);
        }
    }
}