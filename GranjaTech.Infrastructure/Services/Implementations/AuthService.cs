using GranjaTech.Application.DTOs;
using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GranjaTech.Infrastructure.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly GranjaTechDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAuditoriaService _auditoriaService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(GranjaTechDbContext context, IConfiguration configuration, IAuditoriaService auditoriaService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _auditoriaService = auditoriaService;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new InvalidOperationException("Não foi possível identificar o utilizador logado (ID não encontrado no token).");
            }
            return int.Parse(userIdClaim);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _context.Usuarios
                .Include(u => u.Perfil)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Codigo = u.Codigo,
                    Nome = u.Nome,
                    Email = u.Email,
                    PerfilNome = u.Perfil.Nome
                })
                .ToListAsync();
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.ProdutoresGerenciados)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null) return null;

            return new UserDetailDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                PerfilId = usuario.PerfilId,
                ProdutorIds = usuario.ProdutoresGerenciados.Select(fp => fp.ProdutorId).ToList()
            };
        }

        public async Task<bool> RegistrarAsync(RegisterDto registerDto)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == registerDto.Email)) return false;

            var senhaHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Senha);

            var ultimoId = await _context.Usuarios.OrderByDescending(u => u.Id).Select(u => u.Id).FirstOrDefaultAsync();
            var novoCodigo = $"USR-{(ultimoId + 1):D3}";

            var novoUsuario = new Usuario
            {
                Codigo = novoCodigo,
                Nome = registerDto.Nome,
                Email = registerDto.Email,
                SenhaHash = senhaHash,
                PerfilId = registerDto.PerfilId
            };

            await _context.Usuarios.AddAsync(novoUsuario);
            await _context.SaveChangesAsync();

            await _auditoriaService.RegistrarLog("CRIACAO_USUARIO", $"Utilizador '{novoUsuario.Email}' (Código: {novoUsuario.Codigo}) foi criado.");

            if (registerDto.PerfilId == 3 && registerDto.ProdutorIds != null && registerDto.ProdutorIds.Any())
            {
                foreach (var produtorId in registerDto.ProdutorIds)
                {
                    _context.FinanceiroProdutor.Add(new FinanceiroProdutor { FinanceiroId = novoUsuario.Id, ProdutorId = produtorId });
                }
                await _context.SaveChangesAsync();
                await _auditoriaService.RegistrarLog("ASSOCIACAO_USUARIO", $"Utilizador Financeiro '{novoUsuario.Email}' associado aos Produtores IDs: {string.Join(", ", registerDto.ProdutorIds)}.");
            }
            return true;
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var usuario = await _context.Usuarios.Include(u => u.ProdutoresGerenciados).FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return false;
            if (await _context.Usuarios.AnyAsync(u => u.Email == updateUserDto.Email && u.Id != id)) return false;

            usuario.Nome = updateUserDto.Nome;
            usuario.Email = updateUserDto.Email;
            usuario.PerfilId = updateUserDto.PerfilId;
            usuario.ProdutoresGerenciados.Clear();

            if (usuario.PerfilId == 3 && updateUserDto.ProdutorIds != null && updateUserDto.ProdutorIds.Any())
            {
                foreach (var produtorId in updateUserDto.ProdutorIds) { usuario.ProdutoresGerenciados.Add(new FinanceiroProdutor { ProdutorId = produtorId }); }
            }

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("ATUALIZACAO_USUARIO", $"Utilizador '{usuario.Email}' (ID: {usuario.Id}) foi atualizado.");
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var usuario = await _context.Usuarios.Include(u => u.Perfil).FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return false;

            if (usuario.Perfil.Nome == "Produtor")
            {
                if (await _context.Granjas.AnyAsync(g => g.UsuarioId == id)) throw new InvalidOperationException("Este produtor possui granjas e/ou lotes associados e não pode ser excluído.");
                if (await _context.FinanceiroProdutor.AnyAsync(fp => fp.ProdutorId == id)) throw new InvalidOperationException("Este produtor está associado a um ou mais utilizadores financeiros e não pode ser excluído.");
            }

            var nomeUsuarioDeletado = usuario.Nome;
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("DELECAO_USUARIO", $"Utilizador '{nomeUsuarioDeletado}' (ID: {id}) foi deletado.");
            return true;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var usuario = await _context.Usuarios.Include(u => u.Perfil).SingleOrDefaultAsync(u => u.Email == loginDto.Email);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginDto.Senha, usuario.SenhaHash)) return null;
            var token = GenerateJwtToken(usuario);
            return new LoginResponseDto { Token = token };
        }

        public async Task<ProfileDetailDto?> GetProfileAsync()
        {
            var userId = GetCurrentUserId();
            var usuario = await _context.Usuarios
                .Include(u => u.Perfil)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null) return null;

            var associados = new List<string>();
            if (usuario.Perfil.Nome == "Produtor")
            {
                associados = await _context.FinanceiroProdutor
                    .Where(fp => fp.ProdutorId == userId)
                    .Include(fp => fp.Financeiro)
                    .Select(fp => fp.Financeiro.Nome)
                    .ToListAsync();
            }
            else if (usuario.Perfil.Nome == "Financeiro")
            {
                associados = await _context.FinanceiroProdutor
                    .Where(fp => fp.FinanceiroId == userId)
                    .Include(fp => fp.Produtor)
                    .Select(fp => fp.Produtor.Nome)
                    .ToListAsync();
            }

            return new ProfileDetailDto
            {
                Nome = usuario.Nome,
                Email = usuario.Email,
                PerfilNome = usuario.Perfil.Nome,
                Associados = associados
            };
        }

        public async Task<bool> UpdateProfileAsync(UpdateProfileDto profileDto)
        {
            var userId = GetCurrentUserId();
            var usuario = await _context.Usuarios.FindAsync(userId);

            if (usuario == null) return false;

            if (await _context.Usuarios.AnyAsync(u => u.Email == profileDto.Email && u.Id != userId))
            {
                throw new InvalidOperationException("O email informado já está em uso por outra conta.");
            }

            usuario.Nome = profileDto.Nome;
            usuario.Email = profileDto.Email;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("ATUALIZACAO_PERFIL", $"Utilizador (ID: {userId}) atualizou o próprio perfil.");
            return true;
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto passwordDto)
        {
            var userId = GetCurrentUserId();
            var usuario = await _context.Usuarios.FindAsync(userId);

            if (usuario == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(passwordDto.SenhaAtual, usuario.SenhaHash))
            {
                return false; // Senha atual incorreta
            }

            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(passwordDto.NovaSenha);
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarLog("TROCA_SENHA", $"Utilizador (ID: {userId}) alterou a própria senha.");
            return true;
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
            var claims = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()), new Claim(ClaimTypes.Email, usuario.Email), new Claim(ClaimTypes.Role, usuario.Perfil.Nome) });
            var tokenDescriptor = new SecurityTokenDescriptor { Subject = claims, Expires = DateTime.UtcNow.AddHours(8), Issuer = _configuration["Jwt:Issuer"], Audience = _configuration["Jwt:Audience"], SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) };
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }
    }
}
