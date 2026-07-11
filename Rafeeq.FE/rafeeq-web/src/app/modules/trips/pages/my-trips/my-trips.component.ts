import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';

import { TripsService } from '../../trips.service';
import { TripResult } from '../../models/trip.models';

@Component({
  selector: 'app-my-trips',
  standalone: true,
  imports: [CommonModule, TranslateModule, ButtonModule, TagModule],
  template: `
    <h1 class="text-2xl font-extrabold text-ink mb-1">{{ 'trips.myTripsTitle' | translate }}</h1>
    <p class="text-muted mb-5">{{ 'trips.myTripsSubtitle' | translate }}</p>

    <div *ngIf="trips.length; else empty" class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <div *ngFor="let t of trips" class="bg-surface rounded-xl shadow-sm border border-line p-5 flex flex-col gap-3">
        <div class="flex items-center justify-between">
          <div class="text-lg font-bold text-ink">
            {{ t.OriginCity }} <span class="text-brand">→</span> {{ t.DestinationCity }}
          </div>
          <p-tag [value]="'trips.tripStatus.' + t.Status | translate" [severity]="sev(t.Status)"></p-tag>
        </div>

        <div class="flex items-center gap-2 text-sm text-muted">
          <i class="pi pi-calendar"></i>
          <span class="ltr-nums">{{ t.DepartureDateTime | date: 'yyyy-MM-dd HH:mm' }}</span>
        </div>
        <div class="flex items-center gap-2 text-sm">
          <i class="pi pi-users text-muted"></i>
          <span class="ltr-nums">{{ t.SeatsAvailable }} / {{ t.SeatsLimit }}</span>
          <span class="text-muted">{{ 'trips.seatsAvailable' | translate }}</span>
        </div>

        <div class="flex gap-2 border-t border-line pt-3" *ngIf="canComplete(t) || canCancel(t)">
          <button
            *ngIf="canComplete(t)"
            pButton
            type="button"
            [label]="'trips.markCompleted' | translate"
            icon="pi pi-check"
            class="p-button-sm p-button-success flex-1"
            (click)="complete(t)"
          ></button>
          <button
            *ngIf="canCancel(t)"
            pButton
            type="button"
            [label]="'trips.cancelTrip' | translate"
            icon="pi pi-times"
            class="p-button-sm p-button-outlined p-button-danger flex-1"
            (click)="cancel(t)"
          ></button>
        </div>
      </div>
    </div>

    <ng-template #empty>
      <div class="text-center text-muted py-12">
        <i class="pi pi-map text-4xl mb-3 block"></i>{{ 'trips.noTrips' | translate }}
      </div>
    </ng-template>
  `,
})
export class MyTripsComponent implements OnInit {
  trips: TripResult[] = [];
  loading = false;

  constructor(
    private api: TripsService,
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
        if (res.IsSuccess) this.trips = res.Data ?? [];
      },
      error: () => (this.loading = false),
    });
  }

  canComplete(t: TripResult): boolean {
    return ['Published', 'Full', 'InProgress'].includes(t.Status);
  }

  canCancel(t: TripResult): boolean {
    return !['Completed', 'Cancelled'].includes(t.Status);
  }

  sev(status: string): 'success' | 'warning' | 'info' | 'danger' | 'secondary' {
    switch (status) {
      case 'Published':
        return 'success';
      case 'Full':
        return 'warning';
      case 'InProgress':
        return 'info';
      case 'Cancelled':
        return 'danger';
      default:
        return 'secondary';
    }
  }

  complete(t: TripResult): void {
    this.api.complete(t.UniqueId).subscribe({
      next: (res) => this.after(res.IsSuccess, 'trips.tripCompleted', res.Message),
      error: (err) => this.fail(err),
    });
  }

  cancel(t: TripResult): void {
    this.api.cancel(t.UniqueId).subscribe({
      next: (res) => this.after(res.IsSuccess, 'trips.tripCancelled', res.Message),
      error: (err) => this.fail(err),
    });
  }

  private after(ok: boolean, key: string, message?: string | null): void {
    if (ok) {
      this.toast.add({ severity: 'success', summary: this.t.instant(key) });
      this.load();
    } else {
      this.toast.add({ severity: 'error', summary: message || this.t.instant('common.error') });
    }
  }

  private fail(err: unknown): void {
    const message = (err as { error?: { Message?: string } })?.error?.Message;
    this.toast.add({ severity: 'error', summary: message || this.t.instant('common.error') });
  }
}
