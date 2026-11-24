import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { ManufacturersApiService } from '../../../../../api-services/manufacturers/manufacturers-api.service';
import {
  CreateManufacturerCommand,
  GetManufacturerByIdQueryDto,
} from '../../../../../api-services/manufacturers/manufacturers-api.model';
import { BaseFormComponent } from '../../../../../core/components/base-classes/base-form-component';
import { applyServerErrors } from '../../../../../core/forms/server-errors';
import { getErrorMessage } from '../../../../../core/interceptors/error-logging-interceptor.service';
import { ToasterService } from '../../../../../core/services/toaster.service';
import { CODE_LIST_CONSTRAINTS } from '../../code-list-constraints';
import { CodeListDialogData } from '../../code-list-page';

/** Typed form: form.controls.name.value is a string, not any. Control names = API field names. */
interface ManufacturerForm {
  name: FormControl<string>;
  /** '' in the form, sent as null (the country is optional) */
  country: FormControl<string>;
}

/**
 * Polica (#6): create/edit dialog of a manufacturer, opened by ManufacturersComponent
 * (CodeListPageComponent.onCreate / onEdit) with { id } in edit mode and {} in create mode.
 *
 * Follows the starter's product-category-upsert dialog, on top of BaseFormComponent:
 *   ngOnInit  builds the form, then initForm(isEdit) calls loadData() in edit mode (GET by id)
 *   onSubmit  (BaseFormComponent) marks all fields touched and calls save() only if the form is valid
 *   save      POST or PUT; success closes the dialog with the saved name (the page shows the snackbar),
 *             a 400 puts the server's message under its field (applyServerErrors), e.g. a duplicate name
 */
@Component({
  selector: 'app-manufacturer-upsert',
  standalone: false,
  templateUrl: './manufacturer-upsert.component.html',
  styleUrl: './manufacturer-upsert.component.scss',
})
export class ManufacturerUpsertComponent
  extends BaseFormComponent<GetManufacturerByIdQueryDto, FormGroup<ManufacturerForm>>
  implements OnInit
{
  private readonly api = inject(ManufacturersApiService);
  private readonly dialogRef = inject<MatDialogRef<ManufacturerUpsertComponent, string>>(MatDialogRef);
  private readonly data = inject<CodeListDialogData>(MAT_DIALOG_DATA);
  private readonly toaster = inject(ToasterService);
  private readonly translate = inject(TranslateService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly limits = CODE_LIST_CONSTRAINTS;

  /** true while POST/PUT runs (isLoading is also true while an edit dialog loads the record) */
  get isSaving(): boolean {
    return this.isLoading && (!this.isEditMode || !!this.model);
  }

  ngOnInit(): void {
    this.form = new FormGroup<ManufacturerForm>({
      name: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(this.limits.manufacturerNameMaxLength)],
      }),
      country: new FormControl('', {
        nonNullable: true,
        validators: [Validators.maxLength(this.limits.manufacturerCountryMaxLength)],
      }),
    });

    this.initForm(this.data.id !== undefined);
  }

  /** Edit mode: fills the form with the current values. */
  protected loadData(): void {
    this.startLoading();

    this.api
      .getById(this.data.id!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (manufacturer) => {
          this.model = manufacturer;
          // reset (not setValue): the form starts untouched, so no field is red before the user types
          this.form.reset({ name: manufacturer.name, country: manufacturer.country ?? '' });
          this.stopLoading();
        },
        error: (err: HttpErrorResponse) => {
          // e.g. 404 when someone else deleted it meanwhile: say why and close (the list stays as it is)
          if (err.status !== 403) {
            this.toaster.error(getErrorMessage(err, this.translate));
          }
          this.dialogRef.close();
        },
      });
  }

  protected save(): void {
    if (this.isLoading) {
      return; // a second Enter while the first save is still running
    }

    const value = this.form.getRawValue();
    // the same body works for POST and PUT (UpdateManufacturerCommand has the same fields)
    const command: CreateManufacturerCommand = {
      name: value.name.trim(),
      country: value.country.trim() || null, // empty field → null
    };

    const request$: Observable<unknown> = this.isEditMode
      ? this.api.update(this.data.id!, command)
      : this.api.create(command);

    this.startLoading();
    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.dialogRef.close(command.name),
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
    this.dialogRef.close();
  }
}
