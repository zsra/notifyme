/**
 * RFC 7807 error shape used by every NotifyMe.Api error response (see
 * docs/api/admin-api.md#error-format). Hand-written rather than generated: the Admin API's
 * endpoints only declare success-response metadata via `.Produces<T>()` (see
 * ai/plan/phase-13-frontend-foundation.md's design notes), so error responses don't appear in
 * the OpenAPI document's schema at all. ProblemDetails itself is a stable, standard shape, so
 * hand-writing it here carries much less drift risk than hand-writing the DTOs would.
 */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  traceId?: string;
}

export async function parseProblemDetails(response: Response): Promise<ProblemDetails | null> {
  const contentType = response.headers.get("content-type") ?? "";
  if (!contentType.includes("json")) {
    return null;
  }

  try {
    const body = (await response.json()) as ProblemDetails;
    return body;
  } catch {
    return null;
  }
}
