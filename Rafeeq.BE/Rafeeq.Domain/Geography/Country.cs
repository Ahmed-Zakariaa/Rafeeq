using Rafeeq.Domain.Common;

namespace Rafeeq.Domain.Geography;

public class Country : BaseEntity<int>
{
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string IsoCode { get; private set; } = string.Empty;       // EG, SA
    public string CurrencyCode { get; private set; } = string.Empty;  // EGP, SAR
    public string PhonePrefix { get; private set; } = string.Empty;   // +20, +966

    public ICollection<City> Cities { get; private set; } = new List<City>();

    private Country() { }

    public Country(string nameAr, string nameEn, string isoCode, string currencyCode, string phonePrefix)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        IsoCode = isoCode;
        CurrencyCode = currencyCode;
        PhonePrefix = phonePrefix;
    }

    public void Update(string nameAr, string nameEn, string isoCode, string currencyCode, string phonePrefix)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        IsoCode = isoCode;
        CurrencyCode = currencyCode;
        PhonePrefix = phonePrefix;
    }
}
