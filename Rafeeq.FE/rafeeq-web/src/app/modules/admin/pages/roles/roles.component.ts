import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { MessageService } from 'primeng/api';

import { AdminService } from '../../admin.service';
import { AdminRole, AdminRolesService } from '../../admin-roles.service';
import { PermissionMatrixComponent } from '../../components/permission-matrix/permission-matrix.component';

@Component({
  selector: 'app-admin-roles',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    ButtonModule,
    InputTextModule,
    TableModule,
    DialogModule,
    PermissionMatrixComponent,
  ],
  templateUrl: './roles.component.html',
})
export class RolesComponent implements OnInit {
  roles: AdminRole[] = [];
  catalog: string[] = [];
  loading = false;

  dialogVisible = false;
  editing: AdminRole | null = null;
  name = '';
  selectedPerms: string[] = [];
  saving = false;

  constructor(
    private rolesApi: AdminRolesService,
    private admin: AdminService,
    private toast: MessageService,
    private t: TranslateService
  ) {}

  ngOnInit(): void {
    this.admin.getPermissionCatalog().subscribe((res) => {
      if (res.IsSuccess) this.catalog = res.Data ?? [];
    });
    this.load();
  }

  load(): void {
    this.loading = true;
    this.rolesApi.list().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.IsSuccess) this.roles = res.Data ?? [];
      },
      error: () => (this.loading = false),
    });
  }

  openNew(): void {
    this.editing = null;
    this.name = '';
    this.selectedPerms = [];
    this.dialogVisible = true;
  }

  openEdit(role: AdminRole): void {
    this.editing = role;
    this.name = role.Name;
    this.selectedPerms = [...role.Permissions];
    this.dialogVisible = true;
  }

  save(): void {
    if (!this.name.trim()) {
      this.toast.add({ severity: 'warn', summary: this.t.instant('roles.nameRequired') });
      return;
    }
    this.saving = true;
    const payload = { name: this.name.trim(), permissions: this.selectedPerms };
    const req$ = this.editing
      ? this.rolesApi.update(this.editing.UniqueId, payload)
      : this.rolesApi.create(payload);

    req$.subscribe({
      next: (res) => {
        this.saving = false;
        if (res.IsSuccess) {
          this.toast.add({ severity: 'success', summary: this.t.instant('roles.saved') });
          this.dialogVisible = false;
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

  remove(role: AdminRole): void {
    this.rolesApi.remove(role.UniqueId).subscribe({
      next: (res) => {
        if (res.IsSuccess) {
          this.toast.add({ severity: 'success', summary: this.t.instant('roles.deleted') });
          this.load();
        } else {
          this.toast.add({ severity: 'error', summary: res.Message || this.t.instant('common.error') });
        }
      },
      error: (err) =>
        this.toast.add({ severity: 'error', summary: err?.error?.Message || this.t.instant('common.error') }),
    });
  }
}
