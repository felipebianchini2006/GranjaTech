using GranjaTech.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [Authorize(Roles = "Administrador")]
    [ApiController]
    [Route("api/[controller]")]
    public class AuditoriaController : ControllerBase
    {
        private readonly IAuditoriaService _auditoriaService;

        public AuditoriaController(IAuditoriaService auditoriaService)
        {
            _auditoriaService = auditoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await _auditoriaService.GetAllAsync();
            return Ok(logs);
        }
    }
}