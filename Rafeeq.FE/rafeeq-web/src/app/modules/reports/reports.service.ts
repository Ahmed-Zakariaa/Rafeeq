import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { ApiResponse } from '../../core/api-response.model';
import {
  ReportCreateRequest,
  ReportHandleRequest,
  ReportListItem,
  ReportStatus,
} from './models/report.models';

@Injectable({ providedIn: 'root' })
export class ReportsService {
  constructor(private api: ApiService) {}

  create(req: ReportCreateRequest): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('Reports/Create', req);
  }

  getAll(status?: ReportStatus | null): Observable<ApiResponse<ReportListItem[]>> {
    return this.api.get<ReportListItem[]>('Reports/All', status != null ? { status } : undefined);
  }

  handle(req: ReportHandleRequest): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('Reports/Handle', req);
  }
}
