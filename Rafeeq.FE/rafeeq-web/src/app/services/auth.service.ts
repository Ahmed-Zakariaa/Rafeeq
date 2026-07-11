import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { jwtDecode } from 'jwt-decode';

import { ApiService } from '../core/api.service';
import { ApiResponse } from '../core/api-response.model';
import {
  AuthResult,
  CurrentUser,
  ForgotPasswordRequest,
  LoginRequest,
  RegisterRequest,
  ResetPasswordRequest,
} from '../modules/auth/models/auth.models';

const TOKEN_KEY = 'token'; // must match auth.interceptor.ts

// .NET emits role/name claims under these URIs.
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const ID_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _user$ = new BehaviorSubject<CurrentUser | null>(this.decode());
  readonly user$ = this._user$.asObservable();

  constructor(private api: ApiService) {}

  register(req: RegisterRequest): Observable<ApiResponse<AuthResult>> {
    return this.api
      .post<AuthResult>('Auth/Register', req)
      .pipe(tap((res) => res.IsSuccess && this.store(res.Data)));
  }

  login(req: LoginRequest): Observable<ApiResponse<AuthResult>> {
    return this.api
      .post<AuthResult>('Auth/Login', req)
      .pipe(tap((res) => res.IsSuccess && this.store(res.Data)));
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    this._user$.next(null);
  }

  get token(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  get currentUser(): CurrentUser | null {
    return this._user$.value;
  }

  isLoggedIn(): boolean {
    const u = this._user$.value;
    return !!u && u.exp * 1000 > Date.now();
  }

  hasPermission(permission: string): boolean {
    const u = this._user$.value;
    if (!u) return false;
    return u.isSuperAdmin || u.permissions.includes(permission);
  }

  isSuperAdmin(): boolean {
    return this._user$.value?.isSuperAdmin ?? false;
  }

  /** Admin or super admin — used to route into the admin portal. */
  isAdmin(): boolean {
    const u = this._user$.value;
    return !!u && (u.isSuperAdmin || u.roles.includes('Admin'));
  }

  /** Has a rider-side role (can use the trips/vehicles app). */
  isRider(): boolean {
    const u = this._user$.value;
    return !!u && (u.roles.includes('Passenger') || u.roles.includes('Driver'));
  }

  /** Where to land after login, based on the account type. */
  defaultRoute(): string {
    return this.isAdmin() ? '/admin' : '/trips';
  }

  forgotPassword(req: ForgotPasswordRequest): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('Auth/ForgotPassword', req);
  }

  resetPassword(req: ResetPasswordRequest): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('Auth/ResetPassword', req);
  }

  isEmailVerified(): boolean {
    return this._user$.value?.isEmailVerified ?? false;
  }

  /** Verify the emailed OTP; stores the fresh token (emailVerified=true). */
  verifyEmail(code: string): Observable<ApiResponse<AuthResult>> {
    return this.api
      .post<AuthResult>('Auth/VerifyEmail', { code })
      .pipe(tap((res) => res.IsSuccess && this.store(res.Data)));
  }

  resendOtp(): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('Auth/ResendOtp', {});
  }

  private store(data: AuthResult): void {
    localStorage.setItem(TOKEN_KEY, data.Token);
    this._user$.next(this.decode());
  }

  /** Read roles/permissions straight from the JWT (no extra fetch). */
  private decode(): CurrentUser | null {
    const token = localStorage.getItem(TOKEN_KEY);
    if (!token) return null;
    try {
      const p = jwtDecode<Record<string, unknown>>(token);
      return {
        id: String(p[ID_CLAIM] ?? p['sub'] ?? ''),
        fullName: String(p['fullName'] ?? ''),
        roles: toArray(p[ROLE_CLAIM] ?? p['role']),
        permissions: toArray(p['permission']),
        isSuperAdmin: String(p['isSuperAdmin']) === 'true',
        isEmailVerified: String(p['emailVerified']) === 'true',
        exp: Number(p['exp'] ?? 0),
      };
    } catch {
      return null;
    }
  }
}

function toArray(claim: unknown): string[] {
  if (Array.isArray(claim)) return claim.map(String);
  if (claim == null) return [];
  return [String(claim)];
}
