import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map, shareReplay, tap } from 'rxjs/operators';

import { ApiService } from '../core/api.service';
import { ApiResponse } from '../core/api-response.model';
import { CityLookup, CountryLookup } from '../core/lookups';

/**
 * Countries & cities from the BE. Public reads are cached (shareReplay) since the catalog
 * rarely changes; admin mutations clear the cache so dropdowns refresh.
 */
@Injectable({ providedIn: 'root' })
export class LookupsService {
  private _countries?: Observable<CountryLookup[]>;
  private _cities?: Observable<CityLookup[]>;

  constructor(private api: ApiService) {}

  countries(): Observable<CountryLookup[]> {
    if (!this._countries) {
      this._countries = this.api
        .get<CountryLookup[]>('Lookups/Countries')
        .pipe(map((r) => r.Data ?? []), shareReplay(1));
    }
    return this._countries;
  }

  cities(): Observable<CityLookup[]> {
    if (!this._cities) {
      this._cities = this.api
        .get<CityLookup[]>('Lookups/Cities')
        .pipe(map((r) => r.Data ?? []), shareReplay(1));
    }
    return this._cities;
  }

  // ── Admin management ──
  adminCountries(): Observable<ApiResponse<CountryLookup[]>> {
    return this.api.get<CountryLookup[]>('Lookups/Admin/Countries');
  }
  adminCities(countryId?: number): Observable<ApiResponse<CityLookup[]>> {
    return this.api.get<CityLookup[]>('Lookups/Admin/Cities', countryId ? { countryId } : undefined);
  }
  createCountry(dto: unknown): Observable<ApiResponse<boolean>> {
    return this.mutating(this.api.post<boolean>('Lookups/Countries', dto));
  }
  updateCountry(id: number, dto: unknown): Observable<ApiResponse<boolean>> {
    return this.mutating(this.api.put<boolean>(`Lookups/Countries/${id}`, dto));
  }
  setCountryActive(id: number, isActive: boolean): Observable<ApiResponse<boolean>> {
    return this.mutating(this.api.post<boolean>(`Lookups/Countries/${id}/Active`, { isActive }));
  }
  createCity(dto: unknown): Observable<ApiResponse<boolean>> {
    return this.mutating(this.api.post<boolean>('Lookups/Cities', dto));
  }
  updateCity(id: number, dto: unknown): Observable<ApiResponse<boolean>> {
    return this.mutating(this.api.put<boolean>(`Lookups/Cities/${id}`, dto));
  }
  setCityActive(id: number, isActive: boolean): Observable<ApiResponse<boolean>> {
    return this.mutating(this.api.post<boolean>(`Lookups/Cities/${id}/Active`, { isActive }));
  }

  private mutating(o: Observable<ApiResponse<boolean>>): Observable<ApiResponse<boolean>> {
    return o.pipe(tap((r) => r.IsSuccess && this.clearCache()));
  }
  clearCache(): void {
    this._countries = undefined;
    this._cities = undefined;
  }
}
