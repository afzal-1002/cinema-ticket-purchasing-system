import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { AdminWelcomeComponent } from './features/welcome/admin/admin-welcome.component';
import { UserWelcomeComponent } from './features/welcome/user/user-welcome.component';
import { ProfileComponent } from './features/profile/profile.component';
import { CinemaListComponent } from './features/cinemas/cinema-list/cinema-list.component';
import { ScreeningListComponent } from './features/screenings/screening-list/screening-list.component';
import { ScreeningDetailComponent } from './features/screenings/screening-detail/screening-detail.component';
import { ScreeningCreateComponent } from './features/screenings/screening-create/screening-create.component';
import { MovieManagementComponent } from './features/movies/movie-management/movie-management.component';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { UserScreeningsComponent } from './features/browse/user-screenings/user-screenings.component';
import { SeatSelectionComponent } from './features/seat-selection/seat-selection.component';
import { UserManagementComponent } from './features/users/user-management/user-management.component';

export const routes: Routes = [
	{ path: 'login', component: LoginComponent },
	{ path: 'register', component: RegisterComponent },
	{ path: 'profile', component: ProfileComponent, canActivate: [authGuard] },
	{ path: 'cinemas', component: CinemaListComponent, canActivate: [authGuard] },
	{ path: 'screenings', component: ScreeningListComponent, canActivate: [authGuard, adminGuard] },
	{ path: 'screenings/new', component: ScreeningCreateComponent, canActivate: [authGuard, adminGuard] },
	{ path: 'screenings/:id', component: ScreeningDetailComponent, canActivate: [authGuard] },
	{ path: 'movies', component: MovieManagementComponent, canActivate: [authGuard, adminGuard] },
	{ path: 'browse/screenings', component: UserScreeningsComponent, canActivate: [authGuard] },
	{ path: 'browse/screenings/:id/seats', component: SeatSelectionComponent, canActivate: [authGuard] },
	{ path: 'admin/welcome', component: AdminWelcomeComponent, canActivate: [authGuard] },
	{ path: 'admin/users', component: UserManagementComponent, canActivate: [authGuard, adminGuard] },
	{ path: 'user/welcome', component: UserWelcomeComponent, canActivate: [authGuard] },
	{ path: '', redirectTo: 'login', pathMatch: 'full' },
	{ path: '**', redirectTo: 'login' }
];
