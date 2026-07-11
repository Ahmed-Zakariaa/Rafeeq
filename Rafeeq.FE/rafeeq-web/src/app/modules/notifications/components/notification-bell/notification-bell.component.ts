import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { OverlayPanelModule } from 'primeng/overlaypanel';

import { NotificationItem, NotificationsService } from '../../notifications.service';

/** Bell with unread badge + dropdown list. Reused in both shells. */
@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, TranslateModule, ButtonModule, OverlayPanelModule],
  template: `
    <button
      type="button"
      class="relative p-2 rounded-full hover:bg-black/10 text-current"
      (click)="op.toggle($event)"
      [attr.aria-label]="'notif.title' | translate"
    >
      <i class="pi pi-bell text-lg"></i>
      <span
        *ngIf="unread > 0"
        class="absolute top-0 ltr:right-0 rtl:left-0 bg-error text-white text-[10px] leading-none rounded-full px-1.5 py-0.5 min-w-[16px] text-center ltr-nums"
      >
        {{ unread > 9 ? '9+' : unread }}
      </span>
    </button>

    <p-overlayPanel #op (onShow)="loadList()" appendTo="body" styleClass="!p-0">
      <div class="w-80">
        <div class="flex items-center justify-between px-3 py-2 border-b border-line">
          <span class="font-bold text-ink">{{ 'notif.title' | translate }}</span>
          <button
            *ngIf="items.length"
            type="button"
            class="text-xs text-brand font-medium"
            (click)="markAll()"
          >
            {{ 'notif.markAllRead' | translate }}
          </button>
        </div>

        <div class="max-h-96 overflow-auto">
          <button
            *ngFor="let n of items"
            type="button"
            class="w-full text-start px-3 py-2.5 border-b border-line hover:bg-bg flex flex-col gap-0.5"
            [class.bg-brand-050]="!n.IsRead"
            (click)="open(n)"
          >
            <span class="text-sm text-ink">{{ n.TitleKey | translate }}</span>
            <span *ngIf="n.Body" class="text-xs text-muted">{{ n.Body }}</span>
            <span class="text-[10px] text-muted ltr-nums">{{ n.CreatedDate | date: 'yyyy-MM-dd HH:mm' }}</span>
          </button>

          <div *ngIf="!items.length" class="text-center text-muted py-8 text-sm">
            {{ 'notif.empty' | translate }}
          </div>
        </div>
      </div>
    </p-overlayPanel>
  `,
})
export class NotificationBellComponent implements OnInit {
  unread = 0;
  items: NotificationItem[] = [];

  constructor(private api: NotificationsService) {}

  ngOnInit(): void {
    this.loadCount();
  }

  loadCount(): void {
    this.api.unreadCount().subscribe((res) => {
      if (res.IsSuccess) this.unread = res.Data ?? 0;
    });
  }

  loadList(): void {
    this.api.getMine().subscribe((res) => {
      if (res.IsSuccess) this.items = res.Data ?? [];
    });
  }

  open(n: NotificationItem): void {
    if (n.IsRead) return;
    this.api.markRead(n.UniqueId).subscribe(() => {
      n.IsRead = true;
      this.unread = Math.max(0, this.unread - 1);
    });
  }

  markAll(): void {
    this.api.markAllRead().subscribe(() => {
      this.items.forEach((n) => (n.IsRead = true));
      this.unread = 0;
    });
  }
}
