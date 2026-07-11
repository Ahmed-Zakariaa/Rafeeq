import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { ApiResponse } from '../../core/api-response.model';

export interface RateRequest {
  bookingUniqueId: string;
  stars: number;
  comment?: string | null;
}

@Injectable({ providedIn: 'root' })
export class RatingsService {
  constructor(private api: ApiService) {}

  rate(req: RateRequest): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('Ratings/Rate', req);
  }
}
