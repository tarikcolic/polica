/**
 * Polica (#5): calendar dates without time, between mat-datepicker and the API.
 *
 * The backend stores calendar dates as C# DateOnly (SQL `date`), e.g. ExpiryDate and ManufactureDate
 * of a batch (#13). System.Text.Json sends and expects a DateOnly as the text "yyyy-MM-dd", with no
 * time and no time zone:
 *
 *   picked in the datepicker 15.10.2026. → "2026-10-15" → DateOnly 2026-10-15 in the database
 *
 * There is no time zone in that text, so the day can never shift by one (that is what utc-date.ts has to
 * guard against for DateTime fields).
 *
 * Display: the DatePipe reads "2026-10-15" as that day in local time, so a template writes just
 * {{ batch.expiryDate | date }} → 15.10.2026. (default format from app-module.ts).
 *
 * Usage in a form component:
 *   expiryDate: toDateOnly(this.form.controls.expiryDate.value)        // form → API request
 *   this.form.controls.expiryDate.setValue(fromDateOnly(dto.expiryDate)) // API response → form
 */

/** Date from the datepicker → "yyyy-MM-dd" (the day as the user sees it). null stays null. */
export function toDateOnly(date: Date | null | undefined): string | null {
  if (!date) {
    return null;
  }
  // getFullYear/getMonth/getDate read the LOCAL day: the datepicker returns local midnight of the picked day
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0'); // getMonth() is 0-based
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/** "yyyy-MM-dd" from the API → Date at local midnight of that day, for the datepicker. */
export function fromDateOnly(value: string | null | undefined): Date | null {
  if (!value) {
    return null;
  }
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  if (!match) {
    return null;
  }
  // new Date(y, m, d) creates LOCAL midnight; new Date("2026-10-15") would be UTC midnight,
  // which is still 14.10. in time zones west of UTC
  return new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3]));
}
