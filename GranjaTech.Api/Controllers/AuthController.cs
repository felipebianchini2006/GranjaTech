using GranjaTech.Application.DTOs;
using GranjaTech.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _authService.GetAllUsersAsync();
            return Ok(usuarios);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar(RegisterDto registerDto)
        {
            var sucesso = await _authService.RegistrarAsync(registerDto);
            if (!sucesso)
            {
                return BadRequest("Email já cadastrado.");
            }
            return Ok(new { message = "Usuário criado com sucesso!" });
        }

        // NOVO ENDPOINT DE ATUALIZAÇÃO
        [Authorize(Roles = "Administrador")]
        [HttpPut("usuarios/{id}")]
        public async Task<IActionResult> UpdateUsuario(int id, UpdateUserDto updateUserDto)
        {
            var sucesso = await _authService.UpdateUserAsync(id, updateUserDto);
            if (!sucesso)
            {
                return NotFound("Usuário não encontrado ou email já em uso.");
            }
            return NoContent();
        }

        // NOVO ENDPOINT DE EXCLUSÃO
        [Authorize(Roles = "Administrador")]
        [HttpDelete("usuarios/{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var sucesso = await _authService.DeleteUserAsync(id);
            if (!sucesso)
            {
                return NotFound("Usuário não encontrado.");
            }
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            if (response == null)
            {
                return Unauthorized(new { message = "Email ou senha inválidos." });
            }
            return Ok(response);
        }
    }
}