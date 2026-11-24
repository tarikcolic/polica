import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { buildHttpParams } from '../../core/models/build-http-params';
import { CreatedResponse } from '../../core/models/created-response';
import {
  CreateDosageFormCommand,
  DosageFormLookupQueryDto,
  GetDosageFormByIdQueryDto,
  ListDosageFormsRequest,
  ListDosageFormsResponse,
  UpdateDosageFormCommand,
} from './dosage-forms-api.model';

/**
 * Polica (#6): thin HTTP client for DosageFormsController. Same endpoints and rules as
 * ManufacturersApiService: reads for every logged-in user, writes for the administrator (403 otherwise).
 */
@Injectable({
  providedIn: 'root',
})
export class DosageFormsApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/dosage-forms`;
  private http = inject(HttpClient);

  /** GET /api/dosage-forms – one page, sorted by name, optionally filtered by part of the name. */
  list(request: ListDosageFormsRequest): Observable<ListDosageFormsResponse> {
    return this.http.get<ListDosageFormsResponse>(this.baseUrl, {
      params: buildHttpParams(request),
    });
  }

  /** GET /api/dosage-forms/lookup – all dosage forms as { id, name } for dropdowns. */
  lookup(): Observable<DosageFormLookupQueryDto[]> {
    return this.http.get<DosageFormLookupQueryDto[]>(`${this.baseUrl}/lookup`);
  }

  /** GET /api/dosage-forms/{id} – 404 if it does not exist. */
  getById(id: number): Observable<GetDosageFormByIdQueryDto> {
    return this.http.get<GetDosageFormByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

  /** POST /api/dosage-forms → 201 { id }. A duplicate name → 400 with a field error on "name". */
  create(payload: CreateDosageFormCommand): Observable<CreatedResponse> {
    return this.http.post<CreatedResponse>(this.baseUrl, payload);
  }

  /** PUT /api/dosage-forms/{id} → 204. */
  update(id: number, payload: UpdateDosageFormCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  /** DELETE /api/dosage-forms/{id} → 204 (soft delete). */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
