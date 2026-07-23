import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomSelectComponent, SelectOption } from '../custom-select/custom-select.component';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule, FormsModule, CustomSelectComponent],
  template: `
    <div class="pagination-wrapper">
      <div class="pagination-size">
        <label class="pagination-label">Exibir</label>
        <app-custom-select
          [options]="pageSizeSelectOptions"
          [value]="pageSize.toString()"
          [searchable]="false"
          [dropUp]="true"
          placeholder="Itens"
          style="width: 140px; display: inline-block;"
          (valueChange)="onPageSizeSelect($event)"
        ></app-custom-select>
      </div>

      <div class="pagination-info">
        <span>Página <strong>{{ pageNumber }}</strong> / <strong>{{ totalPages || 1 }}</strong></span>
        <span class="total-badge">{{ totalItems }} registro(s)</span>
      </div>

      <div class="pagination-controls">
        <button 
          (click)="goToPage(pageNumber - 1)" 
          [disabled]="pageNumber <= 1" 
          class="btn-page"
          title="Página Anterior"
          aria-label="Página Anterior">
          ‹
        </button>

        <button 
          (click)="goToPage(pageNumber + 1)" 
          [disabled]="pageNumber >= (totalPages || 1)" 
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
      background: rgba(18, 18, 22, 0.85);
      backdrop-filter: blur(16px);
      border: 1px solid rgba(237, 20, 91, 0.25);
      border-radius: 12px;
      margin-top: 1.25rem;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.4);
    }
    .pagination-size {
      display: flex;
      align-items: center;
      gap: 0.65rem;
    }
    .pagination-label {
      font-size: 0.75rem;
      color: #A1A1AA;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.06em;
    }
    .pagination-info {
      font-size: 0.85rem;
      color: #E2E8F0;
      display: flex;
      align-items: center;
      gap: 0.65rem;
    }
    .pagination-info strong {
      color: #ED145B;
      font-weight: 700;
    }
    .total-badge {
      font-size: 0.75rem;
      color: #A1A1AA;
      background: rgba(255, 255, 255, 0.08);
      border: 1px solid rgba(255, 255, 255, 0.1);
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
      background: rgba(255, 255, 255, 0.05);
      border: 1px solid rgba(255, 255, 255, 0.15);
      color: #F8FAFC;
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
      background: #ED145B;
      border-color: #ED145B;
      color: #ffffff;
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(237, 20, 91, 0.4);
    }
    .btn-page:disabled {
      opacity: 0.3;
      cursor: not-allowed;
    }
  `]
})
export class PaginationComponent implements OnChanges {
  @Input() pageNumber = 1;
  @Input() pageSize = 10;
  @Input() totalItems = 0;
  @Input() totalPages = 1;
  @Input() pageSizeOptions: number[] = [5, 10, 20, 50];

  pageSizeSelectOptions: SelectOption[] = [
    { value: '5', label: '5' },
    { value: '10', label: '10' },
    { value: '20', label: '20' },
    { value: '50', label: '50' }
  ];

  ngOnChanges(changes: SimpleChanges) {
    if (changes['pageSizeOptions'] && this.pageSizeOptions) {
      this.pageSizeSelectOptions = this.pageSizeOptions.map(opt => ({
        value: opt.toString(),
        label: `${opt}`
      }));
    }
  }

  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();

  goToPage(page: number) {
    if (page >= 1 && page <= (this.totalPages || 1)) {
      this.pageChange.emit(page);
    }
  }

  onPageSizeSelect(size: string | number) {
    this.pageSizeChange.emit(Number(size));
  }
}
