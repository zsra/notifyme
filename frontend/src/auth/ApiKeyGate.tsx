import type { ReactNode } from "react";
import { useApiKey } from "./ApiKeyContext";
import { ApiKeyPrompt } from "./ApiKeyPrompt";

/**
 * Blocks rendering of Admin-API-dependent screens until an API key is set (see
 * src/auth/ApiKeyContext.tsx), showing the entry screen instead. A `401` response from any
 * request clears the stored key (src/api/client.ts), which naturally routes back here.
 */
export function ApiKeyGate({ children }: { children: ReactNode }) {
  const { apiKey } = useApiKey();

  if (!apiKey) {
    return <ApiKeyPrompt />;
  }

  return <>{children}</>;
}
