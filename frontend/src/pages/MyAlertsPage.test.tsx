import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { MyAlertsPage } from "./MyAlertsPage";
import { useMyApiClient } from "../api/useMyApiClient";
import type { AlertRuleDto, ChannelConfigDto } from "../api/types";

vi.mock("../api/useMyApiClient", () => ({
  useMyApiClient: vi.fn(),
}));

const mockedUseMyApiClient = vi.mocked(useMyApiClient);

const sampleRule: AlertRuleDto = {
  id: "11111111-1111-1111-1111-111111111111",
  name: "My rule",
  category: 0,
  keywords: ["market"],
  minimumSeverity: 1,
  isEnabled: true,
  createdAt: "2026-01-01T00:00:00Z",
};

const sampleChannel: ChannelConfigDto = {
  id: "22222222-2222-2222-2222-222222222222",
  channelType: "slack",
  target: "https://hooks.example.com/abc",
  isEnabled: true,
};

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MyAlertsPage />
    </QueryClientProvider>,
  );
}

/**
 * Covers the Phase 16 consolidated self-service screen (see
 * ai/plan/phase-16-user-self-service.md): rendering the caller's own alert rules/channels and
 * submitting the alert-rule create form against `/api/me/*`. `useMyApiClient` is mocked so this
 * exercises the screen's own query/mutation wiring without a real backend.
 */
describe("MyAlertsPage", () => {
  let client: {
    GET: ReturnType<typeof vi.fn>;
    POST: ReturnType<typeof vi.fn>;
    DELETE: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    client = {
      GET: vi.fn().mockImplementation(async (path: string) => {
        if (path === "/api/me/alert-rules") {
          return { data: [sampleRule], response: new Response(null, { status: 200 }) };
        }
        if (path === "/api/me/channels") {
          return { data: [sampleChannel], response: new Response(null, { status: 200 }) };
        }
        return { data: [], response: new Response(null, { status: 200 }) };
      }),
      POST: vi.fn(),
      DELETE: vi.fn(),
    };
    mockedUseMyApiClient.mockReturnValue(client as never);
  });

  it("renders the caller's own alert rules and channels", async () => {
    renderPage();

    const alertRulesTable = (await screen.findByRole("heading", { name: "Alert rules" })).closest(
      "section",
    ) as HTMLElement;
    expect(await within(alertRulesTable).findByText("My rule")).toBeInTheDocument();

    const channelsSection = screen.getByRole("heading", { name: "Channels" }).closest("section") as HTMLElement;
    expect(await within(channelsSection).findByText("slack")).toBeInTheDocument();

    expect(client.GET).toHaveBeenCalledWith(
      "/api/me/alert-rules",
      expect.objectContaining({ params: { query: {} } }),
    );
  });

  it("submits a create request for a new alert rule", async () => {
    client.POST.mockResolvedValue({ data: sampleRule, response: new Response(null, { status: 201 }) });
    const user = userEvent.setup();
    renderPage();

    const alertRulesSection = (await screen.findByRole("heading", { name: "Alert rules" })).closest(
      "section",
    ) as HTMLElement;
    await within(alertRulesSection).findByText("My rule");

    await user.type(within(alertRulesSection).getByLabelText("Name"), "New rule");
    await user.type(within(alertRulesSection).getByLabelText("Keywords (comma-separated)"), "foo, bar");
    await user.click(within(alertRulesSection).getByRole("button", { name: "Create" }));

    await waitFor(() =>
      expect(client.POST).toHaveBeenCalledWith("/api/me/alert-rules", {
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
