import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../ui/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const isLoginRequest = req.url.toLowerCase().includes('/auth/login');

      if (error.status === 401) {
        if (isLoginRequest) {
          const message = error.error?.message || 'E-mail ou senha incorretos. Verifique suas credenciais.';
          notificationService.error('Falha no Login', message);
        } else {
          notificationService.warning('Sessão Expirada', 'Por favor, faça login novamente.');
          localStorage.removeItem('autoreparos_token');
          localStorage.removeItem('autoreparos_user');
          router.navigate(['/login']);
        }
      } else if (error.status === 403) {
        notificationService.error('Acesso Negado', 'Seu perfil de usuário não possui permissão para esta ação.');
      } else if (error.status === 400) {
        const message = error.error?.message || error.error?.title || 'Requisição inválida. Verifique os dados.';
        notificationService.error('Erro de Validação (400)', message);
      } else if (error.status === 404) {
        notificationService.warning('Não Encontrado (404)', error.error?.message || 'O recurso solicitado não foi encontrado.');
      } else if (error.status === 0 || error.status >= 500) {
        notificationService.error('Erro de Servidor / Conexão', 'Não foi possível se comunicar com o backend.');
      }

      return throwError(() => error);
    })
  );
};
