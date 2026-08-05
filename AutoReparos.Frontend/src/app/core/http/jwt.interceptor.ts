import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { STORAGE_TOKEN } from '../tokens/storage.token';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const storage = inject(STORAGE_TOKEN);
  const token = storage.getItem('autoreparos_token');
  if (token) {
    const cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(cloned);
  }
  return next(req);
};
