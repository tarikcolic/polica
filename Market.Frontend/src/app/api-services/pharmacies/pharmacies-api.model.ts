import { BasePagedQuery } from '../../core/models/paging/base-paged-query';
import { PageResult } from '../../core/models/paging/page-result';

/**
 * Polica (#7): types for /api/pharmacies (PharmaciesController.cs).
 * Each type mirrors one backend class; the comment names it, so a change on one side is easy to find on the other.
 */

// === QUERIES (READ) ===

/**
 * Query parameters for GET /api/pharmacies (administrator).
 * Corresponds to: ListPharmaciesQuery.cs (BasePagedQuery + three optional filters).
 * buildHttpParams() turns it into ?paging.page=1&paging.pageSize=10&search=ap&cityId=1&isActive=true
 * and leaves out every filter that is null (= "all").
 */
export class ListPharmaciesRequest extends BasePagedQuery {
  /** part of the name or of the code */
  search?: string | null;
  cityId?: number | null;
  /** true = only active, false = only inactive, null = all */
  isActive?: boolean | null;
}

/** One row of the list. Corresponds to: ListPharmaciesQueryDto.cs */
export interface ListPharmaciesQueryDto {
  id: number;
  code: string;
  name: string;
  address: string;
  phone: string;
  cityName: string;
  isActive: boolean;
  /** derived on the backend (user with the manager role + PharmacyId); null until #8 → shown as "–" */
  managerName?: string | null;
}

/** Paged response of GET /api/pharmacies. */
export type ListPharmaciesResponse = PageResult<ListPharmaciesQueryDto>;

/** Response of GET /api/pharmacies/{id}. Corresponds to: GetPharmacyByIdQueryDto.cs */
export interface GetPharmacyByIdQueryDto {
  id: number;
  code: string;
  name: string;
  address: string;
  phone: string;
  cityId: number;
  cityName: string;
  isActive: boolean;
}

/**
 * One option of GET /api/pharmacies/lookup: ACTIVE pharmacies only (the backend filters, so an inactive pharmacy
 * never reaches a dropdown). Corresponds to: PharmacyLookupQueryDto.cs. First used by the employee form in #8.
 */
export interface PharmacyLookupQueryDto {
  id: number;
  code: string;
  name: string;
}

// === COMMANDS (WRITE, administrator only) ===

/**
 * Body of POST /api/pharmacies. Corresponds to: CreatePharmacyCommand.cs.
 * No isActive: a new pharmacy is always active; activate()/deactivate() change it.
 */
export interface CreatePharmacyCommand {
  name: string;
  /** stored in uppercase by the backend */
  code: string;
  address: string;
  phone: string;
  cityId: number;
}

/**
 * Body of PUT /api/pharmacies/{id}. Corresponds to: UpdatePharmacyCommand.cs.
 * The Id travels in the URL only; isActive is not part of it (see the backend command's remarks).
 */
export interface UpdatePharmacyCommand {
  name: string;
  code: string;
  address: string;
  phone: string;
  cityId: number;
}
