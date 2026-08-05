import {
  Component,
  HostListener,
  ElementRef,
  inject,
  ChangeDetectionStrategy,
  input,
  output,
  signal,
  computed
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface SelectOption {
  value: string;
  label: string;
}

@Component({
  selector: 'app-custom-select',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cs-wrapper" [class.cs-open]="isOpen()" [class.cs-loading]="loading()" [class.cs-disabled]="disabled()" [class.cs-drop-up]="dropUp()">
      <!-- Trigger -->
      <button
        type="button"
        class="cs-trigger"
        (click)="toggle()"
        [disabled]="disabled() || loading()"
        [attr.aria-expanded]="isOpen()"
        [attr.aria-label]="placeholder()"
      >
        @if (loading()) {
          <span class="cs-spinner">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round">
              <path d="M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0"/>
            </svg>
          </span>
          <span class="cs-loading-text">Carregando...</span>
        } @else {
          <span class="cs-value" [class.cs-placeholder]="!selectedLabel()">
            {{ selectedLabel() || placeholder() }}
          </span>
          <span class="cs-arrow">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="6 9 12 15 18 9"/>
            </svg>
          </span>
        }
      </button>

      <!-- Dropdown Panel -->
      @if (isOpen() && !loading()) {
        <div class="cs-dropdown fade-in-down">
          <!-- Search (when many options) -->
          @if (searchable() && options().length > 6) {
            <div class="cs-search-wrapper">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
              </svg>
              <input
                #searchInput
                type="text"
                class="cs-search"
                placeholder="Filtrar..."
                [ngModel]="searchTerm()"
                (ngModelChange)="searchTerm.set($event)"
                (click)="$event.stopPropagation()"
              />
            </div>
          }

          <div class="cs-list">
            <!-- Placeholder option -->
            <div
              class="cs-option cs-option-placeholder"
              [class.cs-selected]="!value()"
              (click)="select('', '')"
            >
              {{ placeholder() }}
            </div>

            @for (opt of filteredOptions(); track opt.value) {
              <div
                class="cs-option"
                [class.cs-selected]="opt.value === value()"
                (click)="select(opt.value, opt.label)"
              >
                @if (opt.value === value()) {
                  <svg class="cs-check" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round">
                    <polyline points="20 6 9 17 4 12"/>
                  </svg>
                }
                <span>{{ opt.label }}</span>
              </div>
            } @empty {
              <div class="cs-empty">Nenhuma opção encontrada</div>
            }
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .cs-wrapper {
      position: relative;
      width: 100%;
    }

    .cs-trigger {
      width: 100%;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 0.5rem;
      background: rgba(18, 18, 21, 0.6);
      border: 1px solid var(--border-subtle);
      border-radius: 8px;
      padding: 0.75rem 1rem;
      color: var(--text-main);
      font-size: 0.95rem;
      font-family: var(--font-body);
      cursor: pointer;
      transition: all 0.2s ease;
      text-align: left;
    }

    .cs-trigger:hover:not(:disabled) {
      border-color: var(--border-focus);
      background: rgba(28, 28, 33, 0.9);
    }

    .cs-open .cs-trigger {
      border-color: var(--primary);
      box-shadow: 0 0 0 3px var(--border-focus);
      background: rgba(28, 28, 33, 0.95);
    }

    .cs-trigger:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .cs-value {
      flex: 1;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .cs-placeholder {
      color: var(--text-subtle);
    }

    .cs-arrow {
      color: var(--text-subtle);
      transition: transform 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
      flex-shrink: 0;
      line-height: 0;
    }

    .cs-open .cs-arrow {
      transform: rotate(180deg);
      color: var(--primary);
    }

    .cs-spinner svg {
      animation: spin 0.8s linear infinite;
      color: var(--primary);
    }

    .cs-loading-text {
      color: var(--text-subtle);
      font-size: 0.9rem;
      flex: 1;
    }

    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }

    /* Dropdown */
    .cs-dropdown {
      position: absolute;
      top: calc(100% + 6px);
      left: 0;
      right: 0;
      background: var(--bg-surface);
      border: 1px solid var(--border-subtle);
      border-radius: 10px;
      box-shadow: var(--shadow-lg);
      z-index: 9999;
      overflow: hidden;
    }

    @keyframes fadeInDown {
      from { opacity: 0; transform: translateY(-8px) scale(0.98); }
      to   { opacity: 1; transform: translateY(0) scale(1); }
    }

    @keyframes fadeInUp {
      from { opacity: 0; transform: translateY(8px) scale(0.98); }
      to   { opacity: 1; transform: translateY(0) scale(1); }
    }

    .fade-in-down {
      animation: fadeInDown 0.2s cubic-bezier(0.16, 1, 0.3, 1);
    }
    
    .cs-drop-up .cs-dropdown {
      top: auto;
      bottom: calc(100% + 6px);
      box-shadow: var(--shadow-lg);
    }
    
    .cs-drop-up .fade-in-down {
      animation: fadeInUp 0.2s cubic-bezier(0.16, 1, 0.3, 1);
    }

    /* Search */
    .cs-search-wrapper {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.6rem 0.85rem;
      border-bottom: 1px solid var(--border-color);
      color: var(--text-subtle);
    }

    .cs-search {
      flex: 1;
      background: transparent;
      border: none;
      outline: none;
      color: var(--text-main);
      font-size: 0.875rem;
      font-family: var(--font-body);
    }

    .cs-search::placeholder { color: var(--text-subtle); }

    /* List */
    .cs-list {
      max-height: 240px;
      overflow-y: auto;
      scrollbar-width: thin;
      scrollbar-color: #334155 transparent;
    }

    .cs-list::-webkit-scrollbar { width: 4px; }
    .cs-list::-webkit-scrollbar-track { background: transparent; }
    .cs-list::-webkit-scrollbar-thumb { background: #334155; border-radius: 2px; }

    /* Option */
    .cs-option {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.7rem 1rem;
      color: var(--text-muted);
      font-size: 0.9rem;
      cursor: pointer;
      transition: background 0.15s ease, color 0.15s ease;
      border-bottom: 1px solid var(--border-color);
    }

    .cs-option:last-child { border-bottom: none; }

    .cs-option:hover {
      background: var(--bg-surface-hover);
      color: var(--text-main);
    }

    .cs-option-placeholder {
      color: var(--text-subtle);
      font-style: italic;
    }

    .cs-option.cs-selected {
      background: var(--primary-subtle);
      color: var(--primary);
      font-weight: 600;
    }

    .cs-check {
      flex-shrink: 0;
      color: var(--primary);
    }

    .cs-empty {
      padding: 1.25rem;
      text-align: center;
      color: var(--text-subtle);
      font-size: 0.875rem;
    }
  `]
})
export class CustomSelectComponent {
  options = input<SelectOption[]>([]);
  value = input<string>('');
  placeholder = input<string>('-- Selecione --');
  loading = input<boolean>(false);
  disabled = input<boolean>(false);
  searchable = input<boolean>(true);
  dropUp = input<boolean>(false);

  valueChange = output<string>();

  isOpen = signal(false);
  searchTerm = signal('');

  selectedLabel = computed(() => {
    const found = this.options().find(o => o.value === this.value());
    return found ? found.label : '';
  });

  filteredOptions = computed(() => {
    const term = this.searchTerm().toLowerCase();
    return this.options().filter(o => o.label.toLowerCase().includes(term));
  });

  private el = inject(ElementRef);

  toggle() {
    if (this.disabled() || this.loading()) return;
    this.isOpen.update(v => !v);
    if (this.isOpen()) {
      this.searchTerm.set('');
    }
  }

  select(val: string, label: string) {
    this.isOpen.set(false);
    this.valueChange.emit(val);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.el.nativeElement.contains(event.target)) {
      this.isOpen.set(false);
    }
  }

  @HostListener('document:keydown.escape')
  onEscape() {
    this.isOpen.set(false);
  }
}
