import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';

import { AuthService } from '../../../../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslateModule, ButtonModule, InputTextModule],
  template: `
    <div class="bg-surface rounded-xl shadow-sm border border-line p-7">
      <h2 class="text-2xl font-extrabold text-ink mb-1">{{ 'auth.forgotTitle' | translate }}</h2>
      <p class="text-muted mb-6">{{ 'auth.forgotSubtitle' | translate }}</p>

      <div *ngIf="sent" class="bg-brand-050 text-brand rounded-lg p-4 text-sm mb-4">
        {{ 'auth.forgotSent' | translate }}
      </div>

      <form *ngIf="!sent" [formGroup]="form" (ngSubmit)="submit()" class="flex flex-col gap-4">
        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium text-ink req">{{ 'fields.email' | translate }}</label>
          <input pInputText type="email" formControlName="email" class="w-full ltr-nums" />
          <small class="text-error" *ngIf="form.controls.email.touched && form.controls.email.invalid">
            {{ 'validation.email' | translate }}
          </small>
        </div>
        <button pButton type="submit" [label]="'auth.sendResetLink' | translate" [loading]="loading" class="w-full mt-2"></button>
      </form>

      <p class="text-center text-sm text-muted mt-5">
        <a routerLink="/auth/login" class="text-brand font-semibold">{{ 'nav.login' | translate }}</a>
      </p>
    </div>
  `,
})
export class ForgotPasswordComponent {
  loading = false;
  sent = false;

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private toast: MessageService,
    private t: TranslateService
  ) {}

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.auth.forgotPassword(this.form.getRawValue()).subscribe({
      next: () => {
        this.loading = false;
        this.sent = true; // always succeeds (no account enumeration)
      },
      error: () => {
        this.loading = false;
        this.sent = true;
      },
    });
  }
}
