import { Component, inject } from '@angular/core';
import { asapScheduler, observeOn } from 'rxjs';
import {LoadingBarService} from '../../../../core/services/loading-bar.service';

@Component({
  selector: 'app-fit-loading-bar',
  standalone: false,
  templateUrl: './fit-loading-bar.component.html',
  styleUrl: './fit-loading-bar.component.scss',
})
export class FitLoadingBarComponent {
  /**
   * Inject LoadingBarService to access loading state
   * Component subscribes to loading$ observable to show/hide bar
   */
  private loadingBar = inject(LoadingBarService);

  /**
   * Polica (#6): the service's loading state, delivered one microtask later (asapScheduler).
   *
   * Why: an HTTP request can start WHILE Angular is checking the page, e.g. a dialog that loads its record
   * in ngOnInit (the code list edit dialogs). This bar sits in app.component.html and was already checked
   * in that pass, so a change from false to true in the same pass is an
   * ExpressionChangedAfterItHasBeenCheckedError (NG0100), reported by the development build.
   * One microtask later the check has finished; zone.js then runs a new check that shows the bar.
   * The delay is far too short to see.
   */
  protected readonly loading$ = this.loadingBar.loading$.pipe(observeOn(asapScheduler));
}
