using GranjaTech.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [Authorize] // Protege o controller inteiro, qualquer usuário logado pode ver
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("kpis")]
        public async Task<IActionResult> GetKpis()
        {
            var kpis = await _dashboardService.GetKpisAsync();
            return Ok(kpis);
        }

        [HttpGet("resumo-mensal")]
        public async Task<IActionResult> GetResumoMensal()
        {
            var resumo = await _dashboardService.GetMonthlySummaryAsync();
            return Ok(resumo);
        }
    }
}