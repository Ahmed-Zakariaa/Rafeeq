import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AdminService } from '../../admin.service';
import { DashboardData } from '../../models/admin.models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <h1 class="text-2xl font-extrabold text-ink mb-5">{{ 'admin.nav.dashboard' | translate }}</h1>

    <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
      <div *ngFor="let c of cards" class="bg-surface rounded-xl shadow-sm border border-line p-5">
        <div class="flex items-center gap-2 text-muted text-sm mb-2">
          <i class="pi {{ c.icon }}"></i>
          {{ c.labelKey | translate }}
        </div>
        <div class="text-3xl font-extrabold text-ink ltr-nums">{{ c.value }}</div>
      </div>
    </div>
  `,
})
export class DashboardComponent implements OnInit {
  data?: DashboardData;
  cards: { labelKey: string; icon: string; value: number }[] = [];

  constructor(private admin: AdminService) {}

  ngOnInit(): void {
    this.admin.getDashboard().subscribe((res) => {
      if (!res.IsSuccess || !res.Data) return;
      const d = res.Data;
      this.data = d;
      this.cards = [
        { labelKey: 'admin.stats.totalUsers', icon: 'pi-users', value: d.TotalUsers },
        { labelKey: 'admin.stats.drivers', icon: 'pi-car', value: d.Drivers },
        { labelKey: 'admin.stats.passengers', icon: 'pi-user', value: d.Passengers },
        { labelKey: 'admin.stats.admins', icon: 'pi-shield', value: d.Admins },
        { labelKey: 'admin.stats.totalTrips', icon: 'pi-map', value: d.TotalTrips },
        { labelKey: 'admin.stats.publishedTrips', icon: 'pi-send', value: d.PublishedTrips },
        { labelKey: 'admin.stats.bookings', icon: 'pi-ticket', value: d.TotalBookings },
        { labelKey: 'admin.stats.vehicles', icon: 'pi-car', value: d.TotalVehicles },
      ];
    });
  }
}
