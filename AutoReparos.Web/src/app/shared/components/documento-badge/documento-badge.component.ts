import { Component, input, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-documento-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span class="badge-documento" [class]="size()">
      {{ documento() }}
    </span>
  `,
  styles: [`
    :host {
      display: inline-flex;
    }
    .badge-documento {
      font-family: var(--font-mono);
      background: rgba(16, 185, 129, 0.1);
      color: #34D399;
      border: 1px solid rgba(16, 185, 129, 0.3);
      font-weight: 600;
      border-radius: 6px;
      display: inline-flex;
      align-items: center;
      letter-spacing: 0.02em;
      white-space: nowrap;
    }
    .badge-documento.sm {
      font-size: 0.65rem;
      padding: 0.1rem 0.35rem;
    }
    .badge-documento.md {
      font-size: 0.8rem;
      padding: 0.2rem 0.5rem;
    }
    .badge-documento.lg {
      font-size: 1rem;
      padding: 0.3rem 0.8rem;
    }
  `]
})
export class DocumentoBadgeComponent {
  documento = input.required<string>();
  size = input<'sm' | 'md' | 'lg'>('md');
}
