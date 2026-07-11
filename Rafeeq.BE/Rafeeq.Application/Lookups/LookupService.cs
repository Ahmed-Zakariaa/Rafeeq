using Microsoft.EntityFrameworkCore;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Geography;
using Rafeeq.Domain.Geography.DTOs;
using Rafeeq.Domain.Geography.IService;
using Rafeeq.Infrastructure.Persistence;

namespace Rafeeq.Application.Lookups;

public class LookupService : ILookupService
{
    private readonly RafeeqDbContext _db;
    public LookupService(RafeeqDbContext db) => _db = db;

    public async Task<ResultViewModel<List<CountryDto>>> GetCountries(bool includeInactive = false)
    {
        var list = await _db.Countries.AsNoTracking()
            .WhereIf(!includeInactive, c => c.IsActive)
            .OrderBy(c => c.NameEn)
            .Select(c => new CountryDto
            {
                Id = c.Id,
                NameAr = c.NameAr,
                NameEn = c.NameEn,
                IsoCode = c.IsoCode,
                CurrencyCode = c.CurrencyCode,
                PhonePrefix = c.PhonePrefix,
                IsActive = c.IsActive,
            })
            .ToListAsync();
        return ResultViewModel<List<CountryDto>>.Success(list);
    }

    public async Task<ResultViewModel<List<CityDto>>> GetCities(int? countryId, bool includeInactive = false)
    {
        var list = await _db.Cities.AsNoTracking()
            .WhereIf(!includeInactive, c => c.IsActive)
            .WhereIf(countryId != null, c => c.CountryId == countryId)
            .OrderBy(c => c.NameEn)
            .Select(c => new CityDto
            {
                Id = c.Id,
                CountryId = c.CountryId,
                NameAr = c.NameAr,
                NameEn = c.NameEn,
                IsActive = c.IsActive,
            })
            .ToListAsync();
        return ResultViewModel<List<CityDto>>.Success(list);
    }

    public async Task<ResultViewModel<bool>> CreateCountry(CountryUpsertDto dto)
    {
        var iso = dto.IsoCode.Trim().ToUpperInvariant();
        if (await _db.Countries.AnyAsync(c => c.IsoCode == iso))
            throw new BusinessException("isoCodeAlreadyExists", nameof(dto.IsoCode));

        await _db.Countries.AddAsync(new Country(dto.NameAr, dto.NameEn, iso, dto.CurrencyCode, dto.PhonePrefix));
        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<bool>> UpdateCountry(int id, CountryUpsertDto dto)
    {
        var country = await _db.Countries.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new BusinessException("countryNotFound");
        country.Update(dto.NameAr, dto.NameEn, dto.IsoCode.Trim().ToUpperInvariant(), dto.CurrencyCode, dto.PhonePrefix);
        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<bool>> SetCountryActive(int id, bool isActive)
    {
        var country = await _db.Countries.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new BusinessException("countryNotFound");
        if (isActive) country.Activate(); else country.Deactivate();
        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<bool>> CreateCity(CityUpsertDto dto)
    {
        if (!await _db.Countries.AnyAsync(c => c.Id == dto.CountryId))
            throw new BusinessException("countryNotFound", nameof(dto.CountryId));

        await _db.Cities.AddAsync(new City(dto.CountryId, dto.NameAr, dto.NameEn));
        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<bool>> UpdateCity(int id, CityUpsertDto dto)
    {
        var city = await _db.Cities.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new BusinessException("cityNotFound");
        city.Update(dto.NameAr, dto.NameEn);
        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<bool>> SetCityActive(int id, bool isActive)
    {
        var city = await _db.Cities.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new BusinessException("cityNotFound");
        if (isActive) city.Activate(); else city.Deactivate();
        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }
}
