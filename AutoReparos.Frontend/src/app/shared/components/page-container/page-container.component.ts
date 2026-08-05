import { Component, input, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-page-container',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-container fade-in" [class]="type()" [style.max-width]="maxWidth()">
      <ng-content></ng-content>
    </div>
  `,
  styles: [`
    .page-container {
      max-width: 1280px;
      margin: 0 auto;
      padding: 1.5rem;
      width: 100%;
      box-sizing: border-box;
    }
    .kanban {
      display: flex;
      flex-direction: column;
      height: calc(100vh - 7rem);
      overflow: hidden;
      max-width: 100% !important;
      padding-bottom: 0.75rem;
    }
    @media (max-width: 768px) {
      .kanban {
        height: calc(100vh - 8rem);
      }
    }
    .fade-in {
      animation: fadeIn 0.3s cubic-bezier(0.16, 1, 0.3, 1);
    }
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(6px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `]
})
export class PageContainerComponent {
  type = input<'default' | 'kanban'>('default');
  maxWidth = input<string | undefined>();
}
