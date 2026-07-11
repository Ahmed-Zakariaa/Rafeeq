import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { MessageService } from 'primeng/api';

import { ReportsService } from '../../reports.service';
import { ReportReason } from '../../models/report.models';

/** Small "Report" link + dialog, reused wherever a booking counterparty can be reported. */
@Component({
  selector: 'app-report-button',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, ButtonModule, DialogModule, DropdownModule, InputTextareaModule],
  template: `
    <button
      pButton
      type="button"
      [label]="'reports.report' | translate"
      icon="pi pi-flag"
      class="p-button-text p-button-sm p-button-danger"
      (click)="open()"
    ></button>

    <p-dialog
      [(visible)]="visible"
      [modal]="true"
      [style]="{ width: '28rem' }"
      [header]="'reports.title' | translate"
      [dismissableMask]="true"
    >
      <div class="pt-2 flex flex-col gap-4">
        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium text-ink req">{{ 'reports.reasonLabel' | translate }}</label>
          <p-dropdown
            [(ngModel)]="reason"
            [options]="reasonOptions"
            optionLabel="label"
            optionValue="value"
            styleClass="w-full"
          ></p-dropdown>
        </div>
        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium text-ink">{{ 'reports.description' | translate }}</label>
          <textarea pInputTextarea [(ngModel)]="description" rows="3" class="w-full" maxlength="500"></textarea>
        </div>
      </div>
      <ng-template pTemplate="footer">
        <button pButton type="button" [label]="'common.cancel' | translate" class="p-button-text" (click)="visible = false"></button>
        <button pButton type="button" [label]="'reports.submit' | translate" class="p-button-danger" [loading]="saving" (click)="submit()"></button>
      </ng-template>
    </p-dialog>
  `,
})
export class ReportButtonComponent implements OnInit {
  @Input() bookingUniqueId!: string;

  visible = false;
  saving = false;
  reason: ReportReason = ReportReason.InappropriateBehavior;
  description = '';
  reasonOptions: { label: string; value: ReportReason }[] = [];

  constructor(
    private reports: ReportsService,
    private toast: MessageService,
    private t: TranslateService
  ) {}

  ngOnInit(): void {
    this.buildOptions();
    this.t.onLangChange.subscribe(() => this.buildOptions());
  }

  private buildOptions(): void {
    this.reasonOptions = [
      { label: this.t.instant('reports.reason.InappropriateBehavior'), value: ReportReason.InappropriateBehavior },
      { label: this.t.instant('reports.reason.NoShow'), value: ReportReason.NoShow },
      { label: this.t.instant('reports.reason.UnsafeDriving'), value: ReportReason.UnsafeDriving },
      { label: this.t.instant('reports.reason.Harassment'), value: ReportReason.Harassment },
      { label: this.t.instant('reports.reason.Other'), value: ReportReason.Other },
    ];
  }

  open(): void {
    this.reason = ReportReason.InappropriateBehavior;
    this.description = '';
    this.visible = true;
  }

  submit(): void {
    this.saving = true;
    this.reports
      .create({ bookingUniqueId: this.bookingUniqueId, reason: this.reason, description: this.description || null })
      .subscribe({
        next: (res) => {
          this.saving = false;
          if (res.IsSuccess) {
            this.toast.add({ severity: 'success', summary: this.t.instant('reports.sent') });
            this.visible = false;
          } else {
            this.toast.add({ severity: 'error', summary: res.Message || this.t.instant('common.error') });
          }
        },
        error: (err) => {
          this.saving = false;
          this.toast.add({ severity: 'error', summary: err?.error?.Message || this.t.instant('common.error') });
        },
      });
  }
}
