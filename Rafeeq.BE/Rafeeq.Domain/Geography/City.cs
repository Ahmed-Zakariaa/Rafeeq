using Rafeeq.Domain.Common;

namespace Rafeeq.Domain.Geography;

public class City : BaseEntity<int>
{
    public int CountryId { get; private set; }
    public Country? Country { get; private set; }
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;

    private City() { }

    public City(int countryId, string nameAr, string nameEn)
    {
        CountryId = countryId;
        NameAr = nameAr;
        NameEn = nameEn;
    }

    public void Update(string nameAr, string nameEn)
    {
        NameAr = nameAr;
        NameEn = nameEn;
    }
}
