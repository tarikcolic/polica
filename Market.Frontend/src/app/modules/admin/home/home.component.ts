import { Component, inject } from '@angular/core';
import { CurrentUserService } from '../../../core/services/auth/current-user.service';

/**
 * Polica (#4): start page of the staff area (route /admin), shown to all three roles
 * (chain administrator, pharmacy manager, pharmacist).
 *
 * Why it exists: the starter sent the administrator to /admin/products and everyone else to
 * /client (the customer area). Both were removed in #4, and every role now shares the /admin shell
 * (AdminLayoutComponent). The default child route needs a page that every role may open; an
 * admin-only page here (e.g. Settings) would make myAuthGuard send a manager back to /admin, in a loop.
 *
 * It only reads the logged-in user from CurrentUserService (a signal decoded from the JWT),
 * so it makes no API calls. Issue #21 replaces its content with the chain / pharmacy dashboard.
 */
@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent {
  protected currentUser = inject(CurrentUserService);
}
