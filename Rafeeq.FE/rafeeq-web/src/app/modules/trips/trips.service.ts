import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { ApiResponse, QueryModel } from '../../core/api-response.model';
import { TripCreateRequest, TripResult, TripSearchFilter } from './models/trip.models';

@Injectable({ providedIn: 'root' })
export class TripsService {
  constructor(private api: ApiService) {}

  search(query: QueryModel<TripSearchFilter>): Observable<ApiResponse<TripResult[]>> {
    return this.api.post<TripResult[]>('Trips/Search', query);
  }

  create(req: TripCreateRequest): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('Trips/Create', req);
  }

  getMine(): Observable<ApiResponse<TripResult[]>> {
    return this.api.get<TripResult[]>('Trips/Mine');
  }

  complete(uniqueId: string): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`Trips/Complete/${uniqueId}`, {});
  }

  cancel(uniqueId: string): Observable<ApiResponse<boolean>> {
    return this.api.delete<boolean>(`Trips/Cancel/${uniqueId}`);
  }
}
