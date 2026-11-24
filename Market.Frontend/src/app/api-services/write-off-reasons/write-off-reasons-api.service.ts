import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { buildHttpParams } from '../../core/models/build-http-params';
import { CreatedResponse } from '../../core/models/created-response';
import {
  CreateWriteOffReasonCommand,
  GetWriteOffReasonByIdQueryDto,
  ListWriteOffReasonsRequest,
  ListWriteOffReasonsResponse,
  UpdateWriteOffReasonCommand,
  WriteOffReasonLookupQueryDto,
} from './write-off-reasons-api.model';

/**
 * Polica (#6): thin HTTP client for WriteOffReasonsController. Same endpoints and rules as
 * ManufacturersApiService: reads for every logged-in user, writes for the administrator (403 otherwise).
 */
@Injectable({
  providedIn: 'root',
})
export class WriteOffReasonsApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/write-off-reasons`;
  private http = inject(HttpClient);

  /** GET /api/write-off-reasons – one page, sorted by name, optionally filtered by part of the name. */
  list(request: ListWriteOffReasonsRequest): Observable<ListWriteOffReasonsResponse> {
    return this.http.get<ListWriteOffReasonsResponse>(this.baseUrl, {
      params: buildHttpParams(request),
    });
  }

  /** GET /api/write-off-reasons/lookup – all reasons as { id, name, requiresNote } for dropdowns. */
  lookup(): Observable<WriteOffReasonLookupQueryDto[]> {
    return this.http.get<WriteOffReasonLookupQueryDto[]>(`${this.baseUrl}/lookup`);
  }

  /** GET /api/write-off-reasons/{id} – 404 if it does not exist. */
  getById(id: number): Observable<GetWriteOffReasonByIdQueryDto> {
    return this.http.get<GetWriteOffReasonByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

  /** POST /api/write-off-reasons → 201 { id }. A duplicate name → 400 with a field error on "name". */
  create(payload: CreateWriteOffReasonCommand): Observable<CreatedResponse> {
    return this.http.post<CreatedResponse>(this.baseUrl, payload);
  }

  /** PUT /api/write-off-reasons/{id} → 204. */
  update(id: number, payload: UpdateWriteOffReasonCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  /** DELETE /api/write-off-reasons/{id} → 204 (soft delete). */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
