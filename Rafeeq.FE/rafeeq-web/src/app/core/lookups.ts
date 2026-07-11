import { AppLang } from './i18n';

// v1 has no Cities lookup endpoint yet, so the seeded launch cities are listed
// here (matches Rafeeq.Infrastructure/Persistence/DbSeeder.cs insertion order).
// When a Lookups/Cities endpoint lands, replace this with an API call.
export interface CityLookup {
  id: number;
  countryId: number; // 1 = Egypt, 2 = KSA
  nameAr: string;
  nameEn: string;
}

export const CITIES: CityLookup[] = [
  { id: 1, countryId: 1, nameAr: 'القاهرة', nameEn: 'Cairo' },
  { id: 2, countryId: 1, nameAr: 'الإسكندرية', nameEn: 'Alexandria' },
  { id: 3, countryId: 2, nameAr: 'الرياض', nameEn: 'Riyadh' },
  { id: 4, countryId: 2, nameAr: 'جدة', nameEn: 'Jeddah' },
  { id: 5, countryId: 2, nameAr: 'الدمام', nameEn: 'Dammam' },
];

export function cityName(city: CityLookup, lang: AppLang): string {
  return lang === 'en' ? city.nameEn : city.nameAr;
}
