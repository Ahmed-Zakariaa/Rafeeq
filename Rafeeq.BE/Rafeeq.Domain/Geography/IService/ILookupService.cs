using Rafeeq.Domain.Common;
using Rafeeq.Domain.Geography.DTOs;

namespace Rafeeq.Domain.Geography.IService;

public interface ILookupService
{
    // Public reads (active only)
    Task<ResultViewModel<List<CountryDto>>> GetCountries(bool includeInactive = false);
    Task<ResultViewModel<List<CityDto>>> GetCities(int? countryId, bool includeInactive = false);

    // Admin management (Lookups.Manage)
    Task<ResultViewModel<bool>> CreateCountry(CountryUpsertDto dto);
    Task<ResultViewModel<bool>> UpdateCountry(int id, CountryUpsertDto dto);
    Task<ResultViewModel<bool>> SetCountryActive(int id, bool isActive);

    Task<ResultViewModel<bool>> CreateCity(CityUpsertDto dto);
    Task<ResultViewModel<bool>> UpdateCity(int id, CityUpsertDto dto);
    Task<ResultViewModel<bool>> SetCityActive(int id, bool isActive);
}
