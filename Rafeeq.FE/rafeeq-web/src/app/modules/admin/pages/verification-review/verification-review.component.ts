import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { MessageService } from 'primeng/api';

import { VerificationService } from '../../../verification/verification.service';
import { PendingDocument } from '../../../verification/models/verification.models';

@Component({
  selector: 'app-verification-review',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    ButtonModule,
    TableModule,
    TagModule,
    DialogModule,
    InputTextareaModule,
  ],
  templateUrl: './verification-review.component.html',
})
export class VerificationReviewComponent implements OnInit {
  items: PendingDocument[] = [];
  loading = false;

  rejectVisible = false;
  rejectTarget: PendingDocument | null = null;
  rejectReason = '';
  acting = false;

  constructor(
    private api: VerificationService,
    private toast: MessageService,
    private t: TranslateService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.api.getPending().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.IsSuccess) this.items = res.Data ?? [];
      },
      error: () => (this.loading = false),
    });
  }

  view(doc: PendingDocument): void {
    this.api.getFileBlob(doc.UniqueId).subscribe((blob) => window.open(URL.createObjectURL(blob), '_blank'));
  }

  approve(doc: PendingDocument): void {
    this.acting = true;
    this.api.review({ documentUniqueId: doc.UniqueId, approve: true }).subscribe({
      next: (res) => this.done(res.IsSuccess, 'verification.approved', res.Message),
      error: (err) => this.fail(err),
    });
  }

  openReject(doc: PendingDocument): void {
    this.rejectTarget = doc;
    this.rejectReason = '';
    this.rejectVisible = true;
  }

  submitReject(): void {
    if (!this.rejectTarget || !this.rejectReason.trim()) return;
    this.acting = true;
    this.api
      .review({ documentUniqueId: this.rejectTarget.UniqueId, approve: false, rejectionReason: this.rejectReason })
      .subscribe({
        next: (res) => {
          this.rejectVisible = false;
          this.done(res.IsSuccess, 'verification.rejected', res.Message);
        },
        error: (err) => this.fail(err),
      });
  }

  private done(ok: boolean, successKey: string, message?: string | null): void {
    this.acting = false;
    if (ok) {
      this.toast.add({ severity: 'success', summary: this.t.instant(successKey) });
      this.load();
    } else {
      this.toast.add({ severity: 'error', summary: message || this.t.instant('common.error') });
    }
  }

  private fail(err: unknown): void {
    this.acting = false;
    const message = (err as { error?: { Message?: string } })?.error?.Message;
    this.toast.add({ severity: 'error', summary: message || this.t.instant('common.error') });
  }
}
