import { CommonModule, Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../core/services/user.service';
import { UserProfile } from '../../core/models/user.model';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  profileForm: FormGroup;
  isLoading = false;
  isSaving = false;
  serverMessage: string | null = null;
  serverError: string | null = null;
  private currentUser: UserProfile | null = null;

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private authService: AuthService,
    private location: Location
  ) {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(40)]],
      lastName: ['', [Validators.required, Validators.maxLength(40)]],
      phoneNumber: [
        '',
        [
          Validators.required,
          Validators.pattern(/^[+]?[(]?[0-9]{1,4}[)]?[-\s0-9]{5,}$/)
        ]
      ]
    });
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.serverError = null;
    this.userService.getCurrentUser().subscribe({
      next: (user) => {
        this.currentUser = user;
        this.profileForm.patchValue({
          firstName: user.firstName ?? '',
          lastName: user.lastName ?? '',
          phoneNumber: user.phoneNumber ?? ''
        });
        this.profileForm.markAsPristine();
        this.isLoading = false;
      },
      error: (error) => {
        this.serverError = error.error?.message || 'Unable to load your profile right now.';
        this.isLoading = false;
      }
    });
  }

  submit(): void {
    if (!this.currentUser || this.profileForm.invalid || this.isSaving) {
      this.profileForm.markAllAsTouched();
      return;
    }

    const payload = {
      id: this.currentUser.id,
      ...this.profileForm.value,
      version: this.currentUser.version
    };

    this.serverMessage = null;
    this.serverError = null;
    this.isSaving = true;

    this.userService.updateUser(payload).subscribe({
      next: (updatedUser) => {
        this.currentUser = updatedUser;
        this.authService.updateStoredUser(updatedUser);
        this.serverMessage = 'Profile updated successfully.';
        this.isSaving = false;
        this.profileForm.markAsPristine();
      },
      error: (error) => {
        const message = error.status === 409
          ? 'Someone else updated this profile. Reload and try again.'
          : error.error?.message || 'Failed to update profile.';
        this.serverError = message;
        this.isSaving = false;
      }
    });
  }

  get firstName() {
    return this.profileForm.get('firstName');
  }

  get lastName() {
    return this.profileForm.get('lastName');
  }

  get phoneNumber() {
    return this.profileForm.get('phoneNumber');
  }

  goBack(): void {
    this.location.back();
  }
}
