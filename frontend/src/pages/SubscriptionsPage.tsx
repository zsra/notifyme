import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useAdminApiClient } from "../api/useAdminApiClient";
import { unwrap } from "../api/apiError";
import type { CreateSubscriptionRequest } from "../api/types";
import { ErrorMessage } from "../components/ErrorMessage";

const cellStyle: React.CSSProperties = { border: "1px solid #ccc", padding: "0.25rem 0.5rem", textAlign: "left" };

/**
 * Subscriptions screen: links an Alert Rule to a Channel. Read-only besides create/delete (the
 * Admin API has no update endpoint for subscriptions - see `SubscriptionsEndpoints.cs`). Alert
 * Rule/Channel names are looked up client-side so rows don't just show raw GUIDs.
 */
export function SubscriptionsPage() {
  const client = useAdminApiClient();
  const queryClient = useQueryClient();

  const [alertRuleFilter, setAlertRuleFilter] = useState("");
  const [channelFilter, setChannelFilter] = useState("");
  const [newAlertRuleId, setNewAlertRuleId] = useState("");
  const [newChannelConfigId, setNewChannelConfigId] = useState("");

  const { data: alertRules } = useQuery({
    queryKey: ["alert-rules", "", ""],
    queryFn: async () => unwrap(await client!.GET("/api/admin/alert-rules", { params: { query: {} } })),
    enabled: !!client,
  });

  const { data: channels } = useQuery({
    queryKey: ["channels", "", ""],
    queryFn: async () => unwrap(await client!.GET("/api/admin/channels", { params: { query: {} } })),
    enabled: !!client,
  });

  const {
    data: subscriptions,
    isLoading,
    error: listError,
  } = useQuery({
    queryKey: ["subscriptions", alertRuleFilter, channelFilter],
    queryFn: async () =>
      unwrap(
        await client!.GET("/api/admin/subscriptions", {
          params: {
            query: {
              alertRuleId: alertRuleFilter === "" ? undefined : alertRuleFilter,
              channelConfigId: channelFilter === "" ? undefined : channelFilter,
            },
          },
        }),
      ),
    enabled: !!client,
  });

  const createMutation = useMutation({
    mutationFn: async (request: CreateSubscriptionRequest) =>
      unwrap(await client!.POST("/api/admin/subscriptions", { body: request })),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["subscriptions"] });
      setNewAlertRuleId("");
      setNewChannelConfigId("");
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: string) =>
      unwrap(await client!.DELETE("/api/admin/subscriptions/{id}", { params: { path: { id } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["subscriptions"] }),
  });

  function alertRuleName(id: string): string {
    return alertRules?.find((rule) => rule.id === id)?.name ?? id;
  }

  function channelLabel(id: string): string {
    const channel = channels?.find((entry) => entry.id === id);
    return channel ? `${channel.channelType}: ${channel.target}` : id;
  }

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (!newAlertRuleId || !newChannelConfigId) {
      return;
    }
    createMutation.mutate({ alertRuleId: newAlertRuleId, channelConfigId: newChannelConfigId });
  }

  return (
    <section>
      <h2>Subscriptions</h2>

      <div style={{ marginBottom: "1rem" }}>
        <label>
          Alert rule:{" "}
          <select value={alertRuleFilter} onChange={(event) => setAlertRuleFilter(event.target.value)}>
            <option value="">All</option>
            {alertRules?.map((rule) => (
              <option key={rule.id} value={rule.id}>
                {rule.name}
              </option>
            ))}
          </select>
        </label>{" "}
        <label>
          Channel:{" "}
          <select value={channelFilter} onChange={(event) => setChannelFilter(event.target.value)}>
            <option value="">All</option>
            {channels?.map((channel) => (
              <option key={channel.id} value={channel.id}>
                {channel.channelType}: {channel.target}
              </option>
            ))}
          </select>
        </label>
      </div>

      {isLoading && <p>Loading...</p>}
      <ErrorMessage error={listError} />

      <table style={{ borderCollapse: "collapse", width: "100%", marginBottom: "1rem" }}>
        <thead>
          <tr>
            <th style={cellStyle}>Alert rule</th>
            <th style={cellStyle}>Channel</th>
            <th style={cellStyle} />
          </tr>
        </thead>
        <tbody>
          {subscriptions?.map((subscription) => (
            <tr key={subscription.id}>
              <td style={cellStyle}>{alertRuleName(subscription.alertRuleId)}</td>
              <td style={cellStyle}>{channelLabel(subscription.channelConfigId)}</td>
              <td style={cellStyle}>
                <button
                  type="button"
                  onClick={() => {
                    if (window.confirm("Delete this subscription?")) {
                      deleteMutation.mutate(subscription.id);
                    }
                  }}
                >
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <ErrorMessage error={deleteMutation.error} />

      <h3>New subscription</h3>
      <form
        onSubmit={handleSubmit}
        style={{ display: "flex", flexDirection: "column", gap: "0.5rem", maxWidth: "24rem" }}
      >
        <label>
          Alert rule{" "}
          <select
            required
            value={newAlertRuleId}
            onChange={(event) => setNewAlertRuleId(event.target.value)}
          >
            <option value="" disabled>
              Select an alert rule
            </option>
            {alertRules?.map((rule) => (
              <option key={rule.id} value={rule.id}>
                {rule.name}
              </option>
            ))}
          </select>
        </label>
        <label>
          Channel{" "}
          <select
            required
            value={newChannelConfigId}
            onChange={(event) => setNewChannelConfigId(event.target.value)}
          >
            <option value="" disabled>
              Select a channel
            </option>
            {channels?.map((channel) => (
              <option key={channel.id} value={channel.id}>
                {channel.channelType}: {channel.target}
              </option>
            ))}
          </select>
        </label>
        <div>
          <button type="submit" disabled={createMutation.isPending}>
            Create
          </button>
        </div>
        <ErrorMessage error={createMutation.error} />
      </form>
    </section>
  );
}
