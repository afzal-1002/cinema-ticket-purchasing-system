import { CommonModule, Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserProfile, UpdateUserPayload } from '../../../core/models/user.model';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: UserProfile[] = [];
  isLoading = false;
  isSaving = false;
  deletingId: number | null = null;
  errorMessage: string | null = null;
  formMessage: string | null = null;
  successMessage: string | null = null;
  form: FormGroup;
  selectedUser: UserProfile | null = null;
  private currentUserId?: number;

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private fb: FormBuilder,
    private location: Location,
    private router: Router
  ) {
    this.currentUserId = this.authService.getStoredUser()?.id ?? undefined;
    this.form = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      phoneNumber: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(20)]]
    });
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.userService.getUsers().subscribe({
      next: (users) => {
        this.users = users;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load users right now. Please try again later.';
        this.isLoading = false;
      }
    });
  }

  trackById(_index: number, user: UserProfile): number | undefined {
    return user.id;
  }

  startEdit(user: UserProfile): void {
    if (!user.id) {
      return;
    }
    this.selectedUser = user;
    this.form.reset({
      firstName: user.firstName ?? '',
      lastName: user.lastName ?? '',
      phoneNumber: user.phoneNumber ?? ''
    });
    this.formMessage = null;
    this.successMessage = null;
  }

  cancelEdit(): void {
    this.selectedUser = null;
    this.form.reset();
    this.formMessage = null;
  }

  submit(): void {
    if (!this.selectedUser || !this.selectedUser.id) {
      this.formMessage = 'Select a user to update.';
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload: UpdateUserPayload = {
      id: this.selectedUser.id,
      firstName: this.form.value.firstName ?? '',
      lastName: this.form.value.lastName ?? '',
      phoneNumber: this.form.value.phoneNumber ?? '',
      version: this.selectedUser.version
    };

    this.isSaving = true;
    this.formMessage = null;

    this.userService.updateUser(payload).subscribe({
      next: (updated) => {
        this.isSaving = false;
        this.successMessage = `${updated.firstName ?? 'User'} updated successfully.`;
        this.users = this.users.map((user) => (user.id === updated.id ? { ...user, ...updated } : user));
        this.selectedUser = updated;
      },
      error: (err) => {
        this.isSaving = false;
        if (err?.status === 409) {
          this.formMessage = 'Another admin already saved changes to this user. Reloading latest data…';
          this.loadUsers();
        } else {
          this.formMessage = err?.error?.message ?? 'Failed to update user. Please try again.';
        }
      }
    });
  }

  deleteUser(user: UserProfile): void {
    if (!user.id || !this.canDelete(user)) {
      return;
    }
    const confirmed = confirm(
      `Delete ${user.firstName ?? user.email}? This action cannot be undone and removes reservations tied to this account.`
    );
    if (!confirmed) {
      return;
    }

    this.deletingId = user.id;
    this.successMessage = null;
    this.userService.deleteUser(user.id).subscribe({
      next: () => {
        this.deletingId = null;
        this.users = this.users.filter((u) => u.id !== user.id);
        if (this.selectedUser?.id === user.id) {
          this.cancelEdit();
        }
        this.successMessage = 'User removed successfully.';
      },
      error: (err) => {
        this.deletingId = null;
        this.errorMessage = err?.error?.message ?? 'Failed to delete user.';
      }
    });
  }

  refresh(): void {
    this.loadUsers();
  }

  goBack(): void {
    this.location.back();
  }

  goHome(): void {
    this.router.navigate(['/admin/welcome']);
  }

  canDelete(user: UserProfile): boolean {
    return !!user.id && user.id !== this.currentUserId;
  }

  roleBadge(user: UserProfile): string {
    return user.isAdmin ? 'Admin' : 'User';
  }
}
