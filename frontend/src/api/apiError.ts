import { parseProblemDetails } from "./problemDetails";

/**
 * Thrown by `unwrap` whenever an Admin API call comes back non-2xx. Carries the parsed
 * `ProblemDetails.detail` (see docs/api/admin-api.md) as the error message so UI code can just
 * render `error.message` without knowing about the response envelope.
 */
export class ApiError extends Error {}

/**
 * Every `openapi-fetch` call resolves to `{ data, error, response }` rather than throwing. This
 * normalizes that into "throws on failure", which plays more naturally with TanStack Query's
 * mutation/query error handling than checking `error`/`response.ok` at every call site.
 */
export async function unwrap<T>(result: { data?: T; response: Response }): Promise<T> {
  if (!result.response.ok) {
    const problem = await parseProblemDetails(result.response);
    throw new ApiError(problem?.detail ?? `Request failed with status ${result.response.status}`);
  }
  return result.data as T;
}
