import type { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useMyAuth } from "./MyAuthContext";

/**
 * Blocks rendering of `/my/*` screens until an end-user session exists (see MyAuthContext),
 * redirecting to `/login` instead. A `401` response from any `/api/me/*` request clears the
 * stored session (src/api/client.ts's `createMyApiClient`), which naturally routes back here on
 * the next render.
 */
export function MyAuthGate({ children }: { children: ReactNode }) {
  const { token } = useMyAuth();

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}
