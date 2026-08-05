import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { LoadingService } from '../../../core/ui/loading.service';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (loadingService.isLoading()) {
      <div class="loading-overlay">
        <div class="spinner-box">
          <div class="pink-spinner"></div>
          <span class="loading-text">Processando requisição...</span>
        </div>
      </div>
    }
  `,
  styles: [`
    .loading-overlay {
      position: fixed;
      top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(18, 18, 21, 0.75);
      backdrop-filter: blur(4px);
      z-index: 10000;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .spinner-box {
      background: var(--bg-surface);
      border: 1px solid var(--border-focus);
      border-radius: 12px;
      padding: 1.5rem 2.5rem;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1rem;
      box-shadow: var(--shadow-lg);
    }
    .pink-spinner {
      width: 36px;
      height: 36px;
      border: 3.5px solid var(--primary-subtle);
      border-top-color: var(--primary);
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }
    .loading-text {
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--text-main);
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `]
})
export class LoadingSpinnerComponent {
  loadingService = inject(LoadingService);
}
