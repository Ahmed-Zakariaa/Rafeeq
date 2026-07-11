namespace Rafeeq.Domain.Geography.DTOs;

public class CountryDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string PhonePrefix { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CityDto
{
    public int Id { get; set; }
    public int CountryId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CountryUpsertDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string PhonePrefix { get; set; } = string.Empty;
}

public class CityUpsertDto
{
    public int CountryId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

public class SetActiveDto
{
    public bool IsActive { get; set; }
}
