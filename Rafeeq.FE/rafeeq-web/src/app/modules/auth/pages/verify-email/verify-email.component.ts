import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';

import { AuthService } from '../../../../services/auth.service';
import { LanguageService } from '../../../../services/language.service';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, ButtonModule, InputTextModule],
  template: `
    <div class="min-h-screen bg-bg flex items-center justify-center p-6">
      <div class="w-full max-w-md bg-surface rounded-xl shadow-sm border border-line p-7">
        <button
          pButton
          type="button"
          [label]="'nav.language' | translate"
          icon="pi pi-globe"
          class="p-button-text p-button-sm mb-2"
          (click)="lang.toggle()"
        ></button>

        <h2 class="text-2xl font-extrabold text-ink mb-1">{{ 'verifyEmail.title' | translate }}</h2>
        <p class="text-muted mb-6">{{ 'verifyEmail.subtitle' | translate }}</p>

        <div class="flex flex-col gap-1.5 mb-4">
          <label class="text-sm font-medium text-ink req">{{ 'verifyEmail.codeLabel' | translate }}</label>
          <input
            pInputText
            [(ngModel)]="code"
            inputmode="numeric"
            maxlength="6"
            class="w-full ltr-nums tracking-[0.5em] text-center text-lg"
            placeholder="••••••"
          />
        </div>

        <button
          pButton
          type="button"
          [label]="'verifyEmail.verify' | translate"
          [loading]="loading"
          [disabled]="code.length < 6"
          class="w-full mb-3"
          (click)="verify()"
        ></button>

        <div class="flex items-center justify-between text-sm">
          <button type="button" class="text-brand font-medium" (click)="resend()" [disabled]="resending">
            {{ 'verifyEmail.resend' | translate }}
          </button>
          <button type="button" class="text-muted hover:text-ink" (click)="logout()">
            {{ 'nav.logout' | translate }}
          </button>
        </div>
      </div>
    </div>
  `,
})
export class VerifyEmailComponent implements OnInit {
  code = '';
  loading = false;
  resending = false;

  constructor(
    private auth: AuthService,
    private router: Router,
    private toast: MessageService,
    private t: TranslateService,
    public lang: LanguageService
  ) {}

  ngOnInit(): void {
    if (this.auth.isEmailVerified()) this.router.navigate([this.auth.defaultRoute()]);
  }

  verify(): void {
    this.loading = true;
    this.auth.verifyEmail(this.code.trim()).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.IsSuccess) {
          this.toast.add({ severity: 'success', summary: this.t.instant('verifyEmail.verified') });
          this.router.navigate([this.auth.defaultRoute()]);
        } else {
          this.toast.add({ severity: 'error', summary: res.Message || this.t.instant('common.error') });
        }
      },
      error: (err) => {
        this.loading = false;
        this.toast.add({ severity: 'error', summary: err?.error?.Message || this.t.instant('common.error') });
      },
    });
  }

  resend(): void {
    this.resending = true;
    this.auth.resendOtp().subscribe({
      next: () => {
        this.resending = false;
        this.toast.add({ severity: 'success', summary: this.t.instant('verifyEmail.sent') });
      },
      error: () => (this.resending = false),
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth/login']);
  }
}
