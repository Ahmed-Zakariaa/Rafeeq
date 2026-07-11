import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { TagModule } from 'primeng/tag';

import { AuthService } from '../../services/auth.service';

/** Profile card — confirms the auth flow + JWT claims. Lives inside the main shell. */
@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, TranslateModule, TagModule],
  template: `
    <div class="bg-surface rounded-xl shadow-sm border border-line p-7 max-w-2xl">
      <p class="text-muted">{{ 'home.loggedInAs' | translate }}</p>
      <h1 class="text-2xl font-extrabold text-ink mt-1">
        {{ 'home.welcome' | translate: { name: user?.fullName } }}
      </h1>

      <div class="mt-5">
        <div class="text-sm font-medium text-ink mb-2">{{ 'home.roles' | translate }}</div>
        <div class="flex flex-wrap gap-2">
          <p-tag *ngFor="let r of user?.roles" [value]="r" severity="success"></p-tag>
        </div>
      </div>

      <div class="mt-5 ltr-nums text-xs text-muted break-all">
        <div class="font-medium mb-1">permissions:</div>
        {{ user?.permissions?.join(', ') }}
      </div>
    </div>
  `,
})
export class HomeComponent {
  get user() {
    return this.auth.currentUser;
  }
  constructor(private auth: AuthService) {}
}
