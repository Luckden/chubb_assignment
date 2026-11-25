import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatTableModule } from '@angular/material/table';

export interface BreakdownRow {
  label: string;
  count: number | string;
  amount: number | string;
}

/** One exposure breakdown (by status, type or age): claims and claimed amount per row. */
@Component({
  selector: 'app-breakdown-table',
  imports: [MatTableModule, DecimalPipe],
  template: `
    <h3 class="mt-0 mb-2 text-base font-medium">{{ title() }}</h3>
    <table mat-table [dataSource]="rows()" [attr.aria-label]="'Claimed exposure ' + title()">
      <ng-container matColumnDef="label">
        <th mat-header-cell *matHeaderCellDef scope="col">{{ labelHeader() }}</th>
        <td mat-cell *matCellDef="let row">{{ row.label }}</td>
      </ng-container>
      <ng-container matColumnDef="count">
        <th mat-header-cell *matHeaderCellDef scope="col" class="text-right">Claims</th>
        <td mat-cell *matCellDef="let row" class="text-right">{{ row.count }}</td>
      </ng-container>
      <ng-container matColumnDef="amount">
        <th mat-header-cell *matHeaderCellDef scope="col" class="text-right">Claimed</th>
        <td mat-cell *matCellDef="let row" class="text-right">
          {{ row.amount | number: '1.2-2' }}
        </td>
      </ng-container>
      <tr mat-header-row *matHeaderRowDef="columns"></tr>
      <tr mat-row *matRowDef="let row; columns: columns"></tr>
    </table>
  `,
  host: { class: 'block overflow-x-auto rounded-xl border border-outline-variant p-4' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BreakdownTable {
  readonly title = input.required<string>();
  readonly labelHeader = input.required<string>();
  readonly rows = input.required<BreakdownRow[]>();

  protected readonly columns = ['label', 'count', 'amount'];
}
