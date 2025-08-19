using GranjaTech.Application.DTOs;
using GranjaTech.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IAuthService _authService;

        public ProfileController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await _authService.GetProfileAsync();
            if (profile == null)
            {
                return NotFound();
            }
            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto profileDto)
        {
            try
            {
                var sucesso = await _authService.UpdateProfileAsync(profileDto);
                if (!sucesso)
                {
                    return NotFound("Usuário não encontrado.");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto passwordDto)
        {
            var sucesso = await _authService.ChangePasswordAsync(passwordDto);
            if (!sucesso)
            {
                return BadRequest(new { message = "Senha atual incorreta." });
            }
            return Ok(new { message = "Senha alterada com sucesso." });
        }
    }
}
