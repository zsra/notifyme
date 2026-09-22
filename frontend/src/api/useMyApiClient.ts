import { useMemo } from "react";
import { useMyAuth } from "../auth/MyAuthContext";
import { createMyApiClient, type MyApiClient } from "./client";

/**
 * Returns a memoized, typed `/api/me/*` client bound to the current end-user session, or `null`
 * if no session exists yet (callers should gate their queries/mutations on this, mirroring
 * `useAdminApiClient`).
 */
export function useMyApiClient(): MyApiClient | null {
  const { token, logout } = useMyAuth();

  return useMemo(() => {
    if (!token) {
      return null;
    }
    return createMyApiClient(token, logout);
  }, [token, logout]);
}
