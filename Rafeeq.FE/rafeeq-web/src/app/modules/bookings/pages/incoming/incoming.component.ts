import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';

import { BookingsService } from '../../bookings.service';
import { BookingResult } from '../../models/booking.models';
import { bookingSeverity } from '../../booking-status';
import { ReportButtonComponent } from '../../../reports/components/report-button/report-button.component';
import { RateButtonComponent } from '../../../ratings/components/rate-button/rate-button.component';

@Component({
  selector: 'app-incoming-requests',
  standalone: true,
  imports: [CommonModule, TranslateModule, ButtonModule, TagModule, ReportButtonComponent, RateButtonComponent],
  template: `
    <h1 class="text-2xl font-extrabold text-ink mb-1">{{ 'bookings.incomingTitle' | translate }}</h1>
    <p class="text-muted mb-5">{{ 'bookings.incomingSubtitle' | translate }}</p>

    <div *ngIf="items.length; else empty" class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <div *ngFor="let b of items" class="bg-surface rounded-xl shadow-sm border border-line p-5 flex flex-col gap-3">
        <div class="flex items-center justify-between">
          <div class="text-lg font-bold text-ink">{{ b.PassengerName }}</div>
          <p-tag [value]="'bookings.status.' + b.Status | translate" [severity]="sev(b.Status)"></p-tag>
        </div>

        <div class="flex items-center gap-2 text-sm text-muted">
          <i class="pi pi-directions"></i>
          <span>{{ b.OriginCity }} <span class="text-brand">→</span> {{ b.DestinationCity }}</span>
        </div>
        <div class="flex items-center gap-2 text-sm text-muted">
          <i class="pi pi-calendar"></i>
          <span class="ltr-nums">{{ b.DepartureDateTime | date: 'yyyy-MM-dd HH:mm' }}</span>
        </div>
        <div *ngIf="b.PickupNote" class="text-sm text-muted">
          <i class="pi pi-map-marker"></i> {{ b.PickupNote }}
        </div>
        <div *ngIf="b.ContactPhone" class="text-sm text-success ltr-nums flex items-center gap-1">
          <i class="pi pi-phone"></i> {{ b.ContactPhone }}
        </div>

        <div *ngIf="b.Status === 'Requested'" class="flex gap-2">
          <button
            pButton
            type="button"
            [label]="'bookings.accept' | translate"
            icon="pi pi-check"
            class="p-button-sm p-button-success flex-1"
            (click)="respond(b, true)"
          ></button>
          <button
            pButton
            type="button"
            [label]="'bookings.reject' | translate"
            icon="pi pi-times"
            class="p-button-sm p-button-outlined p-button-danger flex-1"
            (click)="respond(b, false)"
          ></button>
        </div>

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
        <i class="pi pi-inbox text-4xl mb-3 block"></i>{{ 'bookings.noIncoming' | translate }}
      </div>
    </ng-template>
  `,
})
export class IncomingComponent implements OnInit {
  items: BookingResult[] = [];
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
    this.api.getIncoming().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.IsSuccess) this.items = res.Data ?? [];
      },
      error: () => (this.loading = false),
    });
  }

  sev(status: string) {
    return bookingSeverity(status);
  }

  respond(b: BookingResult, accept: boolean): void {
    this.api.respond({ bookingUniqueId: b.UniqueId, accept }).subscribe({
      next: (res) => {
        if (res.IsSuccess) {
          this.toast.add({
            severity: 'success',
            summary: this.t.instant(accept ? 'bookings.accepted' : 'bookings.rejected'),
          });
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
