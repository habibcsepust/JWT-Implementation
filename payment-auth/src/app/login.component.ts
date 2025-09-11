// src/app/login.component.ts
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="login-container">
      <h2>Login</h2>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <div>
          <label>Username</label>
          <input type="text" formControlName="username" />
          <div *ngIf="form.controls.username.invalid && form.controls.username.touched" class="error">
            Username is required
          </div>
        </div>

        <div>
          <label>Password</label>
          <input type="password" formControlName="password" />
          <div *ngIf="form.controls.password.invalid && form.controls.password.touched" class="error">
            Password is required
          </div>
        </div>

        <button type="submit" [disabled]="form.invalid || loading">Login</button>

        <p class="error" *ngIf="error">{{ error }}</p>
      </form>
    </div>
  `,
  styles: [`
    .login-container { max-width: 400px; margin: 50px auto; padding: 20px; border: 1px solid #ccc; border-radius: 10px; }
    label { display: block; margin-top: 10px; }
    input { width: 100%; padding: 8px; margin-top: 5px; }
    button { margin-top: 15px; padding: 8px 12px; }
    .error { color: red; font-size: 0.9em; }
  `]
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form = this.fb.group({
    username: ['', Validators.required],
    password: ['', Validators.required]
  });

  loading = false;
  error = '';

  submit() {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';

    const { username, password } = this.form.value;
    this.auth.login(username!, password!).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/dashboard']); // go to dashboard after login
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.title || 'Login failed. Please check credentials.';
      }
    });
  }
}
