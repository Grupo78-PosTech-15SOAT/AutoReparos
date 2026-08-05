import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../features/auth/services/auth.service';
import { NotificationService } from '../ui/notification.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const notification = inject(NotificationService);

  const allowedRoles = route.data?.['roles'] as Array<string>;
  if (!allowedRoles || allowedRoles.length === 0) {
    return true;
  }

  if (authService.hasRole(allowedRoles)) {
    return true;
  }

  notification.error('Acesso Negado', 'Seu perfil de acesso não tem permissão para visualizar esta página.');
  router.navigate(['/ordens-servico/fila']);
  return false;
};
