import { DestroyRef, Directive, inject, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ComponentType } from '@angular/cdk/portal';
import { MatDialog } from '@angular/material/dialog';
import { TranslateService } from '@ngx-translate/core';
import { debounceTime, map, Observable, Subject, Subscription } from 'rxjs';
import { BaseListPagedComponent } from '../../../core/components/base-classes/base-list-paged-component';
import { getErrorMessage } from '../../../core/interceptors/error-logging-interceptor.service';
import { BasePagedQuery } from '../../../core/models/paging/base-paged-query';
import { PageResult } from '../../../core/models/paging/page-result';
import { ToasterService } from '../../../core/services/toaster.service';
import { DialogButton } from '../../shared/models/dialog-config.model';
import { DialogHelperService } from '../../shared/services/dialog-helper.service';

/** What every code list row has: the list page needs the id (edit, delete) and the name (messages). */
export interface CodeListItem {
  id: number;
  name: string;
}

/** What every code list request has: paging (BasePagedQuery) and the name filter. */
export interface CodeListRequest extends BasePagedQuery {
  search?: string | null;
}

/**
 * Data passed to an upsert dialog (MAT_DIALOG_DATA). No id → "create" mode, an id → "edit" mode,
 * where the dialog loads the record with getById. The dialog closes with the saved (trimmed) name on
 * success and with nothing on Cancel, so the page knows whether to show a snackbar and reload.
 */
export interface CodeListDialogData {
  id?: number;
}

/**
 * Translation keys of the success snackbars; each one gets {{name}}, e.g.
 * "Proizvođač \"Pliva\" je dodan.".
 */
export interface CodeListMessages {
  created: string;
  updated: string;
  deleted: string;
}

/** Wait this long after the last key press before searching, so typing "pliva" sends one request, not five. */
const SEARCH_DEBOUNCE_MS = 300;

/**
 * Polica (#6): the behavior the three code list pages (manufacturers, dosage forms, write-off reasons)
 * share: loading a page, search, paging, the create/edit dialog and delete with a confirmation.
 *
 * How it fits together:
 *   BaseComponent             isLoading / errorMessage, startLoading() / stopLoading()
 *    └ BaseListComponent      items, initList()
 *       └ BaseListPagedComponent   request.paging, totalItems/totalPages, goToPage(), changePageSize()
 *          └ CodeListPageComponent (this class)  search, dialogs, delete, snackbars
 *             └ ManufacturersComponent, DosageFormsComponent, WriteOffReasonsComponent
 * A page only says WHICH API service and dialog it uses (the abstract members below) and has its own
 * template with its own columns. <app-fit-paginator-bar [vm]="this"> calls the paging methods directly.
 *
 * @Directive() without a selector: Angular needs a decorator on a base class that uses inject(),
 * but this class is never placed in a template itself (same as BaseFormComponent).
 */
