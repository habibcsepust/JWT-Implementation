// src/app/dashboard.component.ts
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';

@Component({
  standalone: true,
  selector: 'app-dashboard',
  imports: [CommonModule],
  template: `
    <div style="padding:20px">
      <h2>Dashboard</h2>
      <p>✅ You are logged in!</p>
      <button (click)="logout()">Logout</button>
    </div>
  `
})
export class DashboardComponent {
  private auth = inject(AuthService);
  logout() { this.auth.logout(); }
}
