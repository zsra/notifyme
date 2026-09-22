import { useMemo } from "react";
import { useApiKey } from "../auth/ApiKeyContext";
import { createAdminApiClient, type AdminApiClient } from "./client";

/**
 * Returns a memoized, typed Admin API client bound to the current API key, or `null` if no key
 * is set yet (callers should gate their queries/mutations on this, e.g. via TanStack Query's
 * `enabled` option).
 */
export function useAdminApiClient(): AdminApiClient | null {
  const { apiKey, clearApiKey } = useApiKey();

  return useMemo(() => {
    if (!apiKey) {
      return null;
    }
    return createAdminApiClient(apiKey, clearApiKey);
  }, [apiKey, clearApiKey]);
}
