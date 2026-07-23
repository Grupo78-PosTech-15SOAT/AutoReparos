import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoadingService } from '../ui/loading.service';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);
  
  const isLoginRequest = req.url.toLowerCase().includes('/auth/login');

  // Global loading service is disabled to prevent full screen blocking.
  // We use localized spinners in each table instead.
  return next(req);
};
