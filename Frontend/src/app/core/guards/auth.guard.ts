import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (_route, state): boolean | UrlTree => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getToken();

  if (token) {
    return true;
  }

  authService.clearSession();
  return router.createUrlTree(['/login'], {
    queryParams: state.url && state.url !== '/login' ? { returnUrl: state.url } : undefined
  });
};
