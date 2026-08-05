import {
  Component,
  ChangeDetectionStrategy,
  OnDestroy,
  input,
  output,
  effect,
} from '@angular/core';

@Component({
  selector: 'app-drawer',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(document:keydown.escape)': 'onEsc()'
  },
  template: `
    @if (isOpen()) {
      <!-- Backdrop -->
      <div
        class="drawer-backdrop"
        role="presentation"
        (click)="fechar()">
      </div>

      <!-- Painel -->
      <div
        class="drawer-panel"
        [class.drawer-right]="position() === 'right'"
        [class.drawer-left]="position() === 'left'"
        [style.width]="width()"
        role="dialog"
        aria-modal="true"
        aria-label="Painel lateral">

        <!-- Slot: Header -->
        <div class="drawer-header-zone">
          <ng-content select="[drawer-header]"></ng-content>
        </div>

        <!-- Slot: Body (scrollável) -->
        <div class="drawer-body-zone">
          <ng-content select="[drawer-body]"></ng-content>
        </div>

        <!-- Slot: Footer -->
        <div class="drawer-footer-zone">
          <ng-content select="[drawer-footer]"></ng-content>
        </div>
      </div>
    }
  `,
  styles: [`
    /* ── Backdrop ── */
    .drawer-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.55);
      backdrop-filter: blur(2px);
      -webkit-backdrop-filter: blur(2px);
      z-index: 1000;
      animation: backdropFadeIn 0.25s ease-out;
    }

    @keyframes backdropFadeIn {
      from { opacity: 0; }
      to   { opacity: 1; }
    }

    /* ── Painel ── */
    .drawer-panel {
      position: fixed;
      top: 0;
      height: 100vh;
      max-width: 90vw;
      z-index: 1001;
      background: var(--bg-surface, #18181C);
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }

    .drawer-right {
      right: 0;
      border-left: 1px solid var(--border-pink, rgba(237, 20, 91, 0.3));
      box-shadow: -8px 0 32px rgba(0, 0, 0, 0.55),
                  -2px 0 8px rgba(237, 20, 91, 0.12);
      animation: drawerSlideInRight 0.32s cubic-bezier(0.16, 1, 0.3, 1);
    }

    .drawer-left {
      left: 0;
      border-right: 1px solid var(--border-pink, rgba(237, 20, 91, 0.3));
      box-shadow: 8px 0 32px rgba(0, 0, 0, 0.55),
                  2px 0 8px rgba(237, 20, 91, 0.12);
      animation: drawerSlideInLeft 0.32s cubic-bezier(0.16, 1, 0.3, 1);
    }

    @keyframes drawerSlideInRight {
      from { transform: translateX(100%); opacity: 0.6; }
      to   { transform: translateX(0);    opacity: 1; }
    }

    @keyframes drawerSlideInLeft {
      from { transform: translateX(-100%); opacity: 0.6; }
      to   { transform: translateX(0);     opacity: 1; }
    }

    /* ── Zonas internas ── */
    .drawer-header-zone {
      flex-shrink: 0;
      border-bottom: 1px solid var(--border-color, rgba(255, 255, 255, 0.08));
    }

    .drawer-body-zone {
      flex: 1;
      overflow-y: auto;
      min-height: 0;
    }

    .drawer-body-zone::-webkit-scrollbar { width: 4px; }
    .drawer-body-zone::-webkit-scrollbar-track { background: transparent; }
    .drawer-body-zone::-webkit-scrollbar-thumb {
      background: rgba(255, 255, 255, 0.12);
      border-radius: 2px;
    }
    .drawer-body-zone::-webkit-scrollbar-thumb:hover {
      background: rgba(255, 255, 255, 0.22);
    }

    .drawer-footer-zone {
      flex-shrink: 0;
      border-top: 1px solid var(--border-color, rgba(255, 255, 255, 0.08));
    }

    /* ── Responsividade ── */
    @media (max-width: 768px) {
      .drawer-panel {
        max-width: 100vw !important;
        width: 100vw !important;
      }
    }
  `]
})
export class DrawerComponent implements OnDestroy {
  isOpen = input(false);
  width = input('520px');
  position = input<'left' | 'right'>('right');

  closed = output<void>();

  constructor() {
    effect(() => {
      document.body.style.overflow = this.isOpen() ? 'hidden' : '';
    });
  }

  ngOnDestroy(): void {
    document.body.style.overflow = '';
  }

  onEsc(): void {
    if (this.isOpen()) this.fechar();
  }

  fechar(): void {
    this.closed.emit();
  }
}
