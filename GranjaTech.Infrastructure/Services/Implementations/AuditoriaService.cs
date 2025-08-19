using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GranjaTech.Infrastructure.Services.Implementations
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly GranjaTechDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditoriaService(GranjaTechDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<LogAuditoria>> GetAllAsync()
        {
            return await _context.LogsAuditoria.OrderByDescending(l => l.Timestamp).ToListAsync();
        }

        public async Task RegistrarLog(string acao, string detalhes)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                var log = new LogAuditoria
                {
                    Timestamp = DateTime.UtcNow,
                    UsuarioId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)),
                    UsuarioEmail = user.FindFirstValue(ClaimTypes.Email),
                    Acao = acao,
                    Detalhes = detalhes
                };
                await _context.LogsAuditoria.AddAsync(log);
                await _context.SaveChangesAsync();
            }
        }
    }
}