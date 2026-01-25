import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const adminGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const hasToken = !!authService.getToken();

  if (hasToken && authService.isAdmin()) {
    return true;
  }

  const destination = hasToken ? '/user/welcome' : '/login';
  return router.createUrlTree([destination], {
    queryParams: state.url ? { returnUrl: state.url } : undefined
  });
};
