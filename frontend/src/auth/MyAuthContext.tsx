import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from "react";
import type { AuthResultDto, UserDto } from "../api/types";

const STORAGE_KEY = "notifyme.myAuth";

interface StoredSession {
  token: string;
  expiresAt: string;
  user: UserDto;
}

interface MyAuthContextValue {
  token: string | null;
  user: UserDto | null;
  login: (result: AuthResultDto) => void;
  logout: () => void;
}

const MyAuthContext = createContext<MyAuthContextValue | undefined>(undefined);

function readStoredSession(): StoredSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return null;
  }
  try {
    return JSON.parse(raw) as StoredSession;
  } catch {
    return null;
  }
}

/**
 * Holds the end-user's JWT bearer token and profile for the lifetime of the browser tab (see
 * ADR-0010). Stored in `sessionStorage`, the same tab-scoped, "not a secure secret store, just a
 * pragmatic reduction of the persistence window" tradeoff the Admin API key already makes (see
 * src/auth/ApiKeyContext.tsx and ai/decisions/adr/0009-frontend-stack.md). Deliberately a
 * separate context/storage key from the Admin API key: the two auth mechanisms never interact
 * (see ADR-0010).
 */
export function MyAuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<StoredSession | null>(() => readStoredSession());

  const login = useCallback((result: AuthResultDto) => {
    const next: StoredSession = { token: result.token, expiresAt: result.expiresAt, user: result.user };
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(next));
    setSession(next);
  }, []);

  const logout = useCallback(() => {
    sessionStorage.removeItem(STORAGE_KEY);
    setSession(null);
  }, []);

  const value = useMemo(
    () => ({ token: session?.token ?? null, user: session?.user ?? null, login, logout }),
    [session, login, logout],
  );

  return <MyAuthContext.Provider value={value}>{children}</MyAuthContext.Provider>;
}

export function useMyAuth(): MyAuthContextValue {
  const context = useContext(MyAuthContext);
  if (!context) {
    throw new Error("useMyAuth must be used within a MyAuthProvider");
  }
  return context;
}
