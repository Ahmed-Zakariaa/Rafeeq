import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { CheckboxModule } from 'primeng/checkbox';
import { TagModule } from 'primeng/tag';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { DialogModule } from 'primeng/dialog';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { MessageService } from 'primeng/api';

import { TripsService } from '../../trips.service';
import { BookingsService } from '../../../bookings/bookings.service';
import { AuthService } from '../../../../services/auth.service';
import {
  GenderPreference,
  TripResult,
  TripSearchFilter,
  TripSortField,
} from '../../models/trip.models';
import { CITIES, cityName } from '../../../../core/lookups';
import { LanguageService } from '../../../../services/language.service';

@Component({
  selector: 'app-trips-search',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    TranslateModule,
    ButtonModule,
    DropdownModule,
    CalendarModule,
    CheckboxModule,
    TagModule,
    PaginatorModule,
    DialogModule,
    InputTextareaModule,
  ],
  templateUrl: './trips-search.component.html',
})
export class TripsSearchComponent implements OnInit {
  cityOptions: { label: string; value: number }[] = [];
  genderOptions: { label: string; value: number }[] = [];
  sortOptions: { label: string; value: TripSortField }[] = [];

  results: TripResult[] = [];
  total = 0;
  pageNumber = 1;
  pageSize = 10;
  loading = false;
  searched = false;

  form = this.fb.group({
    originCityId: [null as number | null],
    destinationCityId: [null as number | null],
    departureDate: [null as Date | null],
    genderPreference: [null as GenderPreference | null],
    freeOnly: [false],
    sortField: [TripSortField.DepartureDateTime as TripSortField],
    sortType: [0], // 0 Asc, 1 Desc
  });

  // Booking request
  canRequest = this.auth.hasPermission('Bookings.Request');
  requestVisible = false;
  requestTarget: TripResult | null = null;
  pickupNote = '';
  requesting = false;

  constructor(
    private fb: FormBuilder,
    private tripsApi: TripsService,
    private bookings: BookingsService,
    private auth: AuthService,
    private toast: MessageService,
    private t: TranslateService,
    public lang: LanguageService
  ) {}

  openRequest(trip: TripResult): void {
    this.requestTarget = trip;
    this.pickupNote = '';
    this.requestVisible = true;
  }

  submitRequest(): void {
    if (!this.requestTarget) return;
    this.requesting = true;
    this.bookings
      .request({ tripUniqueId: this.requestTarget.UniqueId, pickupNote: this.pickupNote || null })
      .subscribe({
        next: (res) => {
          this.requesting = false;
          if (res.IsSuccess) {
            this.toast.add({ severity: 'success', summary: this.t.instant('bookings.requested') });
            this.requestVisible = false;
          } else {
            this.toast.add({ severity: 'error', summary: res.Message || this.t.instant('common.error') });
          }
        },
        error: (err) => {
          this.requesting = false;
          this.toast.add({ severity: 'error', summary: err?.error?.Message || this.t.instant('common.error') });
        },
      });
  }

  ngOnInit(): void {
    this.buildOptions();
    this.t.onLangChange.subscribe(() => this.buildOptions());
    this.search();
  }

  private buildOptions(): void {
    const l = this.lang.current();
    this.cityOptions = CITIES.map((c) => ({ label: cityName(c, l), value: c.id }));
    this.genderOptions = [
      { label: this.t.instant('trips.gender.Any'), value: GenderPreference.Any },
      { label: this.t.instant('trips.gender.MaleOnly'), value: GenderPreference.MaleOnly },
      { label: this.t.instant('trips.gender.FemaleOnly'), value: GenderPreference.FemaleOnly },
    ];
    this.sortOptions = [
      { label: this.t.instant('trips.sort.departure'), value: TripSortField.DepartureDateTime },
      { label: this.t.instant('trips.sort.price'), value: TripSortField.PricePerSeat },
      { label: this.t.instant('trips.sort.seats'), value: TripSortField.SeatsAvailable },
    ];
  }

  search(resetPage = true): void {
    if (resetPage) this.pageNumber = 1;
    const v = this.form.getRawValue();
    const filter: TripSearchFilter = {
      originCityId: v.originCityId,
      destinationCityId: v.destinationCityId,
      departureDate: v.departureDate ? toDateOnly(v.departureDate) : null,
      genderPreference: v.genderPreference,
      freeOnly: v.freeOnly,
    };

    this.loading = true;
    this.tripsApi
      .search({
        filterModel: filter,
        orderModel: {
          fieldName: v.sortField ?? TripSortField.DepartureDateTime,
          sortType: v.sortType ?? 0,
        },
        pageNumber: this.pageNumber,
        pageSize: this.pageSize,
      })
      .subscribe({
        next: (res) => {
          this.loading = false;
          this.searched = true;
          if (res.IsSuccess) {
            this.results = res.Data ?? [];
            this.total = res.Total;
          } else {
            this.toast.add({ severity: 'error', summary: res.Message || this.t.instant('common.error') });
          }
        },
        error: (err) => {
          this.loading = false;
          this.searched = true;
          this.toast.add({
            severity: 'error',
            summary: err?.error?.Message || this.t.instant('common.error'),
          });
        },
      });
  }

  onPage(e: PaginatorState): void {
    this.pageNumber = (e.page ?? 0) + 1;
    this.pageSize = e.rows ?? this.pageSize;
    this.search(false);
  }
}

/** Local yyyy-MM-dd (avoids UTC shift from toISOString). */
function toDateOnly(d: Date): string {
  const m = `${d.getMonth() + 1}`.padStart(2, '0');
  const day = `${d.getDate()}`.padStart(2, '0');
  return `${d.getFullYear()}-${m}-${day}`;
}
