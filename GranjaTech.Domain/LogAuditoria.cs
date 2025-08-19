using System;

namespace GranjaTech.Domain
{
    public class LogAuditoria
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioEmail { get; set; } = string.Empty;
        public string Acao { get; set; } = string.Empty; // Ex: "CRIOU_LOTE", "DELETOU_GRANJA"
        public string Detalhes { get; set; } = string.Empty; // Ex: "ID do Lote: 42, Nome: Lote-D4"
    }
}