import { ErrorHandler, Injectable, Injector } from '@angular/core';
import { NotificationService } from '../ui/notification.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  constructor(private readonly injector: Injector) {}

  handleError(error: any): void {
    const notificationService = this.injector.get(NotificationService);
    console.error('Unhandled Global Exception:', error);

    const message = error?.message || 'Ocorreu um erro inesperado na aplicação.';
    notificationService.error('Falha de Sistema', message);
  }
}
