import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../../core/ui/notification.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container">
      @for (toast of notificationService.toasts(); track toast.id) {
        <div class="toast-item" [ngClass]="toast.type">
          <div class="toast-content">
            <div class="toast-title">{{ toast.title }}</div>
            <div class="toast-message">{{ toast.message }}</div>
          </div>
          <button (click)="notificationService.remove(toast.id)" class="toast-close">&times;</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 5rem;
      right: 1.5rem;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      max-width: 380px;
      width: 100%;
    }
    .toast-item {
      background: rgba(24, 24, 28, 0.95);
      backdrop-filter: blur(12px);
      border-radius: 8px;
      padding: 1rem;
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 0.75rem;
      box-shadow: 0 10px 25px rgba(0, 0, 0, 0.5);
      border-left: 4px solid #3B82F6;
      animation: slideIn 0.3s cubic-bezier(0.16, 1, 0.3, 1);
    }
    .toast-item.success { border-left-color: #10B981; }
    .toast-item.error { border-left-color: #EF4444; }
    .toast-item.warning { border-left-color: #F59E0B; }
    .toast-item.info { border-left-color: #3B82F6; }

    .toast-title { font-weight: 700; font-size: 0.9rem; color: #ffffff; margin-bottom: 0.2rem; }
    .toast-message { font-size: 0.825rem; color: #A1A1AA; }
    .toast-close { background: none; border: none; color: #71717A; font-size: 1.2rem; cursor: pointer; }
    .toast-close:hover { color: #ffffff; }

    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
  `]
})
export class ToastComponent {
  notificationService = inject(NotificationService);
}
