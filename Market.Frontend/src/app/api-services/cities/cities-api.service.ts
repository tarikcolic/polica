import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CityLookupQueryDto } from './cities-api.model';

/**
 * Polica (#7): thin HTTP client for CitiesController (see api-services/readme.md). Read-only: the cities come
 * with the database migration and have no admin screen. Works for every logged-in user.
 */
@Injectable({
  providedIn: 'root',
})
export class CitiesApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/cities`;
  private http = inject(HttpClient);

  /** GET /api/cities/lookup – all cities as { id, name }, sorted by name, for dropdowns. */
  lookup(): Observable<CityLookupQueryDto[]> {
    return this.http.get<CityLookupQueryDto[]>(`${this.baseUrl}/lookup`);
  }
}
