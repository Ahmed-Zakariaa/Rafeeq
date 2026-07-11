import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { LanguageService } from '../../services/language.service';

/** Branded split-screen shell for the auth pages (login/register). */
@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet, TranslateModule, ButtonModule],
  template: `
    <div class="min-h-screen grid lg:grid-cols-2">
      <!-- Brand panel -->
      <aside
        class="hidden lg:flex flex-col justify-between p-10 text-white"
        style="background: linear-gradient(160deg, var(--brand) 0%, var(--brand-600) 100%);"
      >
        <div class="text-3xl font-extrabold">{{ 'app.name' | translate }}</div>
        <div>
          <h1 class="text-4xl font-extrabold leading-snug mb-3">
            {{ 'app.tagline' | translate }}
          </h1>
          <p class="opacity-90">{{ 'auth.registerSubtitle' | translate }}</p>
        </div>
        <div class="opacity-70 text-sm">© Rafeeq</div>
      </aside>

      <!-- Form panel -->
      <main class="flex flex-col items-center justify-center p-6 relative">
        <button
          pButton
          type="button"
          [label]="'nav.language' | translate"
          icon="pi pi-globe"
          class="p-button-text p-button-sm absolute top-4 ltr:right-4 rtl:left-4"
          (click)="lang.toggle()"
        ></button>

        <div class="w-full max-w-md">
          <div class="lg:hidden text-center text-2xl font-extrabold text-brand mb-6">
            {{ 'app.name' | translate }}
          </div>
          <router-outlet></router-outlet>
        </div>
      </main>
    </div>
  `,
})
export class AuthLayoutComponent {
  constructor(public lang: LanguageService) {}
}
