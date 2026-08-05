import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';

@Component({
  selector: 'app-progress-bar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="progress-bar-container" [class]="'layout-' + labelPosition() + ' ' + size()">
      <div class="progress-track">
        <div class="progress-fill" [style.width.%]="percentage()"></div>
      </div>

      @if (showCount()) {
        <span class="progress-label">
          @if (labelPosition() === 'bottom') {
            {{ completed() }} de {{ total() }} {{ itemText() }} concluídos
          } @else {
            {{ completed() }}/{{ total() }}
          }
        </span>
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
    }

    .progress-bar-container {
      display: flex;
      width: 100%;
    }

    /* Layout Lateral (Card do Kanban) */
    .progress-bar-container.layout-right {
      flex-direction: row;
      align-items: center;
      gap: 0.5rem;
    }

    /* Layout Inferior (Drawer e Detalhes) */
    .progress-bar-container.layout-bottom {
      flex-direction: column;
      gap: 0.4rem;
    }

    .progress-track {
      background: rgba(255, 255, 255, 0.12);
      border: 1px solid rgba(255, 255, 255, 0.05);
      border-radius: 999px;
      width: 100%;
      position: relative;
      overflow: hidden;
      display: block;
    }

    .progress-bar-container.sm .progress-track {
      height: 7px;
      min-height: 7px;
    }

    .progress-bar-container.md .progress-track {
      height: 10px;
      min-height: 10px;
    }

    .progress-bar-container.lg .progress-track {
      height: 12px;
      min-height: 12px;
    }

    .progress-fill {
      background: linear-gradient(90deg, var(--primary) 0%, #FB7185 100%);
      height: 100%;
      position: absolute;
      left: 0;
      top: 0;
      border-radius: 999px;
      transition: width 0.4s cubic-bezier(0.16, 1, 0.3, 1);
      box-shadow: 0 0 8px var(--primary-glow);
    }

    .progress-label {
      font-family: var(--font-mono, 'JetBrains Mono', monospace);
      color: var(--text-subtle, #A1A1AA);
      font-size: 0.72rem;
      font-weight: 600;
      white-space: nowrap;
    }

    .layout-bottom .progress-label {
      font-family: inherit;
      font-size: 0.78rem;
      color: var(--text-muted, #A1A1AA);
    }
  `]
})
export class ProgressBarComponent {
  completed = input(0);
  total = input(0);
  showCount = input(true);
  labelPosition = input<'right' | 'bottom'>('right');
  itemText = input('serviços');
  size = input<'sm' | 'md' | 'lg'>('sm');

  percentage = computed(() => {
    const t = this.total();
    const c = this.completed();
    if (!t || t <= 0) return 0;
    return Math.min(100, Math.max(0, (c / t) * 100));
  });
}
