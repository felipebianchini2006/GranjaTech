
using GranjaTech.Application.DTOs;
using GranjaTech.Application.Services.Interfaces;
using GranjaTech.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


namespace GranjaTech.Api.Controllers
{
    
    [Authorize(Roles = "Administrador,Produtor")]
    [ApiController]
    [Route("api/[controller]")]
    public class SensoresController : ControllerBase
    {
        // Declara uma "caixa de ferramentas" privada para lidar com as regras de sensores.
        private readonly ISensorService _sensorService;

        // Este é o "construtor". Quando o controller é criado, ele recebe as ferramentas
        // de que precisa para trabalhar (o _sensorService). Isso é chamado de injeção de dependência.
        public SensoresController(ISensorService sensorService)
        {
            _sensorService = sensorService;
        }

        // [HttpGet] Responde a requisições do tipo GET para o endereço principal ("api/Sensores").
        // É usado para buscar a lista de todos os sensores.
        [HttpGet]
        public async Task<IActionResult> GetSensores()
        {
            // Pede ao serviço para buscar todos os sensores.
            var sensores = await _sensorService.GetAllAsync();
            // Se der tudo certo, retorna a lista de sensores com um status de sucesso (Ok).
            return Ok(sensores);
        }

        // [HttpPost] Responde a requisições do tipo POST para "api/Sensores".
        // É usado para criar um novo sensor.
        [HttpPost]
        public async Task<IActionResult> PostSensor(CreateSensorDto sensorDto)
        {
            try // Tenta executar o código abaixo.
            {
                // Envia os dados do novo sensor para o serviço, que vai cuidar de salvá-lo.
                await _sensorService.AddAsync(sensorDto);
                // Retorna uma mensagem de sucesso.
                return Ok(new { message = "Sensor adicionado com sucesso." });
            }
            catch (InvalidOperationException ex) // Se acontecer um erro específico (InvalidOperationException)...
            {
                // Retorna um erro de "requisição inválida" com a mensagem do erro.
                return BadRequest(new { message = ex.Message });
            }
        }

        // [HttpDelete] Responde a requisições DELETE para "api/Sensores/5" (por exemplo).
        // O "{id}" no endereço significa que ele espera receber um número.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSensor(int id)
        {
            // Pede ao serviço para deletar o sensor com o ID informado.
            var sucesso = await _sensorService.DeleteAsync(id);
            // Se o serviço responder que não encontrou o sensor para deletar...
            if (!sucesso) return NotFound(); // Retorna um erro 404 "Não Encontrado".
            // Se conseguiu deletar, retorna uma resposta de sucesso sem conteúdo (NoContent).
            return NoContent();
        }

        // [HttpGet] Responde a requisições GET para "api/Sensores/5/leituras" (por exemplo).
        // É usado para buscar todas as leituras de um sensor específico.
        [HttpGet("{sensorId}/leituras")]
        public async Task<IActionResult> GetLeituras(int sensorId)
        {
            // Pede ao serviço a lista de leituras do sensor com o ID informado.
            var leituras = await _sensorService.GetLeiturasBySensorIdAsync(sensorId);
            // Retorna a lista de leituras com um status de sucesso (Ok).
            return Ok(leituras);
        }
    }
}