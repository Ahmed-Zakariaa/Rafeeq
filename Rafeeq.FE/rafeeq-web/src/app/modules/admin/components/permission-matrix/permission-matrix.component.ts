import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { CheckboxModule } from 'primeng/checkbox';

interface PermGroup {
  resource: string;
  perms: string[]; // full "Resource.Action" keys for this module
}

/**
 * Renders the permission catalog as a dynamic module × action matrix (one row per module,
 * a checkbox per available action). Driven entirely by the catalog, so new BE permissions
 * appear automatically. Value is the flat list of granted "Resource.Action" keys.
 */
@Component({
  selector: 'app-permission-matrix',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, CheckboxModule],
  template: `
    <div class="border border-line rounded-lg divide-y divide-line">
      <div
        *ngFor="let g of groups"
        class="flex items-center justify-between gap-3 px-3 py-2.5"
      >
        <span class="text-sm font-medium text-ink">{{ 'perm.resource.' + g.resource | translate }}</span>
        <div class="flex items-center gap-4 flex-wrap justify-end">
          <label
            *ngFor="let p of g.perms"
            class="flex items-center gap-1.5 text-sm text-muted cursor-pointer"
          >
            <p-checkbox
              [binary]="true"
              [ngModel]="isChecked(p)"
              (ngModelChange)="toggle(p, $event)"
              [inputId]="p"
            ></p-checkbox>
            {{ 'perm.action.' + action(p) | translate }}
          </label>
        </div>
      </div>
    </div>
  `,
})
export class PermissionMatrixComponent implements OnChanges {
  @Input() catalog: string[] = [];
  @Input() selected: string[] = [];
  @Output() selectedChange = new EventEmitter<string[]>();

  groups: PermGroup[] = [];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['catalog']) this.buildGroups();
  }

  private buildGroups(): void {
    const map = new Map<string, string[]>();
    for (const perm of this.catalog) {
      const resource = perm.split('.')[0];
      (map.get(resource) ?? map.set(resource, []).get(resource)!).push(perm);
    }
    this.groups = [...map.entries()].map(([resource, perms]) => ({ resource, perms }));
  }

  action(perm: string): string {
    return perm.split('.')[1] ?? perm;
  }

  isChecked(perm: string): boolean {
    return this.selected.includes(perm);
  }

  toggle(perm: string, checked: boolean): void {
    const set = new Set(this.selected);
    if (checked) set.add(perm);
    else set.delete(perm);
    this.selected = [...set];
    this.selectedChange.emit(this.selected);
  }
}
