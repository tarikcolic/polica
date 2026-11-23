import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { catchError, throwError } from 'rxjs';
import { ToasterService } from '../services/toaster.service';

/**
 * HTTP interceptor that logs errors.
 *
 * IMPORTANT: This interceptor shows a toaster ONLY for 403 (no permission) – that message is
 * the same everywhere, so components do not need to handle it. All other toaster/form
 * messages should be handled in individual components where you can provide
 * context-specific error messages (see getErrorMessage below and core/forms/server-errors.ts).
 *
 * Features:
 * - Logs HTTP errors to console (except 401 – see below)
 * - 401 is handled by authInterceptor (token refresh, or logout + redirect to /auth/login);
 *   the login form shows its own 401 message ("Pogrešan email ili lozinka.")
 * - 403 → translated toaster "You do not have permission for this action."
 *   (checked by status – a 403 produced by the ASP.NET policy may have an empty body)
 * - Can be extended to send errors to logging service (Sentry, etc.)
 * - Re-throws errors so components can handle them
 */
export const errorLoggingInterceptor: HttpInterceptorFn = (req, next) => {
  // ToasterService/TranslateService uzimamo tek kad zatrebaju, ne odmah:
  // TranslateService i sam koristi HttpClient (učitava /i18n/*.json), pa bi ga rani
  // inject() u interceptoru mogao uvesti u kružnu zavisnost (NG0200).
  const injector = inject(Injector);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        return throwError(() => error);
      }

      if (error.status !== 401) {
        // Log error to console
        // In production, send to logging service like Sentry
        console.error('HTTP Error:', {
          url: req.url,
          method: req.method,
          status: error.status,
          statusText: error.statusText,
          message: error.message,
          error: error.error,
          timestamp: new Date().toISOString()
        });
      }

      if (error.status === 403) {
        const translate = injector.get(TranslateService);
        injector.get(ToasterService).error(translate.instant('ERRORS.FORBIDDEN'));
      }

      // Optional: Send to external logging service
      // logToSentry(error, req);

      // Re-throw error so components can handle it
      // Components should show appropriate toaster messages with context
      return throwError(() => error);
    })
  );
};

/**
 * Optional: Send errors to external logging service
 * Uncomment and implement when using Sentry, LogRocket, etc.
 */
// function logToSentry(error: HttpErrorResponse, req: HttpRequest<any>): void {
//   if (environment.production) {
//     Sentry.captureException(error, {
//       extra: {
//         url: req.url,
//         method: req.method,
//         status: error.status,
//         body: req.body
//       }
//     });
//   }
// }

/**
 * Helper: Get user-friendly error message
 * Can be used by components to extract error messages
 *
 * Polica (#5): when the server sends no message of its own, the text now comes from the ERRORS.*
 * translation keys (bs.json / en.json), in the language the user picked. The starter returned
 * hard-coded English here, so e.g. the Bosnian login page showed "Unable to connect to server..."
 * when the API was not running – two languages on one screen.
 *
 * The caller passes its TranslateService because this is a plain function, not a service:
 * inject() only works while Angular creates a class, not later inside a subscribe() callback.
 *   error: (err) => this.stopLoading(getErrorMessage(err, this.translate))
 */
export function getErrorMessage(error: HttpErrorResponse, translate: TranslateService): string {
  // Check if error has a custom message from backend (ErrorDto.message)
  if (typeof error.error?.message === 'string' && error.error.message) {
    return error.error.message;
  }

  // Check for validation errors
  const errors = error.error?.errors;
  if (Array.isArray(errors)) {
    // ErrorDto.errors: [{ field, message }]
    const messages = errors
      .map((e: { message?: string }) => e?.message)
      .filter((m): m is string => !!m);
    if (messages.length > 0) {
      return messages.join(' ');
    }
  } else if (errors && typeof errors === 'object') {
    // ProblemDetails: { field: string[] }
    const messages = Object.values(errors).flat();
    if (messages.length > 0) {
      return messages.join(', ');
    }
  }

  // Polica (#5): the ProblemDetails "title" is no longer shown. ASP.NET writes it in English
  // ("Not Found", "Unsupported Media Type"), so the translated text for the status code is used instead.

  // Fallback to generic messages based on status code
  return translate.instant(statusMessageKey(error.status));
}

/**
 * Polica (#5): HTTP status → translation key of a general message (all keys exist in bs.json and en.json).
 * Status 0 means the browser got no response at all: the API is not running, or there is no network.
 */
function statusMessageKey(status: number): string {
  switch (status) {
    case 0:
      return 'ERRORS.NETWORK';
    case 400:
      return 'ERRORS.BAD_REQUEST';
    case 401:
      return 'ERRORS.LOGIN_REQUIRED';
    case 403:
      return 'ERRORS.FORBIDDEN';
    case 404:
      return 'ERRORS.NOT_FOUND';
    case 409:
      return 'ERRORS.CONFLICT';
    case 503:
      return 'ERRORS.SERVICE_UNAVAILABLE';
    default:
      // any other 5xx is a server problem; everything else is unexpected
      return status >= 500 ? 'ERRORS.SERVER_ERROR' : 'ERRORS.UNKNOWN';
  }
}
