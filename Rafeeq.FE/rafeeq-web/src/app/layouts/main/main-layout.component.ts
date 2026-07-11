import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';

import { AuthService } from '../../services/auth.service';
import { LanguageService } from '../../services/language.service';
import { NotificationBellComponent } from '../../modules/notifications/components/notification-bell/notification-bell.component';

interface NavLink {
  labelKey: string;
  link: string;
  permission: string | null; // null = always visible
}

/** Authenticated rider/driver shell: brand, no-wrap nav, Post-trip CTA, account menu. */
@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    TranslateModule,
    ButtonModule,
    MenuModule,
    NotificationBellComponent,
  ],
  template: `
    <div class="min-h-screen bg-bg flex flex-col">
      <header class="bg-surface border-b border-line sticky top-0 z-20">
        <div class="max-w-6xl mx-auto px-4 h-16 flex items-center gap-3">
          <a routerLink="/trips" class="text-xl font-extrabold text-brand shrink-0">
            {{ 'app.name' | translate }}
          </a>

          <nav class="flex items-center gap-1 flex-1 min-w-0 overflow-x-auto no-scrollbar">
            <a
              *ngFor="let item of navLinks"
              [routerLink]="item.link"
              routerLinkActive="bg-brand-050 text-brand"
              [routerLinkActiveOptions]="{ exact: item.link === '/trips' }"
              class="px-3 py-2 rounded-lg text-sm font-medium text-ink hover:bg-brand-050 whitespace-nowrap"
            >
              {{ item.labelKey | translate }}
            </a>
          </nav>

          <!-- Primary driver action -->
          <button
            *ngIf="canCreateTrip"
            pButton
            type="button"
            routerLink="/trips/create"
            [label]="'nav.postTrip' | translate"
            icon="pi pi-plus"
            class="p-button-sm shrink-0 hidden sm:inline-flex"
          ></button>

          <app-notification-bell class="shrink-0"></app-notification-bell>

          <!-- Account menu -->
          <button
            pButton
            type="button"
            class="p-button-text p-button-sm shrink-0 !text-ink"
            (click)="menu.toggle($event)"
          >
            <i class="pi pi-user-circle text-lg"></i>
            <span class="hidden md:inline mx-2">{{ user?.fullName }}</span>
            <i class="pi pi-angle-down text-xs"></i>
          </button>
          <p-menu #menu [model]="menuItems" [popup]="true" appendTo="body"></p-menu>
        </div>

        <div
          *ngIf="emailUnverified"
          class="bg-warning/15 border-t border-warning/30 text-ink text-sm px-4 py-2 text-center"
        >
          <i class="pi pi-exclamation-circle text-warning"></i>
          {{ 'verifyEmail.banner' | translate }}
          <a routerLink="/verify-email" class="text-brand font-semibold ms-1">{{ 'verifyEmail.verifyNow' | translate }}</a>
        </div>
      </header>

      <main class="flex-1 max-w-6xl w-full mx-auto px-4 py-8">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
})
export class MainLayoutComponent implements OnInit {
  get user() {
    return this.auth.currentUser;
  }
  get isAdmin() {
    return this.auth.isAdmin();
  }
  get emailUnverified() {
    return !this.auth.isEmailVerified();
  }

  canCreateTrip = this.auth.hasPermission('Trips.Create');
  navLinks: NavLink[] = [];
  menuItems: MenuItem[] = [];

  private readonly allLinks: NavLink[] = [
    { labelKey: 'nav.trips', link: '/trips', permission: null },
    { labelKey: 'nav.myTrips', link: '/my-trips', permission: 'Trips.ManageOwn' },
    { labelKey: 'nav.vehicles', link: '/vehicles', permission: 'Vehicles.ManageOwn' },
    { labelKey: 'nav.myBookings', link: '/bookings', permission: 'Bookings.Request' },
    { labelKey: 'nav.requests', link: '/requests', permission: 'Bookings.Respond' },
    { labelKey: 'nav.verification', link: '/verification', permission: 'Verification.Upload' },
  ];

  constructor(
    private auth: AuthService,
    private router: Router,
    private lang: LanguageService,
    private t: TranslateService
  ) {}

  ngOnInit(): void {
    this.navLinks = this.allLinks.filter((l) => !l.permission || this.auth.hasPermission(l.permission));
    this.buildMenu();
    this.t.onLangChange.subscribe(() => this.buildMenu());
  }

  private buildMenu(): void {
    this.menuItems = [
      { label: this.t.instant('nav.language'), icon: 'pi pi-globe', command: () => this.lang.toggle() },
      ...(this.isAdmin
        ? [{ label: this.t.instant('nav.adminPanel'), icon: 'pi pi-shield', routerLink: '/admin' }]
        : []),
      { separator: true },
      { label: this.t.instant('nav.logout'), icon: 'pi pi-sign-out', command: () => this.logout() },
    ];
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth/login']);
  }
}
