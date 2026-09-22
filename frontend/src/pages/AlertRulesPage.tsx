import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useAdminApiClient } from "../api/useAdminApiClient";
import { unwrap } from "../api/apiError";
import { EVENT_CATEGORY_LABELS, SEVERITY_LABELS, labelFor, optionsFor } from "../api/enums";
import type { AlertRuleDto, CreateAlertRuleRequest, UpdateAlertRuleRequest } from "../api/types";
import { ErrorMessage } from "../components/ErrorMessage";

interface FormState {
  name: string;
  category: string;
  keywords: string;
  minimumSeverity: string;
  isEnabled: boolean;
}

const emptyForm: FormState = {
  name: "",
  category: "0",
  keywords: "",
  minimumSeverity: "0",
  isEnabled: true,
};

function toKeywords(raw: string): string[] {
  return raw
    .split(",")
    .map((keyword) => keyword.trim())
    .filter((keyword) => keyword.length > 0);
}

const cellStyle: React.CSSProperties = { border: "1px solid #ccc", padding: "0.25rem 0.5rem", textAlign: "left" };

/**
 * Alert Rules CRUD screen (see ai/plan/phase-14-frontend-admin-screens.md). Note the Admin API
 * has no way to change `isEnabled` after creation (`UpdateAlertRuleRequest` doesn't carry it), so
 * the edit form only shows that checkbox for new rules.
 */
export function AlertRulesPage() {
  const client = useAdminApiClient();
  const queryClient = useQueryClient();

  const [categoryFilter, setCategoryFilter] = useState("");
  const [enabledFilter, setEnabledFilter] = useState("");
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);

  const {
    data: alertRules,
    isLoading,
    error: listError,
  } = useQuery({
    queryKey: ["alert-rules", categoryFilter, enabledFilter],
    queryFn: async () =>
      unwrap(
        await client!.GET("/api/admin/alert-rules", {
          params: {
            query: {
              eventCategory: categoryFilter === "" ? undefined : Number(categoryFilter),
              isEnabled: enabledFilter === "" ? undefined : enabledFilter === "true",
            },
          },
        }),
      ),
    enabled: !!client,
  });

  const createMutation = useMutation({
    mutationFn: async (request: CreateAlertRuleRequest) =>
      unwrap(await client!.POST("/api/admin/alert-rules", { body: request })),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["alert-rules"] });
      setForm(emptyForm);
    },
  });

  const updateMutation = useMutation({
    mutationFn: async (request: UpdateAlertRuleRequest) =>
      unwrap(
        await client!.PUT("/api/admin/alert-rules/{id}", {
          params: { path: { id: request.id } },
          body: request,
        }),
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["alert-rules"] });
      cancelEdit();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: string) =>
      unwrap(await client!.DELETE("/api/admin/alert-rules/{id}", { params: { path: { id } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["alert-rules"] }),
  });

  function startEdit(rule: AlertRuleDto) {
    setEditingId(rule.id);
    setForm({
      name: rule.name,
      category: String(rule.category),
      keywords: rule.keywords.join(", "),
      minimumSeverity: String(rule.minimumSeverity),
      isEnabled: rule.isEnabled,
    });
  }

  function cancelEdit() {
    setEditingId(null);
    setForm(emptyForm);
  }

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    const keywords = toKeywords(form.keywords);
    if (editingId) {
      updateMutation.mutate({
        id: editingId,
        name: form.name,
        category: Number(form.category),
        keywords,
        minimumSeverity: Number(form.minimumSeverity),
      });
    } else {
      createMutation.mutate({
        name: form.name,
        category: Number(form.category),
        keywords,
        minimumSeverity: Number(form.minimumSeverity),
        isEnabled: form.isEnabled,
      });
    }
  }

  return (
    <section>
      <h2>Alert Rules</h2>

      <div style={{ marginBottom: "1rem" }}>
        <label>
          Category:{" "}
          <select value={categoryFilter} onChange={(event) => setCategoryFilter(event.target.value)}>
            <option value="">All</option>
            {optionsFor(EVENT_CATEGORY_LABELS).map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
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
            <th style={cellStyle}>Name</th>
            <th style={cellStyle}>Category</th>
            <th style={cellStyle}>Keywords</th>
            <th style={cellStyle}>Min. severity</th>
            <th style={cellStyle}>Enabled</th>
            <th style={cellStyle}>Created</th>
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
              <td style={cellStyle}>{new Date(rule.createdAt).toLocaleString()}</td>
              <td style={cellStyle}>
                <button type="button" onClick={() => startEdit(rule)}>
                  Edit
                </button>{" "}
                <button
                  type="button"
                  onClick={() => {
                    if (window.confirm(`Delete alert rule "${rule.name}"?`)) {
                      deleteMutation.mutate(rule.id);
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

      <h3>{editingId ? "Edit alert rule" : "New alert rule"}</h3>
      <form
        onSubmit={handleSubmit}
        style={{ display: "flex", flexDirection: "column", gap: "0.5rem", maxWidth: "24rem" }}
      >
        <label>
          Name{" "}
          <input
            type="text"
            required
            value={form.name}
            onChange={(event) => setForm({ ...form, name: event.target.value })}
          />
        </label>
        <label>
          Category{" "}
          <select value={form.category} onChange={(event) => setForm({ ...form, category: event.target.value })}>
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
            value={form.keywords}
            onChange={(event) => setForm({ ...form, keywords: event.target.value })}
          />
        </label>
        <label>
          Minimum severity{" "}
          <select
            value={form.minimumSeverity}
            onChange={(event) => setForm({ ...form, minimumSeverity: event.target.value })}
          >
            {optionsFor(SEVERITY_LABELS).map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
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
