using GranjaTech.Application.DTOs; // Adicione este using
using GranjaTech.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Application.Services.Interfaces
{
    
    public interface ISensorService
    {
        // Uma tarefa para buscar todos os sensores cadastrados
        Task<IEnumerable<Sensor>> GetAllAsync();

        // Uma tarefa para adicionar um novo sensor no sistema
        Task AddAsync(CreateSensorDto sensorDto);

        // Uma tarefa para apagar um sensor usando o seu número de ID.
        // Ela deve responder se deu certo (true) ou não (false).
        Task<bool> DeleteAsync(int id);

        // Uma tarefa para buscar todas as leituras de um sensor específico.
        Task<IEnumerable<LeituraSensor>> GetLeiturasBySensorIdAsync(int sensorId);
    }
}