import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';

import { BookingsService } from '../../bookings.service';
import { BookingResult, CANCELLABLE } from '../../models/booking.models';
import { bookingSeverity } from '../../booking-status';
import { ReportButtonComponent } from '../../../reports/components/report-button/report-button.component';
import { RateButtonComponent } from '../../../ratings/components/rate-button/rate-button.component';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule, TranslateModule, ButtonModule, TagModule, ReportButtonComponent, RateButtonComponent],
  template: `
    <h1 class="text-2xl font-extrabold text-ink mb-1">{{ 'bookings.myTitle' | translate }}</h1>
    <p class="text-muted mb-5">{{ 'bookings.mySubtitle' | translate }}</p>

    <div *ngIf="bookings.length; else empty" class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <div *ngFor="let b of bookings" class="bg-surface rounded-xl shadow-sm border border-line p-5 flex flex-col gap-3">
        <div class="flex items-center justify-between">
          <div class="text-lg font-bold text-ink">
            {{ b.OriginCity }} <span class="text-brand">→</span> {{ b.DestinationCity }}
          </div>
          <p-tag [value]="'bookings.status.' + b.Status | translate" [severity]="sev(b.Status)"></p-tag>
        </div>

        <div class="flex items-center gap-2 text-sm text-muted">
          <i class="pi pi-calendar"></i>
          <span class="ltr-nums">{{ b.DepartureDateTime | date: 'yyyy-MM-dd HH:mm' }}</span>
        </div>
        <div class="flex items-center gap-2 text-sm text-muted">
          <i class="pi pi-user"></i><span>{{ b.DriverName }}</span>
        </div>
        <div *ngIf="b.PickupNote" class="text-sm text-muted">
          <i class="pi pi-map-marker"></i> {{ b.PickupNote }}
        </div>

        <div class="flex items-center justify-between">
          <div class="font-bold ltr-nums" [class.text-brand]="!b.IsFree" [class.text-success]="b.IsFree">
            <span *ngIf="!b.IsFree">{{ b.AgreedPrice }} {{ b.CurrencyCode }}</span>
            <span *ngIf="b.IsFree">{{ 'trips.free' | translate }}</span>
          </div>
          <div *ngIf="b.ContactPhone" class="text-sm text-success ltr-nums flex items-center gap-1">
            <i class="pi pi-phone"></i> {{ b.ContactPhone }}
          </div>
        </div>

        <button
          *ngIf="canCancel(b)"
          pButton
          type="button"
          [label]="'bookings.cancel' | translate"
          icon="pi pi-times"
          class="p-button-outlined p-button-danger p-button-sm"
          (click)="cancel(b)"
        ></button>
        <div class="border-t border-line pt-2 flex justify-end gap-2">
          <app-rate-button
            *ngIf="b.Status === 'Completed' && !b.HasRated"
            [bookingUniqueId]="b.UniqueId"
          ></app-rate-button>
          <app-report-button [bookingUniqueId]="b.UniqueId"></app-report-button>
        </div>
      </div>
    </div>

    <ng-template #empty>
      <div class="text-center text-muted py-12">
        <i class="pi pi-ticket text-4xl mb-3 block"></i>{{ 'bookings.none' | translate }}
      </div>
    </ng-template>
  `,
})
export class MyBookingsComponent implements OnInit {
  bookings: BookingResult[] = [];
  loading = false;

  constructor(
    private api: BookingsService,
    private toast: MessageService,
    private t: TranslateService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.api.getMine().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.IsSuccess) this.bookings = res.Data ?? [];
      },
      error: () => (this.loading = false),
    });
  }

  canCancel(b: BookingResult): boolean {
    return CANCELLABLE.includes(b.Status);
  }

  sev(status: string) {
    return bookingSeverity(status);
  }

  cancel(b: BookingResult): void {
    this.api.cancel(b.UniqueId).subscribe({
      next: (res) => {
        if (res.IsSuccess) {
          this.toast.add({ severity: 'success', summary: this.t.instant('bookings.cancelled') });
          this.load();
        } else {
          this.toast.add({ severity: 'error', summary: res.Message || this.t.instant('common.error') });
        }
      },
      error: (err) =>
        this.toast.add({ severity: 'error', summary: err?.error?.Message || this.t.instant('common.error') }),
    });
  }
}
