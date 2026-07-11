import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { RatingModule } from 'primeng/rating';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { MessageService } from 'primeng/api';

import { RatingsService } from '../../ratings.service';

/** "Rate" button + stars/comment dialog, used after a booking is completed. */
@Component({
  selector: 'app-rate-button',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, ButtonModule, DialogModule, RatingModule, InputTextareaModule],
  template: `
    <button
      pButton
      type="button"
      [label]="'ratings.rate' | translate"
      icon="pi pi-star"
      class="p-button-sm p-button-outlined"
      (click)="open()"
    ></button>

    <p-dialog
      [(visible)]="visible"
      [modal]="true"
      [style]="{ width: '26rem' }"
      [header]="'ratings.title' | translate"
      [dismissableMask]="true"
    >
      <div class="pt-2 flex flex-col gap-4">
        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium text-ink req">{{ 'ratings.stars' | translate }}</label>
          <p-rating [(ngModel)]="stars" [cancel]="false"></p-rating>
        </div>
        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium text-ink">{{ 'ratings.comment' | translate }}</label>
          <textarea pInputTextarea [(ngModel)]="comment" rows="3" class="w-full" maxlength="500"></textarea>
        </div>
      </div>
      <ng-template pTemplate="footer">
        <button pButton type="button" [label]="'common.cancel' | translate" class="p-button-text" (click)="visible = false"></button>
        <button pButton type="button" [label]="'ratings.submit' | translate" [loading]="saving" (click)="submit()"></button>
      </ng-template>
    </p-dialog>
  `,
})
export class RateButtonComponent {
  @Input() bookingUniqueId!: string;

  visible = false;
  saving = false;
  stars = 5;
  comment = '';

  constructor(
    private ratings: RatingsService,
    private toast: MessageService,
    private t: TranslateService
  ) {}

  open(): void {
    this.stars = 5;
    this.comment = '';
    this.visible = true;
  }

  submit(): void {
    if (!this.stars) {
      this.toast.add({ severity: 'warn', summary: this.t.instant('ratings.pickStars') });
      return;
    }
    this.saving = true;
    this.ratings
      .rate({ bookingUniqueId: this.bookingUniqueId, stars: this.stars, comment: this.comment || null })
      .subscribe({
        next: (res) => {
          this.saving = false;
          if (res.IsSuccess) {
            this.toast.add({ severity: 'success', summary: this.t.instant('ratings.thanks') });
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
