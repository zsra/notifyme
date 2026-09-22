import { useMutation, useQuery } from "@tanstack/react-query";
import { fetchHealth } from "../api/health";
import { useAdminApiClient } from "../api/useAdminApiClient";
import { unwrap } from "../api/apiError";
import { ErrorMessage } from "../components/ErrorMessage";

/**
 * Landing page: shows `/health` (see docs/api/admin-api.md#health) plus a manual
 * "trigger simulated event" action (`POST /api/admin/events/trigger-simulated`), which is
 * otherwise only run automatically by `EventIngestionWorker`'s polling interval.
 */
export function StatusPage() {
  const client = useAdminApiClient();

  const { data: status, isLoading } = useQuery({
    queryKey: ["health"],
    queryFn: fetchHealth,
    refetchInterval: 15_000,
  });

  const triggerMutation = useMutation({
    mutationFn: async () => unwrap(await client!.POST("/api/admin/events/trigger-simulated", {})),
  });

  return (
    <section>
      <h2>Backend status</h2>
      <p>
        {isLoading ? "Checking..." : `/health reports: ${status}`}
      </p>

      <h3>Trigger simulated event</h3>
      <p>
        <button type="button" onClick={() => triggerMutation.mutate()} disabled={triggerMutation.isPending}>
          Trigger now
        </button>
      </p>
      {triggerMutation.isSuccess && (
        <p>
          Fetched {triggerMutation.data.rawEventCount}, normalized {triggerMutation.data.normalizedEventCount},
          dispatched {triggerMutation.data.notificationsDispatched} notification(s).
        </p>
      )}
      <ErrorMessage error={triggerMutation.error} />
    </section>
  );
}
