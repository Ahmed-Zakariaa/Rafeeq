import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { InputSwitchModule } from 'primeng/inputswitch';
import { MessageService } from 'primeng/api';

import { LookupsService } from '../../../../services/lookups.service';
import { CityLookup, CountryLookup } from '../../../../core/lookups';

@Component({
  selector: 'app-admin-lookups',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    ButtonModule,
    InputTextModule,
    DropdownModule,
    TableModule,
    DialogModule,
    InputSwitchModule,
  ],
  templateUrl: './lookups.component.html',
})
export class LookupsComponent implements OnInit {
  countries: CountryLookup[] = [];
  cities: CityLookup[] = [];
  countryOptions: { label: string; value: number }[] = [];
  cityCountryFilter: number | null = null;
  loadingCountries = false;
  loadingCities = false;

  // Country dialog
  countryVisible = false;
  editingCountryId: number | null = null;
  countryForm = { nameAr: '', nameEn: '', isoCode: '', currencyCode: '', phonePrefix: '' };
  savingCountry = false;

  // City dialog
  cityVisible = false;
  editingCityId: number | null = null;
  cityForm = { countryId: null as number | null, nameAr: '', nameEn: '' };
  savingCity = false;

  constructor(
    private lookups: LookupsService,
    private toast: MessageService,
    private t: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadCountries();
    this.loadCities();
  }

  loadCountries(): void {
    this.loadingCountries = true;
    this.lookups.adminCountries().subscribe({
      next: (res) => {
        this.loadingCountries = false;
        if (res.IsSuccess) {
          this.countries = res.Data ?? [];
          this.countryOptions = this.countries.map((c) => ({ label: `${c.NameEn} — ${c.NameAr}`, value: c.Id }));
        }
      },
      error: () => (this.loadingCountries = false),
    });
  }

  loadCities(): void {
    this.loadingCities = true;
    this.lookups.adminCities(this.cityCountryFilter ?? undefined).subscribe({
      next: (res) => {
        this.loadingCities = false;
        if (res.IsSuccess) this.cities = res.Data ?? [];
      },
      error: () => (this.loadingCities = false),
    });
  }

  // ── Countries ──
  openAddCountry(): void {
    this.editingCountryId = null;
    this.countryForm = { nameAr: '', nameEn: '', isoCode: '', currencyCode: '', phonePrefix: '' };
    this.countryVisible = true;
  }

  openEditCountry(c: CountryLookup): void {
    this.editingCountryId = c.Id;
    this.countryForm = {
      nameAr: c.NameAr,
      nameEn: c.NameEn,
      isoCode: c.IsoCode,
      currencyCode: c.CurrencyCode,
      phonePrefix: c.PhonePrefix,
    };
    this.countryVisible = true;
  }

  saveCountry(): void {
    const call = this.editingCountryId
      ? this.lookups.updateCountry(this.editingCountryId, this.countryForm)
      : this.lookups.createCountry(this.countryForm);
    this.savingCountry = true;
    call.subscribe({
      next: (res) => this.afterSave(res.IsSuccess, res.Message, () => (this.countryVisible = false), 'countries'),
      error: (err) => this.fail(err, 'country'),
    });
  }

  toggleCountry(c: CountryLookup): void {
    this.lookups.setCountryActive(c.Id, !c.IsActive).subscribe({
      next: (res) => this.afterSave(res.IsSuccess, res.Message, undefined, 'countries'),
      error: (err) => this.fail(err, 'country'),
    });
  }

  // ── Cities ──
  openAddCity(): void {
    this.editingCityId = null;
    this.cityForm = { countryId: this.cityCountryFilter ?? this.countries[0]?.Id ?? null, nameAr: '', nameEn: '' };
    this.cityVisible = true;
  }

  openEditCity(c: CityLookup): void {
    this.editingCityId = c.Id;
    this.cityForm = { countryId: c.CountryId, nameAr: c.NameAr, nameEn: c.NameEn };
    this.cityVisible = true;
  }

  saveCity(): void {
    if (!this.cityForm.countryId) return;
    const call = this.editingCityId
      ? this.lookups.updateCity(this.editingCityId, this.cityForm)
      : this.lookups.createCity(this.cityForm);
    this.savingCity = true;
    call.subscribe({
      next: (res) => this.afterSave(res.IsSuccess, res.Message, () => (this.cityVisible = false), 'cities'),
      error: (err) => this.fail(err, 'city'),
    });
  }

  toggleCity(c: CityLookup): void {
    this.lookups.setCityActive(c.Id, !c.IsActive).subscribe({
      next: (res) => this.afterSave(res.IsSuccess, res.Message, undefined, 'cities'),
      error: (err) => this.fail(err, 'city'),
    });
  }

  countryNameById(id: number): string {
    return this.countries.find((c) => c.Id === id)?.NameEn ?? '';
  }

  private afterSave(ok: boolean, message: string | null | undefined, close: (() => void) | undefined, reload: 'countries' | 'cities'): void {
    this.savingCountry = false;
    this.savingCity = false;
    if (ok) {
      this.toast.add({ severity: 'success', summary: this.t.instant('common.save') });
      close?.();
      if (reload === 'countries') this.loadCountries();
      this.loadCities();
    } else {
      this.toast.add({ severity: 'error', summary: message || this.t.instant('common.error') });
    }
  }

  private fail(err: unknown, _kind: string): void {
    this.savingCountry = false;
    this.savingCity = false;
    const message = (err as { error?: { Message?: string } })?.error?.Message;
    this.toast.add({ severity: 'error', summary: message || this.t.instant('common.error') });
  }
}
