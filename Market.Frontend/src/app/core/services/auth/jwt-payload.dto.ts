/**
 * Claim name the backend actually uses for the email. JwtTokenService (Market.Infrastructure) writes
 * the claim with .NET's ClaimTypes.Email, which is this long URI, not the short JWT name "email".
 */
export const EMAIL_CLAIM_URI = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';

// payload kako dolazi iz JWT-a
export interface JwtPayloadDto {
  sub: string;
  /** short JWT name; the current backend does not send it (see EMAIL_CLAIM_URI) */
  email?: string;
  [EMAIL_CLAIM_URI]?: string;
  is_admin: string;
  is_manager: string;
  is_employee: string;
  ver: string;
  iat: number;
  exp: number;
  aud: string;
  iss: string;
}
