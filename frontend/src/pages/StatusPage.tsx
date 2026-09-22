import { useQuery } from "@tanstack/react-query";
import { fetchHealth } from "../api/health";

/**
 * Landing page: shows `/health` (see docs/api/admin-api.md#health) so the foundation phase has
 * something real to verify against a running backend without needing any CRUD screens yet.
 */
export function StatusPage() {
  const { data: status, isLoading } = useQuery({
    queryKey: ["health"],
    queryFn: fetchHealth,
    refetchInterval: 15_000,
  });

  return (
    <section>
      <h2>Backend status</h2>
      <p>
        {isLoading ? "Checking..." : `/health reports: ${status}`}
      </p>
    </section>
  );
}
