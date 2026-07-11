import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';

import { VerificationService } from '../../verification.service';
import { DocType, MyDocument } from '../../models/verification.models';
import { docSeverity } from '../../doc-status';

@Component({
  selector: 'app-verification',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, ButtonModule, DropdownModule, TableModule, TagModule],
  templateUrl: './verification.component.html',
})
export class VerificationComponent implements OnInit {
  docs: MyDocument[] = [];
  loading = false;
  uploading = false;

  docType: DocType = DocType.NationalId;
  file: File | null = null;
  docTypeOptions: { label: string; value: DocType }[] = [];

  constructor(
    private api: VerificationService,
    private toast: MessageService,
    private t: TranslateService
  ) {}

  ngOnInit(): void {
    this.buildOptions();
    this.t.onLangChange.subscribe(() => this.buildOptions());
    this.load();
  }

  private buildOptions(): void {
    this.docTypeOptions = [
      { label: this.t.instant('verification.docType.NationalId'), value: DocType.NationalId },
      { label: this.t.instant('verification.docType.DriverLicense'), value: DocType.DriverLicense },
      { label: this.t.instant('verification.docType.VehicleRegistration'), value: DocType.VehicleRegistration },
    ];
  }

  load(): void {
    this.loading = true;
    this.api.getMine().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.IsSuccess) this.docs = res.Data ?? [];
      },
      error: () => (this.loading = false),
    });
  }

  onFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.file = input.files?.[0] ?? null;
  }

  upload(): void {
    if (!this.file) {
      this.toast.add({ severity: 'warn', summary: this.t.instant('verification.pickFile') });
      return;
    }
    this.uploading = true;
    this.api.upload(this.docType, this.file).subscribe({
      next: (res) => {
        this.uploading = false;
        if (res.IsSuccess) {
          this.toast.add({ severity: 'success', summary: this.t.instant('verification.uploaded') });
          this.file = null;
          this.load();
        } else {
          this.toast.add({ severity: 'error', summary: res.Message || this.t.instant('common.error') });
        }
      },
      error: (err) => {
        this.uploading = false;
        this.toast.add({ severity: 'error', summary: err?.error?.Message || this.t.instant('common.error') });
      },
    });
  }

  view(doc: MyDocument): void {
    this.api.getFileBlob(doc.UniqueId).subscribe((blob) => window.open(URL.createObjectURL(blob), '_blank'));
  }

  sev(status: string) {
    return docSeverity(status);
  }
}
