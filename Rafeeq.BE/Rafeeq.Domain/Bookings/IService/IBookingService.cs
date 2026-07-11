using Rafeeq.Domain.Bookings.DTOs;
using Rafeeq.Domain.Common;

namespace Rafeeq.Domain.Bookings.IService;

public interface IBookingService
{
    Task<ResultViewModel<bool>> Request(BookingRequestDto dto);   // passenger requests a seat
    Task<ResultViewModel<bool>> Respond(BookingRespondDto dto);   // driver accepts/rejects
    Task<ResultViewModel<bool>> Cancel(string uniqueId);          // passenger cancels own booking
    Task<ResultViewModel<List<BookingResultDto>>> GetMine();      // passenger's bookings
    Task<ResultViewModel<List<BookingResultDto>>> GetIncoming();  // requests on the driver's trips
}
