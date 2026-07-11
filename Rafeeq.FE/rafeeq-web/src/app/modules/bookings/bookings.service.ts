import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { ApiResponse } from '../../core/api-response.model';
import { BookingRequest, BookingRespond, BookingResult } from './models/booking.models';

@Injectable({ providedIn: 'root' })
export class BookingsService {
  constructor(private api: ApiService) {}

  request(req: BookingRequest): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('Bookings/Request', req);
  }

  respond(req: BookingRespond): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('Bookings/Respond', req);
  }

  cancel(uniqueId: string): Observable<ApiResponse<boolean>> {
    return this.api.delete<boolean>(`Bookings/Cancel/${uniqueId}`);
  }

  getMine(): Observable<ApiResponse<BookingResult[]>> {
    return this.api.get<BookingResult[]>('Bookings/Mine');
  }

  getIncoming(): Observable<ApiResponse<BookingResult[]>> {
    return this.api.get<BookingResult[]>('Bookings/Incoming');
  }
}
