import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { MultiSelectModule } from 'primeng/multiselect';
import { MessageService } from 'primeng/api';

import { PermissionMatrixComponent } from '../../components/permission-matrix/permission-matrix.component';
import { AdminService } from '../../admin.service';
import { AdminRolesService } from '../../admin-roles.service';
import { AccountStatus, CreateAdminRequest, UserListItem } from '../../models/admin.models';
import { AuthService } from '../../../../services/auth.service';
import { Gender } from '../../../auth/models/auth.models';
import { LookupsService } from '../../../../services/lookups.service';
import { LanguageService } from '../../../../services/language.service';
import { CountryLookup, countryName } from '../../../../core/lookups';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    ButtonModule,
    InputTextModule,
    DropdownModule,
    TableModule,
    TagModule,
    DialogModule,
    PaginatorModule,
    MultiSelectModule,
    PermissionMatrixComponent,
  ],
  templateUrl: './users-list.component.html',
})
export class UsersListComponent implements OnInit {
  readonly Status = AccountStatus;

  role: string | null = null;
  titleKey = 'admin.nav.users';
  isAdminsView = false;

  users: UserListItem[] = [];
  total = 0;
  pageNumber = 1;
  pageSize = 10;
  loading = false;

  search = '';
  statusFilter: AccountStatus | null = null;
  statusOptions: { label: string; value: AccountStatus | null }[] = [];

  canManageUsers = this.auth.hasPermission('Users.Manage');
  canManageAdmins = this.auth.hasPermission('Admins.Manage');

  catalog: string[] = [];
  countries: CountryLookup[] = [];
  countryOptions: { label: string; value: number }[] = [];
  genderOptions: { label: string; value: number }[] = [];

  // Create-admin dialog
  createVisible = false;
  saving = false;
  newAdminPerms: string[] = [];
  createForm = this.fb.nonNullable.group({
    fullName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', [Validators.required]],
    gender: [Gender.Male],
    countryId: [null as number | null, [Validators.required]],
  });

  // Permissions dialog
  permsVisible = false;
  permsTarget: UserListItem | null = null;
  permsSelected: string[] = [];
  permsSaving = false;
  roleOptions: { label: string; value: string }[] = [];
  permsRoles: string[] = [];

  constructor(
    private fb: FormBuilder,
    private admin: AdminService,
    private rolesApi: AdminRolesService,
    private auth: AuthService,
    private lookups: LookupsService,
    private route: ActivatedRoute,
    private toast: MessageService,
    private t: TranslateService,
    public lang: LanguageService
  ) {}

  ngOnInit(): void {
    const data = this.route.snapshot.data;
    this.role = (data['role'] as string) ?? null;
    this.titleKey = (data['titleKey'] as string) ?? 'admin.nav.users';
    this.isAdminsView = this.role === 'Admin';

    this.buildOptions();
    this.t.onLangChange.subscribe(() => this.buildOptions());
    this.lookups.countries().subscribe((cs) => {
      this.countries = cs;
      this.buildOptions();
    });

    if (this.canManageAdmins) {
      this.loadCatalog();
      this.loadRoles();
    }
    this.load();
  }

  private loadRoles(): void {
    this.rolesApi.list().subscribe((res) => {
      if (res.IsSuccess)
        this.roleOptions = (res.Data ?? []).map((r) => ({ label: r.Name, value: r.UniqueId }));
    });
  }

  private buildOptions(): void {
    this.statusOptions = [
      { label: this.t.instant('admin.status.all'), value: null },
      { label: this.t.instant('admin.status.Active'), value: AccountStatus.Active },
      { label: this.t.instant('admin.status.Suspended'), value: AccountStatus.Suspended },
      { label: this.t.instant('admin.status.Banned'), value: AccountStatus.Banned },
    ];
    this.countryOptions = this.countries.map((c) => ({ label: countryName(c, this.lang.current()), value: c.Id }));
    this.genderOptions = [
      { label: this.t.instant('fields.male'), value: Gender.Male },
      { label: this.t.instant('fields.female'), value: Gender.Female },
    ];
  }

  private loadCatalog(): void {
    this.admin.getPermissionCatalog().subscribe((res) => {
      if (res.IsSuccess) this.catalog = res.Data ?? [];
    });
  }

