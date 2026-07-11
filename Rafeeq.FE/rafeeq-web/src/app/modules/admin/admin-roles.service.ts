import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { ApiResponse } from '../../core/api-response.model';

export interface AdminRole {
  UniqueId: string;
  Name: string;
  Permissions: string[];
}

export interface AdminRoleSave {
  name: string;
  permissions: string[];
}

@Injectable({ providedIn: 'root' })
export class AdminRolesService {
  constructor(private api: ApiService) {}

  list(): Observable<ApiResponse<AdminRole[]>> {
    return this.api.get<AdminRole[]>('AdminRoles');
  }

  create(req: AdminRoleSave): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('AdminRoles', req);
  }

  update(uniqueId: string, req: AdminRoleSave): Observable<ApiResponse<boolean>> {
    return this.api.put<boolean>(`AdminRoles/${uniqueId}`, req);
  }

  remove(uniqueId: string): Observable<ApiResponse<boolean>> {
    return this.api.delete<boolean>(`AdminRoles/${uniqueId}`);
  }

  getUserRoles(userUniqueId: string): Observable<ApiResponse<string[]>> {
    return this.api.get<string[]>(`AdminRoles/User/${userUniqueId}`);
  }

  assignUserRoles(userUniqueId: string, roleUniqueIds: string[]): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`AdminRoles/User/${userUniqueId}`, { roleUniqueIds });
  }
}
