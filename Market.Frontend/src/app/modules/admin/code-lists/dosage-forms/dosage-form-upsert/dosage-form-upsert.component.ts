import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { DosageFormsApiService } from '../../../../../api-services/dosage-forms/dosage-forms-api.service';
import {
  CreateDosageFormCommand,
  GetDosageFormByIdQueryDto,
} from '../../../../../api-services/dosage-forms/dosage-forms-api.model';
import { BaseFormComponent } from '../../../../../core/components/base-classes/base-form-component';
import { applyServerErrors } from '../../../../../core/forms/server-errors';
import { getErrorMessage } from '../../../../../core/interceptors/error-logging-interceptor.service';
import { ToasterService } from '../../../../../core/services/toaster.service';
import { CODE_LIST_CONSTRAINTS } from '../../code-list-constraints';
import { CodeListDialogData } from '../../code-list-page';

/** Typed form of a dosage form: only the name. */
interface DosageFormForm {
  name: FormControl<string>;
}

/**
 * Polica (#6): create/edit dialog of a dosage form, opened by DosageFormsComponent.
 * Works exactly like ManufacturerUpsertComponent (see the comment there), with one field.
 */
@Component({
  selector: 'app-dosage-form-upsert',
  standalone: false,
  templateUrl: './dosage-form-upsert.component.html',
  styleUrl: './dosage-form-upsert.component.scss',
})
export class DosageFormUpsertComponent
  extends BaseFormComponent<GetDosageFormByIdQueryDto, FormGroup<DosageFormForm>>
  implements OnInit
{
  private readonly api = inject(DosageFormsApiService);
  private readonly dialogRef = inject<MatDialogRef<DosageFormUpsertComponent, string>>(MatDialogRef);
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
    this.form = new FormGroup<DosageFormForm>({
      name: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(this.limits.dosageFormNameMaxLength)],
      }),
    });

    this.initForm(this.data.id !== undefined);
  }

  protected loadData(): void {
    this.startLoading();

    this.api
      .getById(this.data.id!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (dosageForm) => {
          this.model = dosageForm;
          this.form.reset({ name: dosageForm.name });
          this.stopLoading();
        },
        error: (err: HttpErrorResponse) => {
          if (err.status !== 403) {
            this.toaster.error(getErrorMessage(err, this.translate));
          }
          this.dialogRef.close();
        },
      });
  }

  protected save(): void {
    if (this.isLoading) {
      return;
    }

    // the same body works for POST and PUT (UpdateDosageFormCommand has the same field)
    const command: CreateDosageFormCommand = {
      name: this.form.controls.name.value.trim(),
    };

    const request$: Observable<unknown> = this.isEditMode
      ? this.api.update(this.data.id!, command)
      : this.api.create(command);

    this.startLoading();
    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.dialogRef.close(command.name),
      error: (err: HttpErrorResponse) => {
        const message = applyServerErrors(this.form, err, this.translate);
        // a duplicate name is shown under the field, so the banner stays empty for it
        const shownUnderField = Object.values(this.form.controls).some((c) => c.hasError('server'));
        this.stopLoading(shownUnderField ? undefined : message);
      },
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
