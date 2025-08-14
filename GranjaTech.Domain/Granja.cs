using System;
using System.Collections.Generic;

namespace GranjaTech.Domain
{
    public class Granja
    {
        // Chave primária da tabela. O EF Core entende que 'Id' é a chave por convenção.
        public int Id { get; set; }

        // Nome da granja.
        public string Nome { get; set; } = string.Empty;

        // Localização/endereço da granja.
        public string? Localizacao { get; set; }
    }
}