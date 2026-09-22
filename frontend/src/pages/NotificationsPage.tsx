import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useAdminApiClient } from "../api/useAdminApiClient";
import { unwrap } from "../api/apiError";
import { NOTIFICATION_STATUS_LABELS, labelFor, optionsFor } from "../api/enums";
import { ErrorMessage } from "../components/ErrorMessage";

const cellStyle: React.CSSProperties = { border: "1px solid #ccc", padding: "0.25rem 0.5rem", textAlign: "left" };

/** Read-only notification history (no create/edit/delete - matches the read-only Admin API). */
export function NotificationsPage() {
  const client = useAdminApiClient();

  const [statusFilter, setStatusFilter] = useState("");
  const [alertRuleIdFilter, setAlertRuleIdFilter] = useState("");
  const [sentFromFilter, setSentFromFilter] = useState("");
  const [sentToFilter, setSentToFilter] = useState("");

  const { data: alertRules } = useQuery({
    queryKey: ["alert-rules", "", ""],
    queryFn: async () => unwrap(await client!.GET("/api/admin/alert-rules", { params: { query: {} } })),
    enabled: !!client,
  });

  const {
    data: notifications,
    isLoading,
    error: listError,
  } = useQuery({
    queryKey: ["notifications", statusFilter, alertRuleIdFilter, sentFromFilter, sentToFilter],
    queryFn: async () =>
      unwrap(
        await client!.GET("/api/admin/notifications", {
          params: {
            query: {
              status: statusFilter === "" ? undefined : Number(statusFilter),
              alertRuleId: alertRuleIdFilter === "" ? undefined : alertRuleIdFilter,
              sentFrom: sentFromFilter === "" ? undefined : new Date(sentFromFilter).toISOString(),
              sentTo: sentToFilter === "" ? undefined : new Date(sentToFilter).toISOString(),
            },
          },
        }),
      ),
    enabled: !!client,
  });

  function alertRuleName(id: string): string {
    return alertRules?.find((rule) => rule.id === id)?.name ?? id;
  }

  return (
    <section>
      <h2>Notifications</h2>

      <div style={{ marginBottom: "1rem", display: "flex", gap: "1rem", flexWrap: "wrap" }}>
        <label>
          Status:{" "}
          <select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}>
            <option value="">All</option>
            {optionsFor(NOTIFICATION_STATUS_LABELS).map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </label>
        <label>
          Alert rule:{" "}
          <select value={alertRuleIdFilter} onChange={(event) => setAlertRuleIdFilter(event.target.value)}>
            <option value="">All</option>
            {alertRules?.map((rule) => (
              <option key={rule.id} value={rule.id}>
                {rule.name}
              </option>
            ))}
          </select>
        </label>
        <label>
          Sent from:{" "}
          <input
            type="datetime-local"
            value={sentFromFilter}
            onChange={(event) => setSentFromFilter(event.target.value)}
          />
        </label>
        <label>
          Sent to:{" "}
          <input
            type="datetime-local"
            value={sentToFilter}
            onChange={(event) => setSentToFilter(event.target.value)}
          />
        </label>
      </div>

      {isLoading && <p>Loading...</p>}
      <ErrorMessage error={listError} />

      <table style={{ borderCollapse: "collapse", width: "100%" }}>
        <thead>
          <tr>
            <th style={cellStyle}>Alert rule</th>
            <th style={cellStyle}>Status</th>
            <th style={cellStyle}>Sent at</th>
            <th style={cellStyle}>Error</th>
          </tr>
        </thead>
        <tbody>
          {notifications?.map((notification) => (
            <tr key={notification.id}>
              <td style={cellStyle}>{alertRuleName(notification.alertRuleId)}</td>
              <td style={cellStyle}>{labelFor(NOTIFICATION_STATUS_LABELS, notification.status)}</td>
              <td style={cellStyle}>
                {notification.sentAt ? new Date(notification.sentAt).toLocaleString() : "-"}
              </td>
              <td style={cellStyle}>{notification.error ?? "-"}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  );
}
