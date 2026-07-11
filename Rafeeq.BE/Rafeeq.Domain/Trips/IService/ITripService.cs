using Rafeeq.Domain.Common;
using Rafeeq.Domain.Trips.DTOs;

namespace Rafeeq.Domain.Trips.IService;

public interface ITripService
{
    Task<ResultViewModel<bool>> Create(TripCreateDto dto);
    Task<ResultViewModel<List<TripResultDto>>> Search(QueryViewModel<TripSearchFilterDto, EnumTripSortDto> query);
    Task<ResultViewModel<List<TripResultDto>>> GetMine();   // the driver's own trips (all statuses)
    Task<ResultViewModel<TripResultDto>> GetOne(string uniqueId);
    Task<ResultViewModel<bool>> Complete(string uniqueId);  // driver marks trip done → bookings complete
    Task<ResultViewModel<bool>> Cancel(string uniqueId);
}