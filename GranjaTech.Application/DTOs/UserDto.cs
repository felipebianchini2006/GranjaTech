﻿namespace GranjaTech.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PerfilNome { get; set; } = string.Empty;
    }
}