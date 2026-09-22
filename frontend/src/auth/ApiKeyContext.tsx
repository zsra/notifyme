import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from "react";

const STORAGE_KEY = "notifyme.adminApiKey";

interface ApiKeyContextValue {
  apiKey: string | null;
  setApiKey: (key: string) => void;
  clearApiKey: () => void;
}

const ApiKeyContext = createContext<ApiKeyContextValue | undefined>(undefined);

/**
 * Holds the Admin API key (see ai/decisions/adr/0006-admin-api-auth.md) for the lifetime of the
 * browser tab. Stored in `sessionStorage`, not `localStorage`, per
 * ai/decisions/adr/0009-frontend-stack.md: a modest, pragmatic reduction of the persistence
 * window, not a claim that this is a secure secret store.
 */
export function ApiKeyProvider({ children }: { children: ReactNode }) {
  const [apiKey, setApiKeyState] = useState<string | null>(() => sessionStorage.getItem(STORAGE_KEY));

  const setApiKey = useCallback((key: string) => {
    sessionStorage.setItem(STORAGE_KEY, key);
    setApiKeyState(key);
  }, []);

  const clearApiKey = useCallback(() => {
    sessionStorage.removeItem(STORAGE_KEY);
    setApiKeyState(null);
  }, []);

  const value = useMemo(() => ({ apiKey, setApiKey, clearApiKey }), [apiKey, setApiKey, clearApiKey]);

  return <ApiKeyContext.Provider value={value}>{children}</ApiKeyContext.Provider>;
}

export function useApiKey(): ApiKeyContextValue {
  const context = useContext(ApiKeyContext);
  if (!context) {
    throw new Error("useApiKey must be used within an ApiKeyProvider");
  }
  return context;
}
