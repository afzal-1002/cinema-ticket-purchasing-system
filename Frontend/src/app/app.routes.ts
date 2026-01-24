import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { AdminWelcomeComponent } from './features/welcome/admin/admin-welcome.component';
import { UserWelcomeComponent } from './features/welcome/user/user-welcome.component';

export const routes: Routes = [
	{ path: 'login', component: LoginComponent },
	{ path: 'register', component: RegisterComponent },
	{ path: 'admin/welcome', component: AdminWelcomeComponent },
	{ path: 'user/welcome', component: UserWelcomeComponent },
	{ path: '', redirectTo: 'login', pathMatch: 'full' },
	{ path: '**', redirectTo: 'login' }
];
