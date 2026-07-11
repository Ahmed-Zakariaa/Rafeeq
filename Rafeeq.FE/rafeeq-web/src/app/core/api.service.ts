import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from './api-response.model';

/**
 * Generic HTTP layer (same role as ECM's services/apis/api.service.ts).
 * Every domain service calls these; URLs are relative to environment.API_URL,
 * e.g. api.post('Trips/Search', body) → POST http://localhost:5200/api/Trips/Search
 */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly base = environment.API_URL;

  constructor(private http: HttpClient) {}

  get<T>(url: string, params?: Record<string, unknown>): Observable<ApiResponse<T>> {
    return this.http.get<ApiResponse<T>>(`${this.base}/${url}`, { params: this.toParams(params) });
  }

  post<T>(url: string, body: unknown): Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(`${this.base}/${url}`, body);
  }

  put<T>(url: string, body: unknown): Observable<ApiResponse<T>> {
    return this.http.put<ApiResponse<T>>(`${this.base}/${url}`, body);
  }

  delete<T>(url: string): Observable<ApiResponse<T>> {
    return this.http.delete<ApiResponse<T>>(`${this.base}/${url}`);
  }

  /** Multipart upload (FormData) — sets no Content-Type so the browser adds the boundary. */
  postForm<T>(url: string, form: FormData): Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(`${this.base}/${url}`, form);
  }

  /** Fetches a binary file (auth header added by the interceptor). */
  getBlob(url: string): Observable<Blob> {
    return this.http.get(`${this.base}/${url}`, { responseType: 'blob' });
  }

  private toParams(params?: Record<string, unknown>): HttpParams {
    let p = new HttpParams();
    if (params) {
      for (const key of Object.keys(params)) {
        const value = params[key];
        if (value !== null && value !== undefined) p = p.set(key, String(value));
      }
    }
    return p;
  }
}
