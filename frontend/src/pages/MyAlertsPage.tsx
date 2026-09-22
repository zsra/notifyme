import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMyApiClient } from "../api/useMyApiClient";
import { unwrap } from "../api/apiError";
import { EVENT_CATEGORY_LABELS, SEVERITY_LABELS, labelFor, optionsFor } from "../api/enums";
import type { CreateAlertRuleRequest, CreateChannelConfigRequest, CreateSubscriptionRequest } from "../api/types";
import { ErrorMessage } from "../components/ErrorMessage";

interface AlertRuleFormState {
  name: string;
  category: string;
  keywords: string;
  minimumSeverity: string;
  isEnabled: boolean;
}

const emptyAlertRuleForm: AlertRuleFormState = {
  name: "",
  category: "0",
  keywords: "",
  minimumSeverity: "0",
  isEnabled: true,
};

interface ChannelFormState {
  channelType: string;
  target: string;
  isEnabled: boolean;
}

const emptyChannelForm: ChannelFormState = { channelType: "slack", target: "", isEnabled: true };

function toKeywords(raw: string): string[] {
  return raw
    .split(",")
    .map((keyword) => keyword.trim())
    .filter((keyword) => keyword.length > 0);
}

const cellStyle: React.CSSProperties = { border: "1px solid #ccc", padding: "0.25rem 0.5rem", textAlign: "left" };

/**
 * Consolidated self-service screen (see ai/plan/phase-16-user-self-service.md step 6): create,
 * list, and delete the caller's own alert rules, channels, and subscriptions, all scoped
 * server-side to the JWT's user id via `/api/me/*` (see ADR-0010). Deliberately one page rather
 * than mirroring all three Admin screens - a first self-service cut doesn't need per-resource
 * edit support, only create/list/delete.
 */
