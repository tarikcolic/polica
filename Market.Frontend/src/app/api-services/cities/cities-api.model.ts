/**
 * Polica (#7): types for /api/cities (CitiesController.cs). Cities are seeded on the backend and read-only,
 * so the only type is the dropdown option.
 */

/**
 * One option of GET /api/cities/lookup (all cities, not paged).
 * Corresponds to: CityLookupQueryDto.cs. Used by the pharmacy form and the pharmacy list's city filter (#7),
 * later by the supplier form (#9).
 */
export interface CityLookupQueryDto {
  id: number;
  name: string;
}
