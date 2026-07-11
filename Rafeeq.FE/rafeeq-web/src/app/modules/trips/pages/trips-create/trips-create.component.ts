import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';

import { TripsService } from '../../trips.service';
import { VehiclesService } from '../../../vehicles/vehicles.service';
import { VehicleResult } from '../../../vehicles/models/vehicle.models';
import { GenderPreference, MAX_PRICE_PER_SEAT, TripCreateRequest } from '../../models/trip.models';
import { CityLookup, cityName } from '../../../../core/lookups';
import { LookupsService } from '../../../../services/lookups.service';
import { LanguageService } from '../../../../services/language.service';

@Component({
  selector: 'app-trips-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    TranslateModule,
    ButtonModule,
    InputTextModule,
    InputTextareaModule,
    InputNumberModule,
    DropdownModule,
    CalendarModule,
    CheckboxModule,
  ],
  templateUrl: './trips-create.component.html',
})
export class TripsCreateComponent implements OnInit {
  readonly maxPrice = MAX_PRICE_PER_SEAT;
  minDate = new Date();

  vehicles: VehicleResult[] = [];
  vehicleOptions: { label: string; value: number; seats: number }[] = [];
  allCities: CityLookup[] = [];
  cityOptions: { label: string; value: number }[] = [];
  genderOptions: { label: string; value: number }[] = [];

  loadingVehicles = false;
  saving = false;

  form = this.fb.nonNullable.group({
    vehicleId: [null as number | null, [Validators.required]],
    originCityId: [null as number | null, [Validators.required]],
    destinationCityId: [null as number | null, [Validators.required]],
    departureDateTime: [null as Date | null, [Validators.required]],
    seatsLimit: [3, [Validators.required, Validators.min(1), Validators.max(8)]],
    isFree: [false],
    pricePerSeat: [0 as number, []],
    genderPreference: [GenderPreference.Any],
    pickupPointText: ['', [Validators.required, Validators.maxLength(200)]],
    notes: ['' as string],
  });

  constructor(
    private fb: FormBuilder,
    private tripsApi: TripsService,
    private vehiclesApi: VehiclesService,
    private lookups: LookupsService,
    private router: Router,
    private toast: MessageService,
    private t: TranslateService,
    public lang: LanguageService
  ) {}

  ngOnInit(): void {
    this.buildOptions();
    this.t.onLangChange.subscribe(() => this.buildOptions());
    this.applyPriceValidators(this.form.controls.isFree.value);
    this.form.controls.isFree.valueChanges.subscribe((free) => this.applyPriceValidators(free));
    this.lookups.cities().subscribe((c) => {
      this.allCities = c;
      this.buildOptions();
    });
    this.loadVehicles();
  }

  get hasVehicles(): boolean {
    return this.vehicles.length > 0;
  }

  get selectedVehicleSeats(): number | null {
    const id = this.form.controls.vehicleId.value;
    return this.vehicleOptions.find((v) => v.value === id)?.seats ?? null;
  }

  private applyPriceValidators(isFree: boolean): void {
    const price = this.form.controls.pricePerSeat;
    if (isFree) {
      price.clearValidators();
      price.setValue(0);
    } else {
      price.setValidators([Validators.required, Validators.min(1), Validators.max(this.maxPrice)]);
    }
    price.updateValueAndValidity();
  }

  private buildOptions(): void {
    const l = this.lang.current();
    this.cityOptions = this.allCities.map((c) => ({ label: cityName(c, l), value: c.Id }));
    this.genderOptions = [
      { label: this.t.instant('trips.gender.Any'), value: GenderPreference.Any },
      { label: this.t.instant('trips.gender.MaleOnly'), value: GenderPreference.MaleOnly },
      { label: this.t.instant('trips.gender.FemaleOnly'), value: GenderPreference.FemaleOnly },
    ];
  }

  private loadVehicles(): void {
    this.loadingVehicles = true;
    this.vehiclesApi.getMine().subscribe({
      next: (res) => {
        this.loadingVehicles = false;
        this.vehicles = res.IsSuccess ? res.Data ?? [] : [];
        this.vehicleOptions = this.vehicles.map((v) => ({
          label: `${v.Make} ${v.Model} · ${v.PlateNumber}`,
          value: v.Id,
          seats: v.SeatsCapacity,
        }));
      },
      error: () => (this.loadingVehicles = false),
    });
  }

  submit(): void {
    const v = this.form.getRawValue();

    if (v.originCityId && v.originCityId === v.destinationCityId) {
      this.toast.add({ severity: 'error', summary: this.t.instant('trips.sameCityError') });
      return;
    }
    if (this.selectedVehicleSeats && v.seatsLimit > this.selectedVehicleSeats) {
      this.toast.add({ severity: 'error', summary: this.t.instant('trips.seatsExceedCapacity') });
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const req: TripCreateRequest = {
      vehicleId: v.vehicleId!,
      originCityId: v.originCityId!,
      destinationCityId: v.destinationCityId!,
      departureDateTime: v.departureDateTime!.toISOString(),
      seatsLimit: v.seatsLimit,
      isFree: v.isFree,
      pricePerSeat: v.isFree ? 0 : v.pricePerSeat,
      genderPreference: v.genderPreference,
      pickupPointText: v.pickupPointText,
      notes: v.notes || null,
    };

    this.saving = true;
    this.tripsApi.create(req).subscribe({
      next: (res) => {
        this.saving = false;
        if (res.IsSuccess) {
          this.toast.add({ severity: 'success', summary: this.t.instant('trips.created') });
          this.router.navigate(['/trips']);
        } else {
          this.toast.add({ severity: 'error', summary: res.Message || this.t.instant('common.error') });
        }
      },
      error: (err) => {
        this.saving = false;
        this.toast.add({
          severity: 'error',
          summary: err?.error?.Message || this.t.instant('common.error'),
        });
      },
    });
  }
}
