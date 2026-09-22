import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { AlertRulesPage } from "./AlertRulesPage";
import { useAdminApiClient } from "../api/useAdminApiClient";
import type { AlertRuleDto } from "../api/types";

vi.mock("../api/useAdminApiClient", () => ({
  useAdminApiClient: vi.fn(),
}));

const mockedUseAdminApiClient = vi.mocked(useAdminApiClient);

const sampleRule: AlertRuleDto = {
  id: "11111111-1111-1111-1111-111111111111",
  name: "Sample rule",
  category: 0,
  keywords: ["market"],
  minimumSeverity: 1,
  isEnabled: true,
  createdAt: "2026-01-01T00:00:00Z",
};

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <AlertRulesPage />
    </QueryClientProvider>,
  );
}

/**
 * Covers the Phase 14 Alert Rules screen (see ai/plan/phase-14-frontend-admin-screens.md):
 * fetching/rendering the list, re-querying when a filter changes, and submitting the create
 * form with the shaped request body. `useAdminApiClient` is mocked so these tests exercise the
 * screen's own query/mutation wiring without a real backend.
 */
describe("AlertRulesPage", () => {
  let client: {
    GET: ReturnType<typeof vi.fn>;
    POST: ReturnType<typeof vi.fn>;
    PUT: ReturnType<typeof vi.fn>;
    DELETE: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    client = {
      GET: vi.fn().mockResolvedValue({ data: [sampleRule], response: new Response(null, { status: 200 }) }),
      POST: vi.fn(),
      PUT: vi.fn(),
      DELETE: vi.fn(),
    };
    mockedUseAdminApiClient.mockReturnValue(client as never);
  });

  it("renders alert rules fetched from the API", async () => {
    renderPage();

    expect(await screen.findByText("Sample rule")).toBeInTheDocument();
    expect(client.GET).toHaveBeenCalledWith(
      "/api/admin/alert-rules",
      expect.objectContaining({
        params: { query: { eventCategory: undefined, isEnabled: undefined } },
      }),
    );
  });

  it("re-queries with the selected category filter", async () => {
    const user = userEvent.setup();
    renderPage();
    await screen.findByText("Sample rule");

    const [categorySelect] = screen.getAllByRole("combobox");
    await user.selectOptions(categorySelect, "Market movement");

    await waitFor(() =>
      expect(client.GET).toHaveBeenLastCalledWith(
        "/api/admin/alert-rules",
        expect.objectContaining({
          params: { query: { eventCategory: 1, isEnabled: undefined } },
        }),
      ),
    );
  });

  it("submits a create request with the entered form data", async () => {
    client.POST.mockResolvedValue({ data: sampleRule, response: new Response(null, { status: 201 }) });
    const user = userEvent.setup();
    renderPage();
    await screen.findByText("Sample rule");

    await user.type(screen.getByLabelText("Name"), "New rule");
    await user.type(screen.getByLabelText("Keywords (comma-separated)"), "foo, bar");
    await user.click(screen.getByRole("button", { name: "Create" }));

    await waitFor(() =>
      expect(client.POST).toHaveBeenCalledWith("/api/admin/alert-rules", {
        body: {
          name: "New rule",
          category: 0,
          keywords: ["foo", "bar"],
          minimumSeverity: 0,
          isEnabled: true,
        },
      }),
    );
  });
});
