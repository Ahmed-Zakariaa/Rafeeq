import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { DropdownModule } from 'primeng/dropdown';
import { SelectButtonModule } from 'primeng/selectbutton';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';

import { AuthService } from '../../../../services/auth.service';
import { LookupsService } from '../../../../services/lookups.service';
import { LanguageService } from '../../../../services/language.service';
import { CountryLookup, countryName } from '../../../../core/lookups';
import { Gender, RegisterRequest } from '../../models/auth.models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    TranslateModule,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    DropdownModule,
    SelectButtonModule,
    CheckboxModule,
  ],
  templateUrl: './register.component.html',
})
export class RegisterComponent implements OnInit {
  loading = false;
  countries: CountryLookup[] = [];
  countryOptions: { label: string; value: number }[] = [];
  genderOptions: { label: string; value: Gender }[] = [];

  form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', [Validators.required]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    gender: [Gender.Male, [Validators.required]],
    countryId: [null as number | null, [Validators.required]],
    asDriver: [false],
  });

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private lookups: LookupsService,
    private toast: MessageService,
    private t: TranslateService,
    public lang: LanguageService
  ) {}

  ngOnInit(): void {
    this.buildOptions();
    this.t.onLangChange.subscribe(() => this.buildOptions());
    this.lookups.countries().subscribe((cs) => {
      this.countries = cs;
      if (cs.length && !this.form.controls.countryId.value) {
        this.form.controls.countryId.setValue(cs[0].Id);
      }
      this.buildOptions();
    });
  }

  get phonePrefix(): string {
    return this.countries.find((c) => c.Id === this.form.controls.countryId.value)?.PhonePrefix ?? '';
  }

  private buildOptions(): void {
    const l = this.lang.current();
    this.countryOptions = this.countries.map((c) => ({
      label: countryName(c, l),
      value: c.Id,
    }));
    this.genderOptions = [
      { label: this.t.instant('fields.male'), value: Gender.Male },
      { label: this.t.instant('fields.female'), value: Gender.Female },
    ];
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.auth.register(this.form.getRawValue() as RegisterRequest).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.IsSuccess) {
          this.toast.add({ severity: 'success', summary: this.t.instant('auth.registerSuccess') });
          this.router.navigate(['/verify-email']); // new accounts verify their email next
        } else {
          this.toast.add({
            severity: 'error',
            summary: res.Message || this.t.instant('auth.error'),
          });
        }
      },
      error: (err) => {
        this.loading = false;
        this.toast.add({
          severity: 'error',
          summary: err?.error?.Message || this.t.instant('auth.error'),
        });
      },
    });
  }
}
