import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { toDateOnly } from '../../../core/forms/date-only';
import { APP_DATE_TIME_FORMAT } from '../../../core/locale/app-locale';
import { LoadingBarService } from '../../../core/services/loading-bar.service';
import { ToasterService } from '../../../core/services/toaster.service';
import { FitStatusTone } from '../../shared/components/fit-status-badge/fit-status-badge.component';
import { DialogButton } from '../../shared/models/dialog-config.model';
import { DialogHelperService } from '../../shared/services/dialog-helper.service';

type ToastKind = 'success' | 'error' | 'warning' | 'info';

/**
 * Polica (#5): UI kit (route /admin/ui-kit), a live sample of the regional formats and the shared
 * components from SharedModule, so they can be checked once and copied from in later issues (#6 on).
 *
 * Chain administrator only (guard in admin-routing-module.ts) and not in the menu: it is a reference
 * page for development and review, not part of the pharmacy workflow. It makes no API calls; every
 * value below is a fixed sample.
 */
@Component({
  selector: 'app-ui-kit',
  standalone: false,
  templateUrl: './ui-kit.component.html',
  styleUrl: './ui-kit.component.scss',
})
export class UiKitComponent {
  private dialogHelper = inject(DialogHelperService);
  private toaster = inject(ToasterService);
  private translate = inject(TranslateService);
  private loadingBar = inject(LoadingBarService);

  // ---- Regional formats (bs-BA, configured in app-module.ts and core/locale/app-locale.ts) ----

  /** Exposed to the template, which cannot read an imported constant directly. */
  protected readonly dateTimeFormat = APP_DATE_TIME_FORMAT;
  protected readonly today = new Date();
  /** A timestamp as the API sends it (UTC, "Z"); the DatePipe shows it in local time. */
  protected readonly sampleUtcTimestamp = '2026-10-15T12:30:00Z';
  /** A DateOnly as the API sends it (no time, no zone). */
  protected readonly sampleDateOnly = '2026-10-15';
  protected readonly sampleNumber = 1234567.891;
  protected readonly sampleAmount = 1234.5;

  // ---- Status badges: one example per tone (real status keys come with each feature) ----

  protected readonly badges: { tone: FitStatusTone; labelKey: string; icon?: string }[] = [
    { tone: 'neutral', labelKey: 'UI_KIT.BADGES.NEUTRAL', icon: 'edit_note' },
    { tone: 'info', labelKey: 'UI_KIT.BADGES.INFO', icon: 'send' },
    { tone: 'success', labelKey: 'UI_KIT.BADGES.SUCCESS', icon: 'check_circle' },
    { tone: 'warning', labelKey: 'UI_KIT.BADGES.WARNING', icon: 'schedule' },
    { tone: 'danger', labelKey: 'UI_KIT.BADGES.DANGER', icon: 'block' },
  ];

  // ---- Field errors: a typed form like a real feature form ----

  protected readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(20)],
    }),
    expiryDate: new FormControl<Date | null>(null, { validators: [Validators.required] }),
  });

  /** What a form would send to the API for a DateOnly field (core/forms/date-only.ts). */
  protected get expiryDateForApi(): string | null {
    return toDateOnly(this.form.controls.expiryDate.value);
  }

  /** Same as BaseFormComponent.onSubmit(): touched fields show their <mat-error>. */
  protected validateForm(): void {
    this.form.markAllAsTouched();
    if (this.form.valid) {
      this.toaster.success(this.translate.instant('UI_KIT.FORM.VALID'));
    }
  }

  // ---- Dialogs and snackbars ----

  protected openConfirmAction(): void {
    this.dialogHelper
      .confirmAction(
        'UI_KIT.DIALOGS.ACTION_TITLE',
        'UI_KIT.DIALOGS.ACTION_MESSAGE',
        'UI_KIT.DIALOGS.ACTION_CONFIRM',
        { number: 'PO-2026-00042' }
      )
      .subscribe(confirmed => this.showConfirmResult(confirmed));
  }

  protected openConfirmDelete(): void {
    // confirmDelete() is the starter's method: it returns the clicked button, not true/false
    this.dialogHelper
      .confirmDelete(this.translate.instant('UI_KIT.DIALOGS.DELETE_ITEM'))
      .subscribe(result => this.showConfirmResult(result?.button === DialogButton.DELETE));
  }

  protected showToast(kind: ToastKind): void {
    const message = this.translate.instant(`UI_KIT.TOASTS.${kind.toUpperCase()}`);
    this.toaster[kind](message);
  }

  /** Shows the global loading bar for 2 s, as if an HTTP request were running. */
  protected simulateLoading(): void {
    this.loadingBar.show();
    setTimeout(() => this.loadingBar.hide(), 2000);
  }

  /** Action button inside the empty state sample. */
  protected emptyStateAction(): void {
    this.toaster.info(this.translate.instant('UI_KIT.EMPTY.ACTION_CLICKED'));
  }

  private showConfirmResult(confirmed: boolean): void {
    if (confirmed) {
      this.toaster.success(this.translate.instant('UI_KIT.DIALOGS.CONFIRMED'));
    } else {
      this.toaster.info(this.translate.instant('UI_KIT.DIALOGS.CANCELLED'));
    }
  }
}
