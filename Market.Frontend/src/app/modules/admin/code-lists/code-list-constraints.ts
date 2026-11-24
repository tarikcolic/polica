/**
 * Polica (#6): maximum field lengths of the code lists, the same numbers as the backend's
 * ManufacturerEntity / DosageFormEntity / WriteOffReasonEntity.Constraints (and the database columns).
 *
 * The upsert dialogs use them twice: as Validators.maxLength (message under the field) and as the
 * input's maxlength attribute (the browser stops typing at the limit), plus the "12/100" counter.
 * The backend validators check the same limits again, because the API can be called without this UI.
 */
export const CODE_LIST_CONSTRAINTS = {
  manufacturerNameMaxLength: 100,
  manufacturerCountryMaxLength: 100,
  dosageFormNameMaxLength: 50,
  writeOffReasonNameMaxLength: 100,
} as const;
