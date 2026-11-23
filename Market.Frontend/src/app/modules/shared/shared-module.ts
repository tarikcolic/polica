import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import {FitPaginatorBarComponent} from './components/fit-paginator-bar/fit-paginator-bar.component';
import {materialModules} from './material-modules';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {TranslatePipe} from '@ngx-translate/core';
import { FitConfirmDialogComponent } from './components/fit-confirm-dialog/fit-confirm-dialog.component';
import {DialogHelperService} from './services/dialog-helper.service';
import { FitLoadingBarComponent } from './components/fit-loading-bar/fit-loading-bar.component';
import { FitTableSkeletonComponent } from './components/fit-table-skeleton/fit-table-skeleton.component';
import { FitFieldErrorComponent } from './components/fit-field-error/fit-field-error.component';
import { FitStatusBadgeComponent } from './components/fit-status-badge/fit-status-badge.component';
import { FitEmptyStateComponent } from './components/fit-empty-state/fit-empty-state.component';

/**
 * Shared UI used by every feature module (each lazy module imports SharedModule).
 *
 * Polica (#5): the shared building blocks for a consistent UI on all screens. A live example of each one
 * is on the /admin/ui-kit page (modules/admin/ui-kit):
 *   - confirm dialog        → DialogHelperService (confirm, confirmDelete, confirmAction)
 *   - loading indicator     → app-fit-loading-bar (automatic for every HTTP request) and app-fit-table-skeleton
 *   - success snackbar      → ToasterService (core/services/toaster.service.ts)
 *   - field error           → app-fit-field-error inside <mat-error>
 *   - status badge          → app-fit-status-badge (new in #5)
 *   - empty state for lists → app-fit-empty-state (new in #5)
 */
@NgModule({
  declarations: [
    FitPaginatorBarComponent,
    FitConfirmDialogComponent,
    FitLoadingBarComponent,
    FitTableSkeletonComponent,
    FitFieldErrorComponent,
    FitStatusBadgeComponent,
    FitEmptyStateComponent
  ],
  imports: [
    CommonModule,
    // routerLink directives for shared components (directives only, no routes)
    RouterModule,
    ReactiveFormsModule,
    FormsModule,
    TranslatePipe,
    ...materialModules
  ],
  providers: [
    DialogHelperService
  ],
  exports:[
    FitPaginatorBarComponent,
    CommonModule,
    ReactiveFormsModule,
    TranslatePipe,
    FormsModule,
    FitLoadingBarComponent,
    FitTableSkeletonComponent,
    FitFieldErrorComponent,
    FitStatusBadgeComponent,
    FitEmptyStateComponent,
    materialModules
  ]
})
export class SharedModule { }
