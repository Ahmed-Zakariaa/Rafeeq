import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { ApiResponse } from '../../core/api-response.model';
import { VehicleCreateRequest, VehicleResult } from './models/vehicle.models';

@Injectable({ providedIn: 'root' })
export class VehiclesService {
  constructor(private api: ApiService) {}

  getMine(): Observable<ApiResponse<VehicleResult[]>> {
    return this.api.get<VehicleResult[]>('Vehicles/Mine');
  }

  create(req: VehicleCreateRequest): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('Vehicles/Create', req);
  }
}
