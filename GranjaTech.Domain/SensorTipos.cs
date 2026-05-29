using System;
using System.Collections.Generic;
using System.Linq;

namespace GranjaTech.Domain
{
    public static class SensorTipos
    {
        public const string Temperatura = "Temperatura";
        public const string Umidade = "Umidade";
        public const string Luminosidade = "Luminosidade";

        public static readonly IReadOnlyList<string> Permitidos = new[]
        {
            Temperatura,
            Umidade,
            Luminosidade
        };

        public const string TiposPermitidosTexto = "Temperatura, Umidade e Luminosidade";

        public static bool TryNormalizar(string? tipo, out string tipoNormalizado)
        {
            tipoNormalizado = string.Empty;

            if (string.IsNullOrWhiteSpace(tipo))
            {
                return false;
            }

            var tipoEncontrado = Permitidos.FirstOrDefault(tipoPermitido =>
                string.Equals(tipoPermitido, tipo.Trim(), StringComparison.OrdinalIgnoreCase));

            if (tipoEncontrado == null)
            {
                return false;
            }

            tipoNormalizado = tipoEncontrado;
            return true;
        }
    }
}
