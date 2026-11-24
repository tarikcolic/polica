import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { CityLookupQueryDto } from '../../../../api-services/cities/cities-api.model';
import { CitiesApiService } from '../../../../api-services/cities/cities-api.service';
import {
  CreatePharmacyCommand,
  GetPharmacyByIdQueryDto,
} from '../../../../api-services/pharmacies/pharmacies-api.model';
import { PharmaciesApiService } from '../../../../api-services/pharmacies/pharmacies-api.service';
import { BaseFormComponent } from '../../../../core/components/base-classes/base-form-component';
import { applyServerErrors } from '../../../../core/forms/server-errors';
import { getErrorMessage } from '../../../../core/interceptors/error-logging-interceptor.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { HasUnsavedChanges } from '../../../shared/guards/unsaved-changes.guard';
import { PHARMACY_CONSTRAINTS } from '../pharmacy-constraints';

/** Typed form: form.controls.code.value is a string, not any. Control names = API field names. */
interface PharmacyForm {
  code: FormControl<string>;
  name: FormControl<string>;
  /** null until a city is chosen (Validators.required then fails) */
  cityId: FormControl<number | null>;
  address: FormControl<string>;
  phone: FormControl<string>;
}

/** Where the page returns to after Save or Cancel. */
const LIST_URL = '/admin/pharmacies';

/**
 * Polica (#7): the create/edit PAGE of a pharmacy, at /admin/pharmacies/new and /admin/pharmacies/{id}/edit
 * (pharmacies-routing-module.ts). One component for both URLs: an ":id" in the route means edit mode.
 *
 * Follows the starter's page form (products-add / products-edit) on top of BaseFormComponent:
 *   ngOnInit  builds the form, loads the city options, then initForm(isEdit) calls loadData() in edit mode
 *   onSubmit  (BaseFormComponent) marks all fields touched and calls save() only if the form is valid
 *   save      POST or PUT; success → snackbar and back to the list; a 400 puts the server's message under
 *             its field (applyServerErrors), e.g. a duplicate code or an unknown city
 *
 * HasUnsavedChanges: unsavedChangesGuard (canDeactivate on the route) asks before leaving a changed form.
 */
@Component({
  selector: 'app-pharmacy-upsert',
  standalone: false,
  templateUrl: './pharmacy-upsert.component.html',
  styleUrl: './pharmacy-upsert.component.scss',
})
export class PharmacyUpsertComponent
  extends BaseFormComponent<GetPharmacyByIdQueryDto, FormGroup<PharmacyForm>>
  implements OnInit, HasUnsavedChanges
{
  private readonly api = inject(PharmaciesApiService);
  private readonly citiesApi = inject(CitiesApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);
  private readonly translate = inject(TranslateService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly limits = PHARMACY_CONSTRAINTS;

  /** Id from the route in edit mode, undefined in create mode. */
  pharmacyId?: number;

  /** Options of the city dropdown (GET /api/cities/lookup). */
  cities: CityLookupQueryDto[] = [];

  /** Set after a successful save, so leaving the page right after it does not ask about unsaved changes. */
  private saved = false;

  /** true while POST/PUT runs (isLoading is also true while edit mode loads the pharmacy). */
  get isSaving(): boolean {
    return this.isLoading && (!this.isEditMode || !!this.model);
  }

  ngOnInit(): void {
    this.form = new FormGroup<PharmacyForm>({
      code: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.maxLength(this.limits.codeMaxLength),
          Validators.pattern(this.limits.codePattern),
        ],
      }),
      name: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(this.limits.nameMaxLength)],
      }),
      cityId: new FormControl<number | null>(null, { validators: [Validators.required] }),
      address: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(this.limits.addressMaxLength)],
      }),
      phone: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.maxLength(this.limits.phoneMaxLength),
          Validators.pattern(this.limits.phonePattern),
        ],
      }),
    });

    // City options are needed in both modes. mat-select shows the selected city as soon as the options arrive,
    // so it does not matter whether they come before or after the pharmacy in edit mode.
    this.citiesApi
      .lookup()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((cities) => (this.cities = cities));

    const idParam = this.route.snapshot.paramMap.get('id');
    this.pharmacyId = idParam ? Number(idParam) : undefined;
    this.initForm(this.pharmacyId !== undefined);
  }

  /** Edit mode: fills the form with the current values. */
  protected loadData(): void {
    this.startLoading();

    this.api
      .getById(this.pharmacyId!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (pharmacy) => {
          this.model = pharmacy;
          // reset (not setValue): the form starts pristine and untouched, so no field is red and the
          // unsaved-changes guard does not ask until the user really changes something
          this.form.reset({
            code: pharmacy.code,
            name: pharmacy.name,
            cityId: pharmacy.cityId,
            address: pharmacy.address,
            phone: pharmacy.phone,
          });
          this.stopLoading();
        },
        error: (err: HttpErrorResponse) => {
          // e.g. 404 for a wrong id in the URL: say why and go back to the list
          if (err.status !== 403) {
            this.toaster.error(getErrorMessage(err, this.translate));
          }
          this.router.navigate([LIST_URL]);
        },
      });
  }

  protected save(): void {
    if (this.isLoading) {
      return; // a second Enter while the first save is still running
    }

    const value = this.form.getRawValue();
    // The same body works for POST and PUT (UpdatePharmacyCommand has the same fields).
    // The backend stores the code in uppercase anyway; sending it uppercase keeps the snackbar text the same.
    const command: CreatePharmacyCommand = {
      code: value.code.trim().toUpperCase(),
      name: value.name.trim(),
      cityId: value.cityId!, // not null here: Validators.required passed
      address: value.address.trim(),
      phone: value.phone.trim(),
    };

    const request$: Observable<unknown> = this.isEditMode
      ? this.api.update(this.pharmacyId!, command)
      : this.api.create(command);

    this.startLoading();
    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.saved = true;
        this.stopLoading();
        const key = this.isEditMode ? 'PHARMACIES.UPDATED' : 'PHARMACIES.CREATED';
        this.toaster.success(this.translate.instant(key, { code: command.code, name: command.name }));
        this.router.navigate([LIST_URL]);
      },
      error: (err: HttpErrorResponse) => {
        const message = applyServerErrors(this.form, err, this.translate);
        // a message that landed under a field is not repeated in the banner above the form;
        // the banner is for everything else (no network, 404, 500...)
        const shownUnderField = Object.values(this.form.controls).some((c) => c.hasError('server'));
        this.stopLoading(shownUnderField ? undefined : message);
      },
    });
  }

  onCancel(): void {
    // unsavedChangesGuard asks first if the form was changed
    this.router.navigate([LIST_URL]);
  }

  /** Used by unsavedChangesGuard (canDeactivate). */
  hasUnsavedChanges(): boolean {
    return this.form.dirty && !this.saved;
  }
}
