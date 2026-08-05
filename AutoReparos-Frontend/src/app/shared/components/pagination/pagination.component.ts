import { Component, ChangeDetectionStrategy, input, output, computed } from '@angular/core';
import { CustomSelectComponent, SelectOption } from '../custom-select/custom-select.component';

@Component({
  selector: 'app-pagination',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CustomSelectComponent],
  template: `
    <div class="pagination-wrapper">
      <div class="pagination-size">
        <label class="pagination-label">Exibir</label>
        <app-custom-select
          [options]="pageSizeSelectOptions()"
          [value]="pageSize().toString()"
          [searchable]="false"
          [dropUp]="true"
          placeholder="Itens"
          style="width: 140px; display: inline-block;"
          (valueChange)="onPageSizeSelect($event)"
        ></app-custom-select>
      </div>

      <div class="pagination-info">
        <span>Página <strong>{{ pageNumber() }}</strong> / <strong>{{ totalPages() || 1 }}</strong></span>
        <span class="total-badge">{{ totalItems() }} registro(s)</span>
      </div>

      <div class="pagination-controls">
        <button 
          (click)="goToPage(pageNumber() - 1)" 
          [disabled]="pageNumber() <= 1" 
          class="btn-page"
          title="Página Anterior"
          aria-label="Página Anterior">
          ‹
        </button>

        <button 
          (click)="goToPage(pageNumber() + 1)" 
          [disabled]="pageNumber() >= (totalPages() || 1)" 
          class="btn-page"
          title="Próxima Página"
          aria-label="Próxima Página">
          ›
        </button>
      </div>
    </div>
  `,
  styles: [`
    .pagination-wrapper {
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: 1rem;
      padding: 0.85rem 1.25rem;
      background: var(--bg-surface);
      backdrop-filter: var(--glass-blur);
      border: 1px solid var(--border-color);
      border-radius: 12px;
      margin-top: 1.25rem;
      box-shadow: var(--shadow-sm);
    }
    .pagination-size {
      display: flex;
      align-items: center;
      gap: 0.65rem;
    }
    .pagination-label {
      font-size: 0.75rem;
      color: var(--text-muted);
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.06em;
    }
    .pagination-info {
      font-size: 0.85rem;
      color: var(--text-main);
      display: flex;
      align-items: center;
      gap: 0.65rem;
    }
    .pagination-info strong {
      color: var(--primary);
      font-weight: 700;
    }
    .total-badge {
      font-size: 0.75rem;
      color: var(--text-muted);
      background: var(--mono-badge-bg);
      border: 1px solid var(--border-color);
      padding: 0.2rem 0.65rem;
      border-radius: 999px;
      font-weight: 600;
    }
    .pagination-controls {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .btn-page {
      background: rgba(255, 255, 255, 0.04);
      border: 1px solid var(--border-color);
      color: var(--text-main);
      width: 36px;
      height: 36px;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.25rem;
      font-weight: 700;
      cursor: pointer;
      transition: all 0.2s ease;
      line-height: 1;
    }
    .btn-page:hover:not(:disabled) {
      background: var(--primary);
      border-color: var(--primary);
      color: #ffffff;
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(244, 63, 94, 0.25);
    }
    .btn-page:disabled {
      opacity: 0.3;
      cursor: not-allowed;
    }
  `]
})
export class PaginationComponent {
  pageNumber = input(1);
  pageSize = input(10);
  totalItems = input(0);
  totalPages = input(1);
  pageSizeOptions = input<number[]>([5, 10, 20, 50]);

  pageSizeSelectOptions = computed<SelectOption[]>(() =>
    this.pageSizeOptions().map(opt => ({ value: String(opt), label: String(opt) }))
  );

  pageChange = output<number>();
  pageSizeChange = output<number>();

  goToPage(page: number): void {
    if (page >= 1 && page <= (this.totalPages() || 1)) {
      this.pageChange.emit(page);
    }
  }

  onPageSizeSelect(size: string | number): void {
    this.pageSizeChange.emit(Number(size));
  }
}
