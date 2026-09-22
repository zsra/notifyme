import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "./schema";
import { API_BASE_URL } from "./health";

/**
 * A typed Admin API client (see ai/decisions/adr/0009-frontend-stack.md): request/response
 * bodies are checked against `schema.d.ts`, which is generated from the API's own OpenAPI
 * document (`npm run generate:types`), so the frontend's view of the contract cannot silently
 * drift from `docs/api/admin-api.md`/the real API.
 *
 * `onUnauthorized` is called whenever any request comes back `401`, so callers can react (e.g.
 * clear the stored key and send the user back to the API key entry screen) without every
 * individual call site needing to check for it.
 */
export function createAdminApiClient(apiKey: string, onUnauthorized: () => void) {
  const client = createClient<paths>({ baseUrl: API_BASE_URL });

  const authMiddleware: Middleware = {
    onRequest({ request }) {
      request.headers.set("X-Api-Key", apiKey);
      return request;
    },
    onResponse({ response }) {
      if (response.status === 401) {
        onUnauthorized();
      }
      return response;
    },
  };

  client.use(authMiddleware);
  return client;
}

export type AdminApiClient = ReturnType<typeof createAdminApiClient>;

/**
 * An untyped-auth client for the public (`AllowAnonymous`) `/api/auth/*` endpoints - register and
 * login happen before any token exists, so there's nothing to attach to requests yet.
 */
export function createPublicApiClient() {
  return createClient<paths>({ baseUrl: API_BASE_URL });
}

/**
 * A typed client for the end-user self-service `/api/me/*` endpoints (see ADR-0010), bound to
 * the caller's JWT bearer token. `onUnauthorized` mirrors the Admin client's behavior: a `401`
 * (e.g. an expired token) clears the stored session and sends the user back to the login screen.
 */
export function createMyApiClient(token: string, onUnauthorized: () => void) {
  const client = createClient<paths>({ baseUrl: API_BASE_URL });

  const authMiddleware: Middleware = {
    onRequest({ request }) {
      request.headers.set("Authorization", `Bearer ${token}`);
      return request;
    },
    onResponse({ response }) {
      if (response.status === 401) {
        onUnauthorized();
      }
      return response;
    },
  };

  client.use(authMiddleware);
  return client;
}

export type MyApiClient = ReturnType<typeof createMyApiClient>;
