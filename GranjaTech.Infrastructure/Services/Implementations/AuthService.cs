using GranjaTech.Application.DTOs;
using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System; // Adicione este using para o DateTime
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GranjaTech.Infrastructure.Services.Implementations
{
    // A classe começa aqui
    public class AuthService : IAuthService
    {
        private readonly GranjaTechDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(GranjaTechDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _context.Usuarios
                .Include(u => u.Perfil)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    PerfilNome = u.Perfil.Nome
                })
                .ToListAsync();
        }

        public async Task<bool> RegistrarAsync(RegisterDto registerDto)
        {
            var usuarioExistente = await _context.Usuarios.AnyAsync(u => u.Email == registerDto.Email);
            if (usuarioExistente)
            {
                return false;
            }

            var senhaHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Senha);

            var novoUsuario = new Usuario
            {
                Nome = registerDto.Nome,
                Email = registerDto.Email,
                SenhaHash = senhaHash,
                PerfilId = registerDto.PerfilId
            };

            await _context.Usuarios.AddAsync(novoUsuario);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var usuario = await _context.Usuarios
                                        .Include(u => u.Perfil)
                                        .SingleOrDefaultAsync(u => u.Email == loginDto.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginDto.Senha, usuario.SenhaHash))
            {
                return null;
            }

            var token = GenerateJwtToken(usuario);

            return new LoginResponseDto { Token = token };
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return false; // Usuário não encontrado
            }

            var emailExistente = await _context.Usuarios.AnyAsync(u => u.Email == updateUserDto.Email && u.Id != id);
            if (emailExistente)
            {
                return false; // Conflito de email
            }

            usuario.Nome = updateUserDto.Nome;
            usuario.Email = updateUserDto.Email;
            usuario.PerfilId = updateUserDto.PerfilId;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return false; // Usuário não encontrado
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return true;
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

            var claims = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Perfil.Nome)
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }
    } // <-- A CLASSE TERMINA AQUI. Seus novos métodos estavam provavelmente depois desta chave.
}