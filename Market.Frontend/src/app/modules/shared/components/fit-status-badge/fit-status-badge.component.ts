import { Component, Input } from '@angular/core';

/**
 * Polica (#5): the five colors a status can have. A feature never picks a hex color itself; it maps
 * its own status to one of these tones, so the same meaning has the same color on every screen.
 *
 *   neutral – nothing to do yet or no longer relevant   (PO Draft, alert acknowledged)
 *   info    – in progress, waiting for someone           (PO Sent, transfer Requested / Shipped)
 *   success – finished well, or OK                       (PO Received, Active, transfer Received)
 *   warning – needs attention soon                       (PO PartiallyReceived, alert First)
 *   danger  – stopped, rejected or critical              (Cancelled, Rejected, alert Second / Expired)
 */
export type FitStatusTone = 'neutral' | 'info' | 'success' | 'warning' | 'danger';

/**
 * Polica (#5): status badge, a small colored "pill" for a status in tables and detail pages
 * (ui-patterns.com: clear status indicators). Declared and exported by SharedModule, so every lazy
 * feature module that imports SharedModule can use it.
 *
 * The text comes from the content between the tags (content projection, <ng-content>), so it can be
 * a translation or a computed value:
 *
 *   <app-fit-status-badge tone="success" icon="check_circle">{{ 'COMMON.ACTIVE' | translate }}</app-fit-status-badge>
 *   <app-fit-status-badge [tone]="toneFor(order.status)">{{ 'PO.STATUS.' + order.status | translate }}</app-fit-status-badge>
 *
 * The component has no logic of its own: the feature component decides the tone (e.g. a toneFor()
 * method with a switch over its status enum). Colors are in fit-status-badge.component.scss.
 */
@Component({
  selector: 'app-fit-status-badge',
  standalone: false,
  templateUrl: './fit-status-badge.component.html',
  styleUrl: './fit-status-badge.component.scss',
})
export class FitStatusBadgeComponent {
  /** Color of the badge; the CSS class "tone-<tone>" in the template selects it. */
  @Input() tone: FitStatusTone = 'neutral';

  /** Optional Material icon name shown before the text, e.g. "schedule" or "block". */
  @Input() icon?: string;
}
