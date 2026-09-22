import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useAdminApiClient } from "../api/useAdminApiClient";
import { unwrap } from "../api/apiError";
import type { ChannelConfigDto, CreateChannelConfigRequest, UpdateChannelConfigRequest } from "../api/types";
import { ErrorMessage } from "../components/ErrorMessage";

interface FormState {
  channelType: string;
  target: string;
  isEnabled: boolean;
}

const emptyForm: FormState = { channelType: "slack", target: "", isEnabled: true };

const cellStyle: React.CSSProperties = { border: "1px solid #ccc", padding: "0.25rem 0.5rem", textAlign: "left" };

/**
 * Channels CRUD screen (see ai/plan/phase-14-frontend-admin-screens.md). `channelType` is a free
 * string discriminator on the backend (see docs/architecture/notification-channels.md), not an
 * enum - `known-channel-types` datalist below is just a convenience, any value the backend has a
 * registered `INotificationChannel` for will work.
 */
export function ChannelsPage() {
  const client = useAdminApiClient();
  const queryClient = useQueryClient();

  const [typeFilter, setTypeFilter] = useState("");
  const [enabledFilter, setEnabledFilter] = useState("");
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);

  const {
    data: channels,
    isLoading,
    error: listError,
  } = useQuery({
    queryKey: ["channels", typeFilter, enabledFilter],
    queryFn: async () =>
      unwrap(
        await client!.GET("/api/admin/channels", {
          params: {
            query: {
              channelType: typeFilter === "" ? undefined : typeFilter,
              isEnabled: enabledFilter === "" ? undefined : enabledFilter === "true",
            },
          },
        }),
      ),
    enabled: !!client,
  });

  const createMutation = useMutation({
    mutationFn: async (request: CreateChannelConfigRequest) =>
      unwrap(await client!.POST("/api/admin/channels", { body: request })),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["channels"] });
      setForm(emptyForm);
    },
  });

  const updateMutation = useMutation({
    mutationFn: async (request: UpdateChannelConfigRequest) =>
      unwrap(
        await client!.PUT("/api/admin/channels/{id}", {
          params: { path: { id: request.id } },
          body: request,
        }),
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["channels"] });
      cancelEdit();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: string) =>
      unwrap(await client!.DELETE("/api/admin/channels/{id}", { params: { path: { id } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["channels"] }),
  });

  function startEdit(channel: ChannelConfigDto) {
    setEditingId(channel.id);
    setForm({ channelType: channel.channelType, target: channel.target, isEnabled: channel.isEnabled });
  }

  function cancelEdit() {
    setEditingId(null);
    setForm(emptyForm);
  }

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (editingId) {
      updateMutation.mutate({ id: editingId, channelType: form.channelType, target: form.target });
    } else {
      createMutation.mutate({
        channelType: form.channelType,
        target: form.target,
        isEnabled: form.isEnabled,
      });
    }
  }

  return (
    <section>
      <h2>Channels</h2>

      <datalist id="known-channel-types">
        <option value="slack" />
        <option value="email" />
      </datalist>

      <div style={{ marginBottom: "1rem" }}>
        <label>
          Type:{" "}
          <input
            type="text"
            list="known-channel-types"
            value={typeFilter}
            onChange={(event) => setTypeFilter(event.target.value)}
            placeholder="(all)"
          />
        </label>{" "}
        <label>
          Enabled:{" "}
          <select value={enabledFilter} onChange={(event) => setEnabledFilter(event.target.value)}>
            <option value="">All</option>
            <option value="true">Enabled</option>
            <option value="false">Disabled</option>
          </select>
        </label>
      </div>

      {isLoading && <p>Loading...</p>}
      <ErrorMessage error={listError} />

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
                <button type="button" onClick={() => startEdit(channel)}>
                  Edit
                </button>{" "}
                <button
                  type="button"
                  onClick={() => {
                    if (window.confirm(`Delete channel "${channel.channelType}: ${channel.target}"?`)) {
                      deleteMutation.mutate(channel.id);
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

      <h3>{editingId ? "Edit channel" : "New channel"}</h3>
      <form
        onSubmit={handleSubmit}
        style={{ display: "flex", flexDirection: "column", gap: "0.5rem", maxWidth: "24rem" }}
      >
        <label>
          Type{" "}
          <input
            type="text"
            list="known-channel-types"
            required
            value={form.channelType}
            onChange={(event) => setForm({ ...form, channelType: event.target.value })}
          />
        </label>
        <label>
          Target (webhook URL / email address){" "}
          <input
            type="text"
            required
            value={form.target}
            onChange={(event) => setForm({ ...form, target: event.target.value })}
          />
        </label>
        {!editingId && (
          <label>
            <input
              type="checkbox"
              checked={form.isEnabled}
              onChange={(event) => setForm({ ...form, isEnabled: event.target.checked })}
            />{" "}
            Enabled
          </label>
        )}
        <div>
          <button type="submit" disabled={createMutation.isPending || updateMutation.isPending}>
            {editingId ? "Save" : "Create"}
          </button>{" "}
          {editingId && (
            <button type="button" onClick={cancelEdit}>
              Cancel
            </button>
          )}
        </div>
        <ErrorMessage error={createMutation.error ?? updateMutation.error} />
      </form>
    </section>
  );
}
