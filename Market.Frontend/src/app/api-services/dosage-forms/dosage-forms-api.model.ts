import { BasePagedQuery } from '../../core/models/paging/base-paged-query';
import { PageResult } from '../../core/models/paging/page-result';

/**
 * Polica (#6): types for /api/dosage-forms (DosageFormsController.cs).
 * A dosage form is the physical form of a medication (Tableta, Kapsula, Sirup...); it only has a name.
 * The five basic forms come with the migration (StaticDataSeeder), the administrator can add more.
 */

// === QUERIES (READ) ===

/** Query parameters for GET /api/dosage-forms. Corresponds to: ListDosageFormsQuery.cs */
export class ListDosageFormsRequest extends BasePagedQuery {
  search?: string | null;
}

/** One row of the list. Corresponds to: ListDosageFormsQueryDto.cs */
export interface ListDosageFormsQueryDto {
  id: number;
  name: string;
}

/** Paged response of GET /api/dosage-forms. */
export type ListDosageFormsResponse = PageResult<ListDosageFormsQueryDto>;

/** Response of GET /api/dosage-forms/{id}. Corresponds to: GetDosageFormByIdQueryDto.cs */
export interface GetDosageFormByIdQueryDto {
  id: number;
  name: string;
}

/** One option of GET /api/dosage-forms/lookup. Corresponds to: DosageFormLookupQueryDto.cs */
export interface DosageFormLookupQueryDto {
  id: number;
  name: string;
}

// === COMMANDS (WRITE, administrator only) ===

/** Body of POST /api/dosage-forms. Corresponds to: CreateDosageFormCommand.cs */
export interface CreateDosageFormCommand {
  name: string;
}

/** Body of PUT /api/dosage-forms/{id}. Corresponds to: UpdateDosageFormCommand.cs */
export interface UpdateDosageFormCommand {
  name: string;
}
