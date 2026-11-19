import { Component } from '@angular/core';

/**
 * Postavke aplikacije (ruta /admin/settings) – namjerno prazna stranica.
 * Ovdje se dodaju postavke vaše aplikacije (npr. podaci o firmi, podrazumijevana valuta,
 * pravila poslovanja). Lični podaci i lozinka su na stranici "Moj profil" (/admin/profile).
 *
 * Polica (#4): only the chain administrator may open it (route guard in admin-routing-module.ts,
 * menu item hidden for other roles). Issue #11 fills it with the real settings
 * (expiry alert thresholds, minimum shelf life on receipt, transfer suggestion period).
 */
@Component({
  selector: 'app-admin-settings',
  standalone: false,
  templateUrl: './admin-settings.component.html',
  styleUrl: './admin-settings.component.scss',
})
export class AdminSettingsComponent {}
