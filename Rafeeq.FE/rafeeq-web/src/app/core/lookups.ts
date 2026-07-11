import { AppLang } from './i18n';

// Loaded from the BE Lookups API (PascalCase, inside ApiResponse.Data).
export interface CountryLookup {
  Id: number;
  NameAr: string;
  NameEn: string;
  IsoCode: string;
  CurrencyCode: string;
  PhonePrefix: string;
  IsActive: boolean;
}

export interface CityLookup {
  Id: number;
  CountryId: number;
  NameAr: string;
  NameEn: string;
  IsActive: boolean;
}

export function cityName(c: { NameAr: string; NameEn: string }, lang: AppLang): string {
  return lang === 'en' ? c.NameEn : c.NameAr;
}

export function countryName(c: { NameAr: string; NameEn: string }, lang: AppLang): string {
  return lang === 'en' ? c.NameEn : c.NameAr;
}
