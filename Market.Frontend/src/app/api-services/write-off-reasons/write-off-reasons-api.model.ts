import { BasePagedQuery } from '../../core/models/paging/base-paged-query';
import { PageResult } from '../../core/models/paging/page-result';

/**
 * Polica (#6): types for /api/write-off-reasons (WriteOffReasonsController.cs).
 * A write-off reason explains why stock is removed (expired, damaged packaging...). When requiresNote is
 * true, the pharmacist must also type a note when choosing this reason on a write-off (#20).
 */

// === QUERIES (READ) ===

/** Query parameters for GET /api/write-off-reasons. Corresponds to: ListWriteOffReasonsQuery.cs */
export class ListWriteOffReasonsRequest extends BasePagedQuery {
  search?: string | null;
}

/** One row of the list. Corresponds to: ListWriteOffReasonsQueryDto.cs */
export interface ListWriteOffReasonsQueryDto {
  id: number;
  name: string;
  requiresNote: boolean;
}

/** Paged response of GET /api/write-off-reasons. */
export type ListWriteOffReasonsResponse = PageResult<ListWriteOffReasonsQueryDto>;

/** Response of GET /api/write-off-reasons/{id}. Corresponds to: GetWriteOffReasonByIdQueryDto.cs */
export interface GetWriteOffReasonByIdQueryDto {
  id: number;
  name: string;
  requiresNote: boolean;
}

/**
 * One option of GET /api/write-off-reasons/lookup. Corresponds to: WriteOffReasonLookupQueryDto.cs.
 * requiresNote is included so the write-off form (#20) can make its note field required for this reason.
 */
export interface WriteOffReasonLookupQueryDto {
  id: number;
  name: string;
  requiresNote: boolean;
}

// === COMMANDS (WRITE, administrator only) ===

/** Body of POST /api/write-off-reasons. Corresponds to: CreateWriteOffReasonCommand.cs */
export interface CreateWriteOffReasonCommand {
  name: string;
  requiresNote: boolean;
}

/**
 * Body of PUT /api/write-off-reasons/{id}. Corresponds to: UpdateWriteOffReasonCommand.cs.
 * requiresNote is required in the PUT (a missing value would otherwise silently become false).
 */
export interface UpdateWriteOffReasonCommand {
  name: string;
  requiresNote: boolean;
}
