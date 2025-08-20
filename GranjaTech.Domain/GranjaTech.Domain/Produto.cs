namespace GranjaTech.Domain
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // Ex: "Ração", "Vacina", "Medicamento"
        public decimal Quantidade { get; set; }
        public string UnidadeDeMedida { get; set; } = string.Empty; // Ex: "kg", "un", "litro"

        // Cada produto pertence a uma granja específica
        public int GranjaId { get; set; }
        public Granja Granja { get; set; } = null!;
    }
}
