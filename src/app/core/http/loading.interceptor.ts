import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoadingService } from '../ui/loading.service';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);
  
  const isLoginRequest = req.url.toLowerCase().includes('/auth/login');

  if (!isLoginRequest) {
    loadingService.show();
  }

  return next(req).pipe(
    finalize(() => {
      if (!isLoginRequest) {
        loadingService.hide();
      }
    })
  );
};