export function MyAlertsPage() {
  const client = useMyApiClient();
  const queryClient = useQueryClient();

  const [alertRuleForm, setAlertRuleForm] = useState<AlertRuleFormState>(emptyAlertRuleForm);
  const [channelForm, setChannelForm] = useState<ChannelFormState>(emptyChannelForm);
  const [newAlertRuleId, setNewAlertRuleId] = useState("");
  const [newChannelConfigId, setNewChannelConfigId] = useState("");

  const {
    data: alertRules,
    isLoading: alertRulesLoading,
    error: alertRulesError,
  } = useQuery({
    queryKey: ["my-alert-rules"],
    queryFn: async () => unwrap(await client!.GET("/api/me/alert-rules", { params: { query: {} } })),
    enabled: !!client,
  });

  const {
    data: channels,
    isLoading: channelsLoading,
    error: channelsError,
  } = useQuery({
    queryKey: ["my-channels"],
    queryFn: async () => unwrap(await client!.GET("/api/me/channels", { params: { query: {} } })),
    enabled: !!client,
  });

  const {
    data: subscriptions,
    isLoading: subscriptionsLoading,
    error: subscriptionsError,
  } = useQuery({
    queryKey: ["my-subscriptions"],
    queryFn: async () => unwrap(await client!.GET("/api/me/subscriptions", { params: { query: {} } })),
    enabled: !!client,
  });

  const createAlertRuleMutation = useMutation({
    mutationFn: async (request: CreateAlertRuleRequest) =>
      unwrap(await client!.POST("/api/me/alert-rules", { body: request })),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["my-alert-rules"] });
      setAlertRuleForm(emptyAlertRuleForm);
    },
  });

  const deleteAlertRuleMutation = useMutation({
    mutationFn: async (id: string) =>
      unwrap(await client!.DELETE("/api/me/alert-rules/{id}", { params: { path: { id } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["my-alert-rules"] }),
  });

  const createChannelMutation = useMutation({
    mutationFn: async (request: CreateChannelConfigRequest) =>
      unwrap(await client!.POST("/api/me/channels", { body: request })),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["my-channels"] });
      setChannelForm(emptyChannelForm);
    },
  });

  const deleteChannelMutation = useMutation({
    mutationFn: async (id: string) =>
      unwrap(await client!.DELETE("/api/me/channels/{id}", { params: { path: { id } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["my-channels"] }),
  });

  const createSubscriptionMutation = useMutation({
    mutationFn: async (request: CreateSubscriptionRequest) =>
      unwrap(await client!.POST("/api/me/subscriptions", { body: request })),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["my-subscriptions"] });
      setNewAlertRuleId("");
      setNewChannelConfigId("");
    },
  });

  const deleteSubscriptionMutation = useMutation({
    mutationFn: async (id: string) =>
      unwrap(await client!.DELETE("/api/me/subscriptions/{id}", { params: { path: { id } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["my-subscriptions"] }),
  });

  function handleCreateAlertRule(event: React.FormEvent) {
    event.preventDefault();
    createAlertRuleMutation.mutate({
      name: alertRuleForm.name,
      category: Number(alertRuleForm.category),
      keywords: toKeywords(alertRuleForm.keywords),
      minimumSeverity: Number(alertRuleForm.minimumSeverity),
      isEnabled: alertRuleForm.isEnabled,
    });
  }

  function handleCreateChannel(event: React.FormEvent) {
    event.preventDefault();
    createChannelMutation.mutate({
      channelType: channelForm.channelType,
      target: channelForm.target,
      isEnabled: channelForm.isEnabled,
    });
  }

  function handleCreateSubscription(event: React.FormEvent) {
    event.preventDefault();
    if (!newAlertRuleId || !newChannelConfigId) {
      return;
    }
    createSubscriptionMutation.mutate({ alertRuleId: newAlertRuleId, channelConfigId: newChannelConfigId });
  }

  function alertRuleName(id: string): string {
    return alertRules?.find((rule) => rule.id === id)?.name ?? id;
  }

  function channelLabel(id: string): string {
    const channel = channels?.find((entry) => entry.id === id);
    return channel ? `${channel.channelType}: ${channel.target}` : id;
  }

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "2rem" }}>
      <section>
        <h2>Alert rules</h2>

        {alertRulesLoading && <p>Loading...</p>}
        <ErrorMessage error={alertRulesError} />

        <table style={{ borderCollapse: "collapse", width: "100%", marginBottom: "1rem" }}>
          <thead>
            <tr>
              <th style={cellStyle}>Name</th>
              <th style={cellStyle}>Category</th>
              <th style={cellStyle}>Keywords</th>
              <th style={cellStyle}>Min. severity</th>
              <th style={cellStyle}>Enabled</th>
              <th style={cellStyle} />
            </tr>
          </thead>
          <tbody>
            {alertRules?.map((rule) => (
              <tr key={rule.id}>
                <td style={cellStyle}>{rule.name}</td>
                <td style={cellStyle}>{labelFor(EVENT_CATEGORY_LABELS, rule.category)}</td>
                <td style={cellStyle}>{rule.keywords.join(", ")}</td>
                <td style={cellStyle}>{labelFor(SEVERITY_LABELS, rule.minimumSeverity)}</td>
                <td style={cellStyle}>{rule.isEnabled ? "Yes" : "No"}</td>
                <td style={cellStyle}>
                  <button
                    type="button"
                    onClick={() => {
                      if (window.confirm(`Delete alert rule "${rule.name}"?`)) {
                        deleteAlertRuleMutation.mutate(rule.id);
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
        <ErrorMessage error={deleteAlertRuleMutation.error} />

        <h3>New alert rule</h3>
        <form
          onSubmit={handleCreateAlertRule}
          style={{ display: "flex", flexDirection: "column", gap: "0.5rem", maxWidth: "24rem" }}
        >
          <label>
            Name{" "}
            <input
              type="text"
              required
              value={alertRuleForm.name}
              onChange={(event) => setAlertRuleForm({ ...alertRuleForm, name: event.target.value })}
            />
          </label>
          <label>
            Category{" "}
            <select
              value={alertRuleForm.category}
              onChange={(event) => setAlertRuleForm({ ...alertRuleForm, category: event.target.value })}
            >
              {optionsFor(EVENT_CATEGORY_LABELS).map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <label>
            Keywords (comma-separated){" "}
            <input
              type="text"
              value={alertRuleForm.keywords}
              onChange={(event) => setAlertRuleForm({ ...alertRuleForm, keywords: event.target.value })}
            />
          </label>
          <label>
            Minimum severity{" "}
            <select
              value={alertRuleForm.minimumSeverity}
              onChange={(event) => setAlertRuleForm({ ...alertRuleForm, minimumSeverity: event.target.value })}
            >
              {optionsFor(SEVERITY_LABELS).map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <label>
            <input
              type="checkbox"
              checked={alertRuleForm.isEnabled}
              onChange={(event) => setAlertRuleForm({ ...alertRuleForm, isEnabled: event.target.checked })}
            />{" "}
            Enabled
          </label>
          <div>
            <button type="submit" disabled={createAlertRuleMutation.isPending}>
              Create
            </button>
          </div>
          <ErrorMessage error={createAlertRuleMutation.error} />
        </form>
      </section>

      <section>
        <h2>Channels</h2>

        {channelsLoading && <p>Loading...</p>}
        <ErrorMessage error={channelsError} />

        <datalist id="my-known-channel-types">
          <option value="slack" />
          <option value="email" />
        </datalist>

        <table style={{ borderCollapse: "collapse", width: "100%", marginBottom: "1rem" }}>
          <thead>
            <tr>
              <th style={cellStyle}>Type</th>
              <th style={cellStyle}>Target</th>
              <th style={cellStyle}>Enabled</th>
              <th style={cellStyle} />
            </tr>
          </thead>
          <tbody>
            {channels?.map((channel) => (
              <tr key={channel.id}>
                <td style={cellStyle}>{channel.channelType}</td>
                <td style={cellStyle}>{channel.target}</td>
                <td style={cellStyle}>{channel.isEnabled ? "Yes" : "No"}</td>
                <td style={cellStyle}>
                  <button
                    type="button"
                    onClick={() => {
                      if (window.confirm(`Delete channel "${channel.channelType}: ${channel.target}"?`)) {
                        deleteChannelMutation.mutate(channel.id);
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
        <ErrorMessage error={deleteChannelMutation.error} />

        <h3>New channel</h3>
        <form
          onSubmit={handleCreateChannel}
          style={{ display: "flex", flexDirection: "column", gap: "0.5rem", maxWidth: "24rem" }}
        >
          <label>
            Type{" "}
            <input
              type="text"
              list="my-known-channel-types"
              required
              value={channelForm.channelType}
              onChange={(event) => setChannelForm({ ...channelForm, channelType: event.target.value })}
            />
          </label>
          <label>
            Target (webhook URL / email address){" "}
            <input
              type="text"
              required
              value={channelForm.target}
              onChange={(event) => setChannelForm({ ...channelForm, target: event.target.value })}
            />
          </label>
          <label>
            <input
              type="checkbox"
              checked={channelForm.isEnabled}
              onChange={(event) => setChannelForm({ ...channelForm, isEnabled: event.target.checked })}
            />{" "}
            Enabled
          </label>
          <div>
            <button type="submit" disabled={createChannelMutation.isPending}>
              Create
            </button>
          </div>
          <ErrorMessage error={createChannelMutation.error} />
        </form>
      </section>

      <section>
        <h2>Subscriptions</h2>

        {subscriptionsLoading && <p>Loading...</p>}
        <ErrorMessage error={subscriptionsError} />

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
                        deleteSubscriptionMutation.mutate(subscription.id);
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
        <ErrorMessage error={deleteSubscriptionMutation.error} />

        <h3>New subscription</h3>
        <form
          onSubmit={handleCreateSubscription}
          style={{ display: "flex", flexDirection: "column", gap: "0.5rem", maxWidth: "24rem" }}
        >
          <label>
            Alert rule{" "}
            <select required value={newAlertRuleId} onChange={(event) => setNewAlertRuleId(event.target.value)}>
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
            <button type="submit" disabled={createSubscriptionMutation.isPending}>
              Create
            </button>
          </div>
          <ErrorMessage error={createSubscriptionMutation.error} />
        </form>
      </section>
    </div>
  );
}
