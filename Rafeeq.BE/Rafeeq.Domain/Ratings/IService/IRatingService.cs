using Rafeeq.Domain.Common;
using Rafeeq.Domain.Ratings.DTOs;

namespace Rafeeq.Domain.Ratings.IService;

public interface IRatingService
{
    Task<ResultViewModel<bool>> Rate(RateDto dto);   // rate the counterparty of a completed booking
}
