import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { ApiResponse } from '../../core/api-response.model';

export interface NotificationItem {
  UniqueId: string;
  Type: string;
  TitleKey: string;
  Body: string | null;
  IsRead: boolean;
  CreatedDate: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  constructor(private api: ApiService) {}

  getMine(): Observable<ApiResponse<NotificationItem[]>> {
    return this.api.get<NotificationItem[]>('Notifications/Mine');
  }

  unreadCount(): Observable<ApiResponse<number>> {
    return this.api.get<number>('Notifications/UnreadCount');
  }

  markRead(uniqueId: string): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`Notifications/MarkRead/${uniqueId}`, {});
  }

  markAllRead(): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('Notifications/MarkAllRead', {});
  }
}
