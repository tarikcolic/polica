import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TranslateService } from '@ngx-translate/core';
import { debounceTime, map, Observable, Subject, Subscription } from 'rxjs';
import { CityLookupQueryDto } from '../../../../api-services/cities/cities-api.model';
import { CitiesApiService } from '../../../../api-services/cities/cities-api.service';
import {
  ListPharmaciesQueryDto,
  ListPharmaciesRequest,
} from '../../../../api-services/pharmacies/pharmacies-api.model';
import { PharmaciesApiService } from '../../../../api-services/pharmacies/pharmacies-api.service';
import { BaseListPagedComponent } from '../../../../core/components/base-classes/base-list-paged-component';
import { getErrorMessage } from '../../../../core/interceptors/error-logging-interceptor.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { DialogHelperService } from '../../../shared/services/dialog-helper.service';

/** Values of the status filter; mapped to request.isActive (null / true / false) in onStatusChange(). */
export type PharmacyStatusFilter = 'all' | 'active' | 'inactive';

/** Wait this long after the last key press before searching (same as the code lists). */
const SEARCH_DEBOUNCE_MS = 300;

/**
 * Polica (#7): /admin/pharmacies, the pharmacy registry (administrator only).
 *
 * How it fits together:
 *   BaseComponent → BaseListComponent → BaseListPagedComponent (request.paging, goToPage(), changePageSize())
 *    └ PharmaciesComponent (this class): filters, loading a page, activate/deactivate
 * <app-fit-paginator-bar [vm]="this"> calls the paging methods of BaseListPagedComponent directly.
 * Create and edit are separate pages (PharmacyUpsertComponent), reached with routerLink from the template.
 *
 * The search-while-typing logic is the same as in CodeListPageComponent (#6); it is repeated here instead of
 * extending that class, because the code list base class also brings dialogs and delete, which pharmacies
 * do not have.
 */
@Component({
  selector: 'app-pharmacies',
  standalone: false,
  templateUrl: './pharmacies.component.html',
  styleUrl: './pharmacies.component.scss',
})
export class PharmaciesComponent
  extends BaseListPagedComponent<ListPharmaciesQueryDto, ListPharmaciesRequest>
  implements OnInit
{
  private readonly api = inject(PharmaciesApiService);
  private readonly citiesApi = inject(CitiesApiService);
  private readonly dialogHelper = inject(DialogHelperService);
  private readonly toaster = inject(ToasterService);
  private readonly translate = inject(TranslateService);
  private readonly destroyRef = inject(DestroyRef);

  /** Column order of the mat-table (matColumnDef names in the template). */
  readonly displayedColumns = ['code', 'name', 'city', 'address', 'phone', 'manager', 'status', 'actions'];

  /** Options of the city filter (GET /api/cities/lookup). */
  cities: CityLookupQueryDto[] = [];

  /** Text in the search box. request.search gets it only after the pause in typing (see ngOnInit). */
  searchText = '';

  /** Selected option of the status filter. */
  statusFilter: PharmacyStatusFilter = 'all';

  /** false until the first page arrives: the template shows the table skeleton instead of an empty table. */
  hasLoaded = false;

  private readonly searchChanges = new Subject<string>();
  private loadSubscription?: Subscription;

  constructor() {
    super();
    // BaseListPagedComponent reads paging from this.request (page 1, 10 rows); no filters = all pharmacies
    this.request = new ListPharmaciesRequest();
  }

  ngOnInit(): void {
    // Search while typing: wait for a short pause, then search with the trimmed text.
    this.searchChanges
      .pipe(
        map((text) => text.trim()),
        debounceTime(SEARCH_DEBOUNCE_MS),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((search) => this.applySearch(search));

    // City options for the filter. If this request fails, errorLoggingInterceptor already shows the error
    // and the filter simply has no options; the list itself still works.
    this.citiesApi
      .lookup()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((cities) => (this.cities = cities));

    this.initList(); // → loadData() → loadPagedData()
  }

  protected loadPagedData(): void {
    // A newer load replaces one still running (fast typing, quick paging): unsubscribing cancels the older
    // HTTP request, so an older response can never overwrite the rows of a newer one.
    this.loadSubscription?.unsubscribe();
    this.startLoading();

    this.loadSubscription = this.api
      .list(this.request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (page) => {
          // stopLoading() first: handlePageResult() may start another load (the page no longer exists)
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

  // === Filters ===

  /** true when any filter is set; the empty state then offers "clear filters" instead of "add". */
  get hasFilters(): boolean {
    return !!this.request.search || this.request.cityId != null || this.request.isActive != null;
  }

  /** (ngModelChange) of the search box. */
  onSearchInput(text: string): void {
    this.searchText = text;
    this.searchChanges.next(text);
  }

  /** The "x" in the search box. */
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
    this.reloadFromFirstPage();
  }

  /** (selectionChange) of the city filter; null = "all cities". */
  onCityChange(cityId: number | null): void {
    this.request.cityId = cityId;
    this.reloadFromFirstPage();
  }

  /** (selectionChange) of the status filter. */
  onStatusChange(status: PharmacyStatusFilter): void {
    this.statusFilter = status;
    // 'all' → null (the backend applies no filter), otherwise true / false
    this.request.isActive = status === 'all' ? null : status === 'active';
    this.reloadFromFirstPage();
  }

  /** Button in the "no results" empty state: removes every filter at once. */
  clearFilters(): void {
    this.searchText = '';
    this.searchChanges.next('');
    this.statusFilter = 'all';
    this.request.search = null;
    this.request.cityId = null;
    this.request.isActive = null;
    this.reloadFromFirstPage();
  }

  /** A changed filter starts on the first page (page 3 of the old result may not exist in the new one). */
  private reloadFromFirstPage(): void {
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  // === Activate / deactivate ===

  /**
   * Confirm modal first (DialogHelperService.confirmAction, the shared modal from #5), then the API call.
   * Deactivation gets the red ("warn") button: the pharmacy disappears from every dropdown.
   */
  onToggleActive(pharmacy: ListPharmaciesQueryDto): void {
    const deactivate = pharmacy.isActive;
    const keys = deactivate ? 'PHARMACIES.DEACTIVATE' : 'PHARMACIES.ACTIVATE';
    const params = { code: pharmacy.code, name: pharmacy.name };

    this.dialogHelper
      .confirmAction(`${keys}.TITLE`, `${keys}.MESSAGE`, `${keys}.CONFIRM`, params, deactivate ? 'warn' : 'primary')
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        const request$: Observable<void> = deactivate
          ? this.api.deactivate(pharmacy.id)
          : this.api.activate(pharmacy.id);

        request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
          next: () => {
            this.toaster.success(this.translate.instant(`${keys}.SUCCESS`, params));
            this.loadPagedData(); // the badge changes; with a status filter the row may leave the page
          },
          error: (err: HttpErrorResponse) => {
            // 403 already got its snackbar from errorLoggingInterceptor; from #13 on also 409 "has stock"
            if (err.status !== 403) {
              this.toaster.error(getErrorMessage(err, this.translate));
            }
            this.loadPagedData();
          },
        });
      });
  }
}
