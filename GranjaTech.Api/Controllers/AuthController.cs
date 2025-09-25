using GranjaTech.Application.DTOs;
using GranjaTech.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        [HttpGet("usuarios/{id}")]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var usuario = await _authService.GetUserByIdAsync(id);
            if (usuario == null) return NotFound();
            return Ok(usuario);
        }

        // Cadastro público (sem Authorize)
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar(RegisterDto registerDto)
        {
            try
            {
                var sucesso = await _authService.RegistrarAsync(registerDto);
                if (!sucesso) return BadRequest(new { message = "Email já cadastrado." });
                return Ok(new { message = "Usuário criado com sucesso!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("usuarios/{id}")]
        public async Task<IActionResult> UpdateUsuario(int id, UpdateUserDto updateUserDto)
        {
            var sucesso = await _authService.UpdateUserAsync(id, updateUserDto);
            if (!sucesso) return NotFound("Usuário não encontrado ou email já em uso.");
            return NoContent();
        }

        [Authorize(Roles = "Administrador")]
        [HttpDelete("usuarios/{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            try
            {
                var sucesso = await _authService.DeleteUserAsync(id);
                if (!sucesso) return NotFound("Usuário não encontrado.");
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            if (response == null) return Unauthorized(new { message = "Email ou senha inválidos." });
            return Ok(response);
        }
    }
}