/**
 * Polica (#7): field limits and patterns of a pharmacy, the same as the backend's PharmacyEntity.Constraints
 * and the patterns in CreatePharmacyCommandValidator (and the database column sizes).
 *
 * The form uses them twice: as validators (message under the field) and as the input's maxlength attribute
 * (the browser stops typing at the limit). The backend checks the same rules again, because the API can be
 * called without this UI.
 */
export const PHARMACY_CONSTRAINTS = {
  nameMaxLength: 100,
  codeMaxLength: 10,
  addressMaxLength: 200,
  phoneMaxLength: 30,
  /** letters A-Z (either case, stored in uppercase), digits and "-", e.g. AP-01 */
  codePattern: /^[A-Za-z0-9-]+$/,
  /** digits, spaces and + - / ( ), e.g. +387 33 123 456 */
  phonePattern: /^[0-9+()/ -]+$/,
} as const;
