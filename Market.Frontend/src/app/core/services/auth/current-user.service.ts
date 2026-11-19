// src/app/core/services/auth/current-user.service.ts
import { Injectable, inject, computed } from '@angular/core';
import { AuthFacadeService } from './auth-facade.service';

@Injectable({ providedIn: 'root' })
export class CurrentUserService {
  private auth = inject(AuthFacadeService);

  /** Signal koji UI može čitati (readonly) */
  currentUser = computed(() => this.auth.currentUser());

  isAuthenticated = computed(() => this.auth.isAuthenticated());
  isAdmin = computed(() => this.auth.isAdmin());
  isManager = computed(() => this.auth.isManager());
  isEmployee = computed(() => this.auth.isEmployee());

  get snapshot() {
    return this.auth.currentUser();
  }

  /**
   * Start page after login, and the page myAuthGuard sends a user to when a role check fails.
   * Polica (#4): all three roles share the /admin shell, whose home page is open to every role.
   * (The starter sent non-admin users to /client, the customer area that was removed.)
   */
  getDefaultRoute(): string {
    const user = this.snapshot;
    if (!user) return '/auth/login';

    return '/admin';
  }
}
