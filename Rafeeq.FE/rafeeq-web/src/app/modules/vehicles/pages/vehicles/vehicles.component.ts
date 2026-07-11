import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TableModule } from 'primeng/table';
import { MessageService } from 'primeng/api';

import { VehiclesService } from '../../vehicles.service';
import { VehicleCreateRequest, VehicleResult } from '../../models/vehicle.models';

@Component({
  selector: 'app-vehicles',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    TableModule,
  ],
  templateUrl: './vehicles.component.html',
})
export class VehiclesComponent implements OnInit {
  vehicles: VehicleResult[] = [];
  loadingList = false;
  saving = false;

  form = this.fb.nonNullable.group({
    make: ['', [Validators.required]],
    model: ['', [Validators.required]],
    color: ['', [Validators.required]],
    plateNumber: ['', [Validators.required]],
    seatsCapacity: [4, [Validators.required, Validators.min(1), Validators.max(8)]],
  });

  constructor(
    private fb: FormBuilder,
    private vehiclesApi: VehiclesService,
    private toast: MessageService,
    private t: TranslateService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loadingList = true;
    this.vehiclesApi.getMine().subscribe({
      next: (res) => {
        this.loadingList = false;
        if (res.IsSuccess) this.vehicles = res.Data ?? [];
      },
      error: () => (this.loadingList = false),
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving = true;
    this.vehiclesApi.create(this.form.getRawValue() as VehicleCreateRequest).subscribe({
      next: (res) => {
        this.saving = false;
        if (res.IsSuccess) {
          this.toast.add({ severity: 'success', summary: this.t.instant('vehicles.created') });
          this.form.reset({ seatsCapacity: 4 });
          this.load();
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
