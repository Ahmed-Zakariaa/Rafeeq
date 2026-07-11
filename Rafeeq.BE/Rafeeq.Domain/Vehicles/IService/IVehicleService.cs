using Rafeeq.Domain.Common;
using Rafeeq.Domain.Vehicles.DTOs;

namespace Rafeeq.Domain.Vehicles.IService;

public interface IVehicleService
{
    Task<ResultViewModel<bool>> Create(VehicleCreateDto dto);
    Task<ResultViewModel<List<VehicleResultDto>>> GetMine();
}
