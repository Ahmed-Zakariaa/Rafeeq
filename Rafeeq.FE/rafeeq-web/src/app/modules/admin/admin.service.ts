import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { ApiResponse, QueryModel } from '../../core/api-response.model';
import {
  AccountStatus,
  CreateAdminRequest,
  DashboardData,
  UserListFilter,
  UserListItem,
} from './models/admin.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  constructor(private api: ApiService) {}

  listUsers(query: QueryModel<UserListFilter>): Observable<ApiResponse<UserListItem[]>> {
    return this.api.post<UserListItem[]>('Admin/Users', query);
  }

  createAdmin(req: CreateAdminRequest): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('Admin/Admins', req);
  }

  getUserPermissions(uniqueId: string): Observable<ApiResponse<string[]>> {
    return this.api.get<string[]>(`Admin/Users/${uniqueId}/Permissions`);
  }

  assignPermissions(uniqueId: string, permissions: string[]): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`Admin/Users/${uniqueId}/Permissions`, { permissions });
  }

  setUserStatus(uniqueId: string, status: AccountStatus): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`Admin/Users/${uniqueId}/Status`, { status });
  }

  getDashboard(): Observable<ApiResponse<DashboardData>> {
    return this.api.get<DashboardData>('Admin/Dashboard');
  }

  getPermissionCatalog(): Observable<ApiResponse<string[]>> {
    return this.api.get<string[]>('Admin/Permissions/Catalog');
  }
}
