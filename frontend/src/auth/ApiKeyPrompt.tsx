import { useState, type FormEvent } from "react";
import { useApiKey } from "./ApiKeyContext";

/**
 * Simple one-field entry screen for the Admin API key (see
 * ai/decisions/adr/0006-admin-api-auth.md). Shown whenever no key is stored yet, and again
 * whenever a request comes back `401` (see src/api/client.ts's `onUnauthorized`).
 */
export function ApiKeyPrompt() {
  const { setApiKey } = useApiKey();
  const [value, setValue] = useState("");

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const trimmed = value.trim();
    if (trimmed.length > 0) {
      setApiKey(trimmed);
    }
  }

  return (
    <div style={{ maxWidth: "24rem", margin: "4rem auto", fontFamily: "sans-serif" }}>
      <h1>NotifyMe Admin</h1>
      <p>Enter the Admin API key to continue. It is kept only for this browser tab.</p>
      <form onSubmit={handleSubmit}>
        <input
          type="password"
          value={value}
          onChange={(event) => setValue(event.target.value)}
          placeholder="Admin API key"
          autoFocus
          style={{ width: "100%", padding: "0.5rem", boxSizing: "border-box" }}
        />
        <button type="submit" style={{ marginTop: "0.75rem", padding: "0.5rem 1rem" }}>
          Continue
        </button>
      </form>
    </div>
  );
}