  load(resetPage = true): void {
    if (resetPage) this.pageNumber = 1;
    this.loading = true;
    this.admin
      .listUsers({
        filterModel: { role: this.role, search: this.search || null, accountStatus: this.statusFilter },
        pageNumber: this.pageNumber,
        pageSize: this.pageSize,
      })
      .subscribe({
        next: (res) => {
          this.loading = false;
          if (res.IsSuccess) {
            this.users = res.Data ?? [];
            this.total = res.Total;
          }
        },
        error: () => (this.loading = false),
      });
  }

  onPage(e: PaginatorState): void {
    this.pageNumber = (e.page ?? 0) + 1;
    this.pageSize = e.rows ?? this.pageSize;
    this.load(false);
  }

  statusSeverity(s: string): 'success' | 'warning' | 'danger' {
    return s === 'Active' ? 'success' : s === 'Suspended' ? 'warning' : 'danger';
  }

  setStatus(user: UserListItem, status: AccountStatus): void {
    this.admin.setUserStatus(user.UniqueId, status).subscribe({
      next: (res) => {
        if (res.IsSuccess) {
          this.toast.add({ severity: 'success', summary: this.t.instant('admin.statusUpdated') });
          this.load(false);
        } else {
          this.toast.add({ severity: 'error', summary: res.Message || this.t.instant('common.error') });
        }
      },
      error: (err) =>
        this.toast.add({ severity: 'error', summary: err?.error?.Message || this.t.instant('common.error') }),
    });
  }

  // ── Create admin ──────────────────────────────────────────────
  openCreate(): void {
    this.createForm.reset({ gender: Gender.Male, countryId: this.countries[0]?.Id ?? null });
    this.newAdminPerms = [];
    this.createVisible = true;
  }

  submitCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }
    this.saving = true;
    const raw = this.createForm.getRawValue();
    const req: CreateAdminRequest = { ...raw, countryId: raw.countryId!, permissions: this.newAdminPerms };
    this.admin.createAdmin(req).subscribe({
      next: (res) => {
        this.saving = false;
        if (res.IsSuccess) {
          this.toast.add({ severity: 'success', summary: this.t.instant('admin.adminInvited') });
          this.createVisible = false;
          this.load();
        } else {
          this.toast.add({ severity: 'error', summary: res.Message || this.t.instant('common.error') });
        }
      },
      error: (err) => {
        this.saving = false;
        this.toast.add({ severity: 'error', summary: err?.error?.Message || this.t.instant('common.error') });
      },
    });
  }

  // ── Manage permissions ────────────────────────────────────────
  openPerms(user: UserListItem): void {
    this.permsTarget = user;
    this.permsSelected = [];
    this.permsRoles = [];
    this.permsVisible = true;
    this.admin.getUserPermissions(user.UniqueId).subscribe((res) => {
      if (res.IsSuccess) this.permsSelected = res.Data ?? [];
    });
    this.rolesApi.getUserRoles(user.UniqueId).subscribe((res) => {
      if (res.IsSuccess) this.permsRoles = res.Data ?? [];
    });
  }

  // Save both direct permissions and assigned roles for the target admin.
  savePerms(): void {
    if (!this.permsTarget) return;
    const id = this.permsTarget.UniqueId;
    this.permsSaving = true;

    this.admin.assignPermissions(id, this.permsSelected).subscribe({
      next: (res) => {
        if (!res.IsSuccess) {
          this.permsSaving = false;
          this.toast.add({ severity: 'error', summary: res.Message || this.t.instant('common.error') });
          return;
        }
        this.rolesApi.assignUserRoles(id, this.permsRoles).subscribe({
          next: (r2) => {
            this.permsSaving = false;
            if (r2.IsSuccess) {
              this.toast.add({ severity: 'success', summary: this.t.instant('admin.permsUpdated') });
              this.permsVisible = false;
            } else {
              this.toast.add({ severity: 'error', summary: r2.Message || this.t.instant('common.error') });
            }
          },
          error: (err) => this.failPerms(err),
        });
      },
      error: (err) => this.failPerms(err),
    });
  }

  private failPerms(err: unknown): void {
    this.permsSaving = false;
    const message = (err as { error?: { Message?: string } })?.error?.Message;
    this.toast.add({ severity: 'error', summary: message || this.t.instant('common.error') });
  }
}
