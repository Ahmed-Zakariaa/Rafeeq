import { Routes } from '@angular/router';
import { adminGuard, authGuard, guestGuard, permissionGuard, verifiedGuard } from './guards/auth.guard';
import { AuthLayoutComponent } from './layouts/auth/auth-layout.component';
import { MainLayoutComponent } from './layouts/main/main-layout.component';
import { AdminLayoutComponent } from './layouts/admin/admin-layout.component';

export const routes: Routes = [
  {
    path: 'auth',
    component: AuthLayoutComponent,
    canActivate: [guestGuard],
    children: [
      {
        path: 'login',
        loadComponent: () =>
          import('./modules/auth/pages/login/login.component').then((m) => m.LoginComponent),
      },
      {
        path: 'register',
        loadComponent: () =>
          import('./modules/auth/pages/register/register.component').then((m) => m.RegisterComponent),
      },
      {
        path: 'forgot-password',
        loadComponent: () =>
          import('./modules/auth/pages/forgot-password/forgot-password.component').then(
            (m) => m.ForgotPasswordComponent
          ),
      },
      {
        path: 'reset-password',
        loadComponent: () =>
          import('./modules/auth/pages/reset-password/reset-password.component').then(
            (m) => m.ResetPasswordComponent
          ),
      },
      {
        path: 'set-password',
        data: { mode: 'set' },
        loadComponent: () =>
          import('./modules/auth/pages/reset-password/reset-password.component').then(
            (m) => m.ResetPasswordComponent
          ),
      },
      { path: '', pathMatch: 'full', redirectTo: 'login' },
    ],
  },

  // Email verification (logged-in, standalone full-screen page).
  {
    path: 'verify-email',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./modules/auth/pages/verify-email/verify-email.component').then((m) => m.VerifyEmailComponent),
  },

  // Admin portal (sidebar shell) — must come before the catch-all main layout.
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [authGuard, verifiedGuard, adminGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./modules/admin/pages/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'drivers',
        data: { role: 'Driver', titleKey: 'admin.nav.drivers' },
        loadComponent: () =>
          import('./modules/admin/pages/users-list/users-list.component').then((m) => m.UsersListComponent),
      },
      {
        path: 'passengers',
        data: { role: 'Passenger', titleKey: 'admin.nav.passengers' },
        loadComponent: () =>
          import('./modules/admin/pages/users-list/users-list.component').then((m) => m.UsersListComponent),
      },
      {
        path: 'users',
        data: { role: null, titleKey: 'admin.nav.users' },
        loadComponent: () =>
          import('./modules/admin/pages/users-list/users-list.component').then((m) => m.UsersListComponent),
      },
      {
        path: 'admins',
        data: { role: 'Admin', titleKey: 'admin.nav.admins' },
        loadComponent: () =>
          import('./modules/admin/pages/users-list/users-list.component').then((m) => m.UsersListComponent),
      },
      {
        path: 'verification',
        canActivate: [permissionGuard],
        data: { permission: 'Verification.Review' },
        loadComponent: () =>
          import('./modules/admin/pages/verification-review/verification-review.component').then(
            (m) => m.VerificationReviewComponent
          ),
      },
      {
        path: 'reports',
        canActivate: [permissionGuard],
        data: { permission: 'Reports.Handle' },
        loadComponent: () =>
          import('./modules/admin/pages/reports/reports.component').then((m) => m.ReportsComponent),
      },
      {
        path: 'roles',
        canActivate: [permissionGuard],
        data: { permission: 'Admins.Manage' },
        loadComponent: () =>
          import('./modules/admin/pages/roles/roles.component').then((m) => m.RolesComponent),
      },
      {
        path: 'lookups',
        canActivate: [permissionGuard],
        data: { permission: 'Lookups.Manage' },
        loadComponent: () =>
          import('./modules/admin/pages/lookups/lookups.component').then((m) => m.LookupsComponent),
      },
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
    ],
  },

  // Rider/driver app (top-nav shell)
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard, verifiedGuard],
    children: [
      {
        path: 'trips',
        loadComponent: () =>
          import('./modules/trips/pages/trips-search/trips-search.component').then(
            (m) => m.TripsSearchComponent
          ),
      },
      {
        path: 'trips/create',
        canActivate: [permissionGuard],
        data: { permission: 'Trips.Create' },
        loadComponent: () =>
          import('./modules/trips/pages/trips-create/trips-create.component').then(
            (m) => m.TripsCreateComponent
          ),
      },
      {
        path: 'my-trips',
        canActivate: [permissionGuard],
        data: { permission: 'Trips.ManageOwn' },
        loadComponent: () =>
          import('./modules/trips/pages/my-trips/my-trips.component').then((m) => m.MyTripsComponent),
      },
      {
        path: 'vehicles',
        canActivate: [permissionGuard],
        data: { permission: 'Vehicles.ManageOwn' },
        loadComponent: () =>
          import('./modules/vehicles/pages/vehicles/vehicles.component').then((m) => m.VehiclesComponent),
      },
      {
        path: 'bookings',
        canActivate: [permissionGuard],
        data: { permission: 'Bookings.Request' },
        loadComponent: () =>
          import('./modules/bookings/pages/my-bookings/my-bookings.component').then(
            (m) => m.MyBookingsComponent
          ),
      },
      {
        path: 'requests',
        canActivate: [permissionGuard],
        data: { permission: 'Bookings.Respond' },
        loadComponent: () =>
          import('./modules/bookings/pages/incoming/incoming.component').then((m) => m.IncomingComponent),
      },
      {
        path: 'verification',
        canActivate: [permissionGuard],
        data: { permission: 'Verification.Upload' },
        loadComponent: () =>
          import('./modules/verification/pages/verification/verification.component').then(
            (m) => m.VerificationComponent
          ),
      },
      {
        path: 'home',
        loadComponent: () => import('./pages/home/home.component').then((m) => m.HomeComponent),
      },
      { path: '', pathMatch: 'full', redirectTo: 'trips' },
    ],
  },
  { path: '**', redirectTo: '' },
];
