import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';

import { ReportsService } from '../../../reports/reports.service';
import { ReportListItem, ReportStatus } from '../../../reports/models/report.models';

@Component({
  selector: 'app-admin-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, ButtonModule, DropdownModule, TableModule, TagModule],
  templateUrl: './reports.component.html',
})
export class ReportsComponent implements OnInit {
  readonly Status = ReportStatus;

  items: ReportListItem[] = [];
  loading = false;
  statusFilter: ReportStatus | null = null;
  statusOptions: { label: string; value: ReportStatus | null }[] = [];

  constructor(
    private reports: ReportsService,
    private toast: MessageService,
    private t: TranslateService
  ) {}

  ngOnInit(): void {
    this.buildOptions();
    this.t.onLangChange.subscribe(() => this.buildOptions());
    this.load();
  }

  private buildOptions(): void {
    this.statusOptions = [
      { label: this.t.instant('reports.statusAll'), value: null },
      { label: this.t.instant('reports.status.Open'), value: ReportStatus.Open },
      { label: this.t.instant('reports.status.Reviewed'), value: ReportStatus.Reviewed },
      { label: this.t.instant('reports.status.ActionTaken'), value: ReportStatus.ActionTaken },
      { label: this.t.instant('reports.status.Dismissed'), value: ReportStatus.Dismissed },
    ];
  }

  load(): void {
    this.loading = true;
    this.reports.getAll(this.statusFilter).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.IsSuccess) this.items = res.Data ?? [];
      },
      error: () => (this.loading = false),
    });
  }

  sev(status: string): 'warning' | 'info' | 'success' | 'secondary' {
    return status === 'Open'
      ? 'warning'
      : status === 'ActionTaken'
        ? 'success'
        : status === 'Reviewed'
          ? 'info'
          : 'secondary';
  }

  handle(report: ReportListItem, status: ReportStatus): void {
    this.reports.handle({ reportUniqueId: report.UniqueId, status }).subscribe({
      next: (res) => {
        if (res.IsSuccess) {
          this.toast.add({ severity: 'success', summary: this.t.instant('reports.handled') });
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
