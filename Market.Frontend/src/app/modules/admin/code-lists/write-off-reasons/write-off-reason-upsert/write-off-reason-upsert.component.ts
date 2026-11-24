import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { WriteOffReasonsApiService } from '../../../../../api-services/write-off-reasons/write-off-reasons-api.service';
import {
  CreateWriteOffReasonCommand,
  GetWriteOffReasonByIdQueryDto,
} from '../../../../../api-services/write-off-reasons/write-off-reasons-api.model';
import { BaseFormComponent } from '../../../../../core/components/base-classes/base-form-component';
import { applyServerErrors } from '../../../../../core/forms/server-errors';
import { getErrorMessage } from '../../../../../core/interceptors/error-logging-interceptor.service';
import { ToasterService } from '../../../../../core/services/toaster.service';
import { CODE_LIST_CONSTRAINTS } from '../../code-list-constraints';
import { CodeListDialogData } from '../../code-list-page';

/** Typed form of a write-off reason. */
interface WriteOffReasonForm {
  name: FormControl<string>;
  /** checkbox; true = the pharmacist must type a note when choosing this reason (#20) */
  requiresNote: FormControl<boolean>;
}

/**
 * Polica (#6): create/edit dialog of a write-off reason, opened by WriteOffReasonsComponent.
 * Works exactly like ManufacturerUpsertComponent (see the comment there); the extra field is the
 * "requires a note" checkbox, which is always sent (the PUT requires it).
 */
@Component({
  selector: 'app-write-off-reason-upsert',
  standalone: false,
  templateUrl: './write-off-reason-upsert.component.html',
  styleUrl: './write-off-reason-upsert.component.scss',
})
export class WriteOffReasonUpsertComponent
  extends BaseFormComponent<GetWriteOffReasonByIdQueryDto, FormGroup<WriteOffReasonForm>>
  implements OnInit
{
  private readonly api = inject(WriteOffReasonsApiService);
  private readonly dialogRef = inject<MatDialogRef<WriteOffReasonUpsertComponent, string>>(MatDialogRef);
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
    this.form = new FormGroup<WriteOffReasonForm>({
      name: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(this.limits.writeOffReasonNameMaxLength)],
      }),
      // a new reason does not require a note unless the administrator ticks the box
      requiresNote: new FormControl(false, { nonNullable: true }),
    });

    this.initForm(this.data.id !== undefined);
  }

  protected loadData(): void {
    this.startLoading();

    this.api
      .getById(this.data.id!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (reason) => {
          this.model = reason;
          this.form.reset({ name: reason.name, requiresNote: reason.requiresNote });
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

    const value = this.form.getRawValue();
    // the same body works for POST and PUT (UpdateWriteOffReasonCommand has the same fields)
    const command: CreateWriteOffReasonCommand = {
      name: value.name.trim(),
      requiresNote: value.requiresNote,
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
