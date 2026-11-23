import { Component, Input } from '@angular/core';

/**
 * Polica (#5): empty state for lists ("blank slate" on ui-patterns.com). Shown INSTEAD of the table
 * when a page of results has no rows, so the user sees why the list is empty and what to do next,
 * not an empty table header. Declared and exported by SharedModule.
 *
 * Texts are translation keys (like DialogConfig.titleKey in the confirm dialog). The defaults cover the
 * two common cases; a feature passes its own keys when it has a better message:
 *   - nothing created yet     → EMPTY_STATE.NO_DATA_*     (+ a "create" button)
 *   - filters match nothing   → EMPTY_STATE.NO_RESULTS_*  (+ a "clear filters" button)
 *
 * The action button is projected between the tags (<ng-content>), so the feature keeps its own
 * click handler and permission check:
 *
 *   @if (rows.length === 0) {
 *     <app-fit-empty-state icon="local_shipping"
 *                          titleKey="SUPPLIERS.EMPTY.TITLE" messageKey="SUPPLIERS.EMPTY.MESSAGE">
 *       <button mat-flat-button (click)="create()">{{ 'SUPPLIERS.ADD' | translate }}</button>
 *     </app-fit-empty-state>
 *   }
 *
 * While the list is still loading, show <app-fit-table-skeleton> instead; the empty state is only for
 * a response that really has no rows.
 */
@Component({
  selector: 'app-fit-empty-state',
  standalone: false,
  templateUrl: './fit-empty-state.component.html',
  styleUrl: './fit-empty-state.component.scss',
})
export class FitEmptyStateComponent {
  /** Material icon name shown in the circle above the title. */
  @Input() icon = 'inbox';

  /** Translation key of the title (bs.json / en.json). */
  @Input() titleKey = 'EMPTY_STATE.NO_DATA_TITLE';

  /** Translation key of the explanation under the title; set it to '' to show no message. */
  @Input() messageKey = 'EMPTY_STATE.NO_DATA_MESSAGE';
}
