import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../../core/ui/loading.service';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule],
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
      background: rgba(10, 10, 12, 0.7);
      backdrop-filter: blur(4px);
      z-index: 10000;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .spinner-box {
      background: #18181C;
      border: 1px solid rgba(237, 20, 91, 0.4);
      border-radius: 12px;
      padding: 1.5rem 2.5rem;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1rem;
      box-shadow: 0 12px 30px rgba(0, 0, 0, 0.8);
    }
    .pink-spinner {
      width: 36px;
      height: 36px;
      border: 3.5px solid rgba(237, 20, 91, 0.2);
      border-top-color: #ED145B;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }
    .loading-text {
      font-size: 0.85rem;
      font-weight: 600;
      color: #E2E8F0;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `]
})
export class LoadingSpinnerComponent {
  loadingService = inject(LoadingService);
}
