import { Component, input, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-id-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span class="badge-id" [class]="size()">
      {{ text() }}
    </span>
  `,
  styles: [`
    :host {
      display: inline-flex;
    }
    .badge-id {
      font-family: var(--font-mono);
      background: rgba(237, 20, 91, 0.08);
      color: #ED145B;
      border: 1px solid rgba(237, 20, 91, 0.25);
      font-weight: 600;
      border-radius: 6px;
      display: inline-flex;
      align-items: center;
      white-space: nowrap;
    }
    .badge-id.sm {
      font-size: 0.65rem;
      padding: 0.1rem 0.35rem;
    }
    .badge-id.md {
      font-size: 0.8rem;
      padding: 0.2rem 0.5rem;
    }
    .badge-id.lg {
      font-size: 1rem;
      padding: 0.3rem 0.8rem;
    }
  `]
})
export class IdBadgeComponent {
  text = input.required<string>();
  size = input<'sm' | 'md' | 'lg'>('md');
}
