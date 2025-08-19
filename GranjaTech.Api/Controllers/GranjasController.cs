using GranjaTech.Application.DTOs;
using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GranjasController : ControllerBase
    {
        private readonly IGranjaService _granjaService;

        public GranjasController(IGranjaService granjaService)
        {
            _granjaService = granjaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Granja>>> GetGranjas()
        {
            var granjas = await _granjaService.GetAllAsync();
            return Ok(granjas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Granja>> GetGranja(int id)
        {
            var granja = await _granjaService.GetByIdAsync(id);
            if (granja == null) return NotFound();
            return Ok(granja);
        }

        [HttpPost]
        public async Task<IActionResult> PostGranja(CreateGranjaDto granjaDto)
        {
            try
            {
                await _granjaService.AddAsync(granjaDto);
                return Ok(new { message = "Granja criada com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutGranja(int id, UpdateGranjaDto granjaDto)
        {
            try
            {
                var sucesso = await _granjaService.UpdateAsync(id, granjaDto);
                if (!sucesso)
                {
                    return NotFound("Granja não encontrada ou permissão negada.");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGranja(int id)
        {
            try
            {
                await _granjaService.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
