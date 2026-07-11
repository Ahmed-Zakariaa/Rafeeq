import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { MessageService } from 'primeng/api';

import { AuthService } from '../../../../services/auth.service';

/** Handles both initial activation (/auth/set-password) and reset (/auth/reset-password). */
@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslateModule, ButtonModule, PasswordModule],
  template: `
    <div class="bg-surface rounded-xl shadow-sm border border-line p-7">
      <h2 class="text-2xl font-extrabold text-ink mb-1">{{ titleKey | translate }}</h2>
      <p class="text-muted mb-6">{{ subtitleKey | translate }}</p>

      <div *ngIf="!token" class="bg-error/10 text-error rounded-lg p-4 text-sm">
        {{ 'auth.missingToken' | translate }}
      </div>

      <form *ngIf="token" [formGroup]="form" (ngSubmit)="submit()" class="flex flex-col gap-4">
        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium text-ink req">{{ 'auth.newPassword' | translate }}</label>
          <p-password formControlName="newPassword" [toggleMask]="true" [feedback]="true" styleClass="w-full" inputStyleClass="w-full"></p-password>
          <small class="text-error" *ngIf="form.controls.newPassword.touched && form.controls.newPassword.invalid">
            {{ 'validation.minlength' | translate }}
          </small>
        </div>
        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium text-ink req">{{ 'auth.confirmPassword' | translate }}</label>
          <p-password formControlName="confirm" [toggleMask]="true" [feedback]="false" styleClass="w-full" inputStyleClass="w-full"></p-password>
          <small class="text-error" *ngIf="form.controls.confirm.touched && form.hasError('mismatch')">
            {{ 'auth.passwordsMismatch' | translate }}
          </small>
        </div>
        <button pButton type="submit" [label]="submitKey | translate" [loading]="loading" class="w-full mt-2"></button>
      </form>

      <p class="text-center text-sm text-muted mt-5">
        <a routerLink="/auth/login" class="text-brand font-semibold">{{ 'nav.login' | translate }}</a>
      </p>
    </div>
  `,
})
export class ResetPasswordComponent implements OnInit {
  loading = false;
  token = '';
  titleKey = 'auth.resetTitle';
  subtitleKey = 'auth.resetSubtitle';
  submitKey = 'auth.resetSubmit';

  form = this.fb.nonNullable.group(
    {
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirm: ['', [Validators.required]],
    },
    { validators: (g) => (g.value.newPassword === g.value.confirm ? null : { mismatch: true }) }
  );

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private toast: MessageService,
    private t: TranslateService
  ) {}

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    const data = this.route.snapshot.data;
    if (data['mode'] === 'set') {
      this.titleKey = 'auth.setTitle';
      this.subtitleKey = 'auth.setSubtitle';
      this.submitKey = 'auth.setSubmit';
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.auth.resetPassword({ token: this.token, newPassword: this.form.getRawValue().newPassword }).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.IsSuccess) {
          this.toast.add({ severity: 'success', summary: this.t.instant('auth.passwordSet') });
          this.router.navigate(['/auth/login']);
        } else {
          this.toast.add({ severity: 'error', summary: res.Message || this.t.instant('auth.invalidToken') });
        }
      },
      error: (err) => {
        this.loading = false;
        this.toast.add({ severity: 'error', summary: err?.error?.Message || this.t.instant('auth.invalidToken') });
      },
    });
  }
}
