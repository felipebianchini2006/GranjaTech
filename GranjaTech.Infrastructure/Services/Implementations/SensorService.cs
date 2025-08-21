using GranjaTech.Application.DTOs;
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
    public class SensorService : ISensorService
    {
        private readonly GranjaTechDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditoriaService _auditoriaService;

        public SensorService(GranjaTechDbContext context, IHttpContextAccessor httpContextAccessor, IAuditoriaService auditoriaService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _auditoriaService = auditoriaService;
        }

        // MÉTODO ATUALIZADO PARA SER MAIS SEGURO
        private (int userId, string userRole) GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                throw new InvalidOperationException("Contexto de utilizador não encontrado.");
            }

            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoleClaim = user.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(userRoleClaim))
            {
                throw new InvalidOperationException("Não foi possível identificar o utilizador logado (claims não encontradas no token).");
            }

            return (int.Parse(userIdClaim), userRoleClaim);
        }

        public async Task<IEnumerable<Sensor>> GetAllAsync()
        {
            var (userId, userRole) = GetCurrentUser();
            IQueryable<Sensor> query = _context.Sensores.Include(s => s.Granja).ThenInclude(g => g.Usuario);

            if (userRole == "Administrador") { }
            else if (userRole == "Produtor")
            {
                query = query.Where(s => s.Granja.UsuarioId == userId);
            }
            else
            {
                return new List<Sensor>();
            }
            return await query.ToListAsync();
        }

        public async Task AddAsync(CreateSensorDto sensorDto)
        {
            var (userId, userRole) = GetCurrentUser();
            var granja = await _context.Granjas.FindAsync(sensorDto.GranjaId);
            if (granja == null || (userRole == "Produtor" && granja.UsuarioId != userId))
            {
                throw new InvalidOperationException("Permissão negada ou granja inválida.");
            }

            if (await _context.Sensores.AnyAsync(s => s.IdentificadorUnico == sensorDto.IdentificadorUnico))
            {
                throw new InvalidOperationException("Já existe um sensor com este Identificador Único.");
            }

            var sensor = new Sensor
            {
                Tipo = sensorDto.Tipo,
                IdentificadorUnico = sensorDto.IdentificadorUnico,
                GranjaId = sensorDto.GranjaId
            };

            await _context.Sensores.AddAsync(sensor);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("CRIACAO_SENSOR", $"Sensor '{sensor.IdentificadorUnico}' (ID: {sensor.Id}) adicionado à Granja ID: {sensor.GranjaId}.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var (userId, userRole) = GetCurrentUser();
            var sensor = await _context.Sensores.Include(s => s.Granja).FirstOrDefaultAsync(s => s.Id == id);
            if (sensor == null) return false;

            if (userRole == "Produtor" && sensor.Granja.UsuarioId != userId)
            {
                throw new InvalidOperationException("Permissão negada.");
            }

            _context.Sensores.Remove(sensor);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("DELECAO_SENSOR", $"Sensor '{sensor.IdentificadorUnico}' (ID: {id}) deletado.");
            return true;
        }

        public async Task<IEnumerable<LeituraSensor>> GetLeiturasBySensorIdAsync(int sensorId)
        {
            var (userId, userRole) = GetCurrentUser();
            var sensor = await _context.Sensores.Include(s => s.Granja).FirstOrDefaultAsync(s => s.Id == sensorId);
            if (sensor == null) return new List<LeituraSensor>();

            bool temPermissao = false;
            if (userRole == "Administrador" || (userRole == "Produtor" && sensor.Granja.UsuarioId == userId))
            {
                temPermissao = true;
            }
            else if (userRole == "Financeiro")
            {
                var produtorIds = await _context.FinanceiroProdutor
                    .Where(fp => fp.FinanceiroId == userId)
                    .Select(fp => fp.ProdutorId)
                    .ToListAsync();

                if (produtorIds.Contains(sensor.Granja.UsuarioId))
                {
                    temPermissao = true;
                }
            }

            if (!temPermissao)
            {
                return new List<LeituraSensor>();
            }

            return await _context.LeiturasSensores
                .Where(l => l.SensorId == sensorId)
                .OrderByDescending(l => l.Timestamp)
                .Take(100)
                .ToListAsync();
        }
    }
}
