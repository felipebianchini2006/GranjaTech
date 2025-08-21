using GranjaTech.Application.DTOs; // Adicione este using
using GranjaTech.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GranjaTech.Application.Services.Interfaces
{
    public interface ISensorService
    {
        Task<IEnumerable<Sensor>> GetAllAsync();
        Task AddAsync(CreateSensorDto sensorDto); // Assinatura alterada
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<LeituraSensor>> GetLeiturasBySensorIdAsync(int sensorId);
    }
}