@Directive()
export abstract class CodeListPageComponent<TItem extends CodeListItem, TRequest extends CodeListRequest>
  extends BaseListPagedComponent<TItem, TRequest>
  implements OnInit
{
  protected readonly translate = inject(TranslateService);
  private readonly dialog = inject(MatDialog);
  private readonly dialogHelper = inject(DialogHelperService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  /** Text in the search box. request.search gets it only after the pause in typing (see ngOnInit). */
  searchText = '';

  /**
   * false until the first page arrives. Until then the template shows the table skeleton; later loads
   * (paging, search) keep the current rows visible and only the global loading bar runs, so the table
   * does not flicker on every page change.
   */
  hasLoaded = false;

  private readonly searchChanges = new Subject<string>();
  private loadSubscription?: Subscription;

  /** The page's create/edit dialog component (e.g. ManufacturerUpsertComponent). */
  protected abstract readonly upsertDialog: ComponentType<unknown>;

  /** The page's snackbar texts. */
  protected abstract readonly messages: CodeListMessages;

  /** One page from the API, e.g. manufacturersApi.list(request). */
  protected abstract fetchPage(request: TRequest): Observable<PageResult<TItem>>;

  /** Deletes one row through the API, e.g. manufacturersApi.delete(id). */
  protected abstract deleteById(id: number): Observable<void>;

  ngOnInit(): void {
    // Search while typing: wait for a short pause, then search with the trimmed text.
    this.searchChanges
      .pipe(
        map((text) => text.trim()),
        debounceTime(SEARCH_DEBOUNCE_MS),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((search) => this.applySearch(search));

    this.initList(); // → loadData() → loadPagedData()
  }

  protected loadPagedData(): void {
    // A newer load replaces one still running (fast typing, quick paging). Unsubscribing cancels the
    // older HTTP request, so an older response can never overwrite the rows of a newer one.
    this.loadSubscription?.unsubscribe();
    this.startLoading();

    this.loadSubscription = this.fetchPage(this.request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (page) => {
          // stopLoading() first: handlePageResult() may start another load (page no longer exists)
          this.stopLoading();
          this.hasLoaded = true;
          this.handlePageResult(page);
        },
        // the template shows the message with a "Pokušaj ponovo" button
        error: (err: HttpErrorResponse) => this.stopLoading(getErrorMessage(err, this.translate)),
      });
  }

  /** "Pokušaj ponovo" after a failed load. */
  retry(): void {
    this.loadPagedData();
  }

  // === Search ===

  /** (ngModelChange) of the search box. */
  onSearchInput(text: string): void {
    this.searchText = text;
    this.searchChanges.next(text);
  }

  /** The "x" in the search box and the button in the "no results" empty state. */
  clearSearch(): void {
    this.searchText = '';
    // also restart the debounce with '', so a search typed just before the click cannot arrive later
    this.searchChanges.next('');
    this.applySearch('');
  }

  private applySearch(search: string): void {
    // same text as the current filter (e.g. only a space was added) → nothing to reload
    if ((this.request.search ?? '') === search) {
      return;
    }
    this.request.search = search || null; // buildHttpParams leaves null out of the URL
    this.request.paging.page = 1; // a new filter starts on the first page
    this.loadPagedData();
  }

  // === Create / edit ===

  onCreate(): void {
    this.openUpsertDialog({}, this.messages.created);
  }

  onEdit(item: TItem): void {
    this.openUpsertDialog({ id: item.id }, this.messages.updated);
  }

  private openUpsertDialog(data: CodeListDialogData, successKey: string): void {
    this.dialog
      .open<unknown, CodeListDialogData, string>(this.upsertDialog, {
        data,
        width: '520px',
        maxWidth: '95vw',
      })
      .afterClosed()
      .subscribe((savedName) => {
        // undefined = closed with "Otkaži", Esc or a click outside the dialog
        if (!savedName) {
          return;
        }
        this.toaster.success(this.translate.instant(successKey, { name: savedName }));
        this.loadPagedData();
      });
  }

  // === Delete ===

  /** Shared confirm modal first (DialogHelperService.confirmDelete), then DELETE. */
  onDelete(item: TItem): void {
    this.dialogHelper.confirmDelete(item.name).subscribe((result) => {
      if (result?.button !== DialogButton.DELETE) {
        return;
      }

      this.deleteById(item.id)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.toaster.success(this.translate.instant(this.messages.deleted, { name: item.name }));
            // handlePageResult() moves back a page if this was the last row of the last page
            this.loadPagedData();
          },
          error: (err: HttpErrorResponse) => {
            // 403 already got its snackbar from errorLoggingInterceptor
            if (err.status !== 403) {
              // e.g. 404 when someone else deleted it first; from #10/#20 on also 409 "in use"
              this.toaster.error(getErrorMessage(err, this.translate));
            }
            this.loadPagedData();
          },
        });
    });
  }
}
