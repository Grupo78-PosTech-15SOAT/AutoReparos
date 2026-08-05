import { Component, ChangeDetectionStrategy, input } from '@angular/core';

@Component({
  selector: 'app-placa-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span class="badge-placa" [class]="size()">
      {{ placa() }}
    </span>
  `,
  styles: [`
    :host {
      display: inline-flex;
    }
    .badge-placa {
      font-family: var(--font-mono);
      background: rgba(59, 130, 246, 0.08);
      color: #93C5FD;
      border-left: 4px solid #3B82F6;
      border-top: 1px solid rgba(59, 130, 246, 0.2);
      border-right: 1px solid rgba(59, 130, 246, 0.2);
      border-bottom: 1px solid rgba(59, 130, 246, 0.2);
      font-weight: 700;
      border-radius: 4px;
      display: inline-flex;
      align-items: center;
      letter-spacing: 0.05em;
      text-transform: uppercase;
      white-space: nowrap;
    }
    .badge-placa.sm {
      font-size: 0.65rem;
      padding: 0.08rem 0.3rem;
      border-left-width: 3px;
    }
    .badge-placa.md {
      font-size: 0.8rem;
      padding: 0.15rem 0.5rem;
      border-left-width: 4px;
    }
    .badge-placa.lg {
      font-size: 1rem;
      padding: 0.25rem 0.75rem;
      border-left-width: 6px;
    }
  `]
})
export class PlacaBadgeComponent {
  placa = input.required<string>();
  size = input<'sm' | 'md' | 'lg'>('md');
}
