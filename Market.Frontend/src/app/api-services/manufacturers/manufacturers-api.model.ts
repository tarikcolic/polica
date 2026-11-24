import { BasePagedQuery } from '../../core/models/paging/base-paged-query';
import { PageResult } from '../../core/models/paging/page-result';

/**
 * Polica (#6): types for /api/manufacturers (ManufacturersController.cs).
 * Each type mirrors one backend class; the comment names it, so a change on one side is easy to find
 * on the other. Same layout as the starter's api-services/readme.md describes.
 */

// === QUERIES (READ) ===

/**
 * Query parameters for GET /api/manufacturers.
 * Corresponds to: ListManufacturersQuery.cs (BasePagedQuery + Search).
 * buildHttpParams() turns it into ?paging.page=1&paging.pageSize=10&search=pli
 * and leaves out an empty search.
 */
export class ListManufacturersRequest extends BasePagedQuery {
  /** part of the name; the backend matches it anywhere in the name */
  search?: string | null;
}

/** One row of the list. Corresponds to: ListManufacturersQueryDto.cs */
export interface ListManufacturersQueryDto {
  id: number;
  name: string;
  country?: string | null;
}

/** Paged response of GET /api/manufacturers. */
export type ListManufacturersResponse = PageResult<ListManufacturersQueryDto>;

/** Response of GET /api/manufacturers/{id}. Corresponds to: GetManufacturerByIdQueryDto.cs */
export interface GetManufacturerByIdQueryDto {
  id: number;
  name: string;
  country?: string | null;
}

/**
 * One option of GET /api/manufacturers/lookup (all manufacturers, not paged).
 * Corresponds to: ManufacturerLookupQueryDto.cs. Used by the medication form's dropdown in #10.
 */
export interface ManufacturerLookupQueryDto {
  id: number;
  name: string;
}

// === COMMANDS (WRITE, administrator only) ===

/** Body of POST /api/manufacturers. Corresponds to: CreateManufacturerCommand.cs */
export interface CreateManufacturerCommand {
  name: string;
  country?: string | null;
}

/**
 * Body of PUT /api/manufacturers/{id}. Corresponds to: UpdateManufacturerCommand.cs.
 * The Id travels in the URL only (the backend ignores an Id in the body).
 */
export interface UpdateManufacturerCommand {
  name: string;
  country?: string | null;
}
