import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { buildHttpParams } from '../../core/models/build-http-params';
import { CreatedResponse } from '../../core/models/created-response';
import {
  CreatePharmacyCommand,
  GetPharmacyByIdQueryDto,
  ListPharmaciesRequest,
  ListPharmaciesResponse,
  PharmacyLookupQueryDto,
  UpdatePharmacyCommand,
} from './pharmacies-api.model';

/**
 * Polica (#7): thin HTTP client for PharmaciesController (one method per endpoint, no state, no UI logic;
 * see api-services/readme.md).
 *
 * list/create/update/activate/deactivate need the administrator: for anyone else the backend answers 403 and
 * errorLoggingInterceptor shows "Nemate dozvolu za ovu akciju." by itself. lookup() works for every logged-in
 * user. The access token is added by authInterceptor.
 */
@Injectable({
  providedIn: 'root',
})
export class PharmaciesApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/pharmacies`;
  private http = inject(HttpClient);

  /** GET /api/pharmacies – one page, sorted by code, with the optional filters of the request. */
  list(request: ListPharmaciesRequest): Observable<ListPharmaciesResponse> {
    return this.http.get<ListPharmaciesResponse>(this.baseUrl, {
      params: buildHttpParams(request),
    });
  }

  /** GET /api/pharmacies/lookup – ACTIVE pharmacies as { id, code, name } for dropdowns. */
  lookup(): Observable<PharmacyLookupQueryDto[]> {
    return this.http.get<PharmacyLookupQueryDto[]>(`${this.baseUrl}/lookup`);
  }

  /** GET /api/pharmacies/{id} – 404 if it does not exist, 403 if the user may not see it. */
  getById(id: number): Observable<GetPharmacyByIdQueryDto> {
    return this.http.get<GetPharmacyByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

  /** POST /api/pharmacies → 201 { id }. A duplicate code → 400 with a field error on "code". */
  create(payload: CreatePharmacyCommand): Observable<CreatedResponse> {
    return this.http.post<CreatedResponse>(this.baseUrl, payload);
  }

  /** PUT /api/pharmacies/{id} → 204. */
  update(id: number, payload: UpdatePharmacyCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  /** POST /api/pharmacies/{id}/activate → 204 (no body). */
  activate(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/activate`, null);
  }

  /** POST /api/pharmacies/{id}/deactivate → 204 (no body). From #13 on: 409 while the pharmacy holds stock. */
  deactivate(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/deactivate`, null);
  }
}
