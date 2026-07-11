import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';

import { AuthService } from '../../services/auth.service';
import { LanguageService } from '../../services/language.service';
import { NotificationBellComponent } from '../../modules/notifications/components/notification-bell/notification-bell.component';

interface AdminNavItem {
  labelKey: string;
  icon: string;
  link: string;
  permission: string;
}

/** Admin portal shell: sidebar (permission-gated) + top bar. Distinct from the rider/driver UX. */
@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    TranslateModule,
    ButtonModule,
    NotificationBellComponent,
  ],
  template: `
    <div class="min-h-screen bg-bg flex flex-col">
      <!-- Top bar -->
      <header class="bg-brand text-white h-14 flex items-center px-4 gap-3 shrink-0">
        <span class="text-lg font-extrabold">{{ 'app.name' | translate }}</span>
        <span class="text-xs bg-white/20 rounded px-2 py-0.5">{{ 'admin.portal' | translate }}</span>
        <div class="flex-1"></div>
        <a
          *ngIf="isRider"
          routerLink="/trips"
          pButton
          type="button"
          [label]="'nav.riderApp' | translate"
          icon="pi pi-car"
          class="p-button-text p-button-sm text-white"
        ></a>
        <app-notification-bell></app-notification-bell>
        <span class="text-sm opacity-90 hidden sm:inline">{{ user?.fullName }}</span>
        <button
          pButton
          type="button"
          [label]="'nav.language' | translate"
          icon="pi pi-globe"
          class="p-button-text p-button-sm text-white"
          (click)="lang.toggle()"
        ></button>
        <button
          pButton
          type="button"
          [label]="'nav.logout' | translate"
          icon="pi pi-sign-out"
          class="p-button-text p-button-sm text-white"
          (click)="logout()"
        ></button>
      </header>

      <div class="flex flex-1 min-h-0">
        <!-- Sidebar -->
        <aside class="w-60 bg-surface border-e border-line p-3 shrink-0 hidden md:block">
          <nav class="flex flex-col gap-1">
            <a
              *ngFor="let item of navItems"
              [routerLink]="item.link"
              routerLinkActive="bg-brand-050 text-brand font-semibold"
              class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm text-ink hover:bg-brand-050"
            >
              <i class="pi {{ item.icon }}"></i>
              {{ item.labelKey | translate }}
            </a>
          </nav>
        </aside>

        <!-- Content -->
        <main class="flex-1 min-w-0 p-5 overflow-auto">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
  `,
})
export class AdminLayoutComponent {
  get user() {
    return this.auth.currentUser;
  }
  get isRider() {
    return this.auth.isRider();
  }

  navItems: AdminNavItem[] = (
    [
      { labelKey: 'admin.nav.dashboard', icon: 'pi-chart-bar', link: '/admin/dashboard', permission: 'Dashboard.View' },
      { labelKey: 'admin.nav.drivers', icon: 'pi-car', link: '/admin/drivers', permission: 'Drivers.View' },
      { labelKey: 'admin.nav.passengers', icon: 'pi-users', link: '/admin/passengers', permission: 'Passengers.View' },
      { labelKey: 'admin.nav.users', icon: 'pi-id-card', link: '/admin/users', permission: 'Users.View' },
      { labelKey: 'admin.nav.admins', icon: 'pi-shield', link: '/admin/admins', permission: 'Admins.View' },
      { labelKey: 'admin.nav.verification', icon: 'pi-verified', link: '/admin/verification', permission: 'Verification.Review' },
      { labelKey: 'admin.nav.reports', icon: 'pi-flag', link: '/admin/reports', permission: 'Reports.Handle' },
      { labelKey: 'admin.nav.roles', icon: 'pi-sitemap', link: '/admin/roles', permission: 'Admins.Manage' },
    ] as AdminNavItem[]
  ).filter((i) => this.auth.hasPermission(i.permission));

  constructor(
    private auth: AuthService,
    private router: Router,
    public lang: LanguageService
  ) {}

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth/login']);
  }
}
