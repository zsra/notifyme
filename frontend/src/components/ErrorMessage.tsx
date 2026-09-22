/** Minimal, consistent rendering of a thrown error (see `src/api/apiError.ts`) next to a form/action. */
export function ErrorMessage({ error }: { error: unknown }) {
  if (!error) {
    return null;
  }
  const message = error instanceof Error ? error.message : "Unknown error";
  return <p style={{ color: "#b00020" }}>{message}</p>;
}
