/**
 * The base URL the Admin API is reachable at during local development. Not a secret - just the
 * dev server's own address - so a build-time env var with a sane default is enough; no runtime
 * configuration screen is needed for this value (contrast with the API key, which does need one,
 * see src/auth/ApiKeyContext.tsx).
 */
export const API_BASE_URL: string = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5062";

export type HealthStatus = "Healthy" | "Degraded" | "Unhealthy" | "Unreachable";

/**
 * `/health` (see docs/api/admin-api.md#health) is intentionally outside the OpenAPI document (it
 * isn't a minimal-API endpoint with `.Produces<T>()` metadata, just ASP.NET Core's health checks
 * middleware), so it isn't part of the generated `paths` type and is called with a plain
 * `fetch` here instead of through the typed Admin API client.
 */
export async function fetchHealth(): Promise<HealthStatus> {
  try {
    const response = await fetch(`${API_BASE_URL}/health`);
    const text = (await response.text()).trim();
    if (text === "Healthy" || text === "Degraded" || text === "Unhealthy") {
      return text;
    }
    return "Unreachable";
  } catch {
    return "Unreachable";
  }
}
