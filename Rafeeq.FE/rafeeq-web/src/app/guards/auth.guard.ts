import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/** Blocks routes for anonymous users; sends them to login. */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isLoggedIn()) return true;
  return router.createUrlTree(['/auth/login']);
};

/** Keeps logged-in users out of the auth pages (login/register), routing them home by account type. */
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.isLoggedIn()) return true;
  return router.parseUrl(auth.defaultRoute());
};

/** Admin portal access: super admin or any Admin-role user. */
export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isLoggedIn() && auth.isAdmin()) return true;
  return router.createUrlTree(['/trips']);
};

/** Email verification is mandatory: a logged-in but unverified user is locked onto the verify page. */
export const verifiedGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isLoggedIn() && !auth.isEmailVerified()) return router.parseUrl('/verify-email');
  return true;
};

/** Route must declare `data: { permission: 'Trips.Create' }`. Driven by JWT claims. */
export const permissionGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const required = route.data?.['permission'] as string | undefined;
  if (!required || auth.hasPermission(required)) return true;
  return router.createUrlTree(['/trips']);
};
