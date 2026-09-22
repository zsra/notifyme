import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { LoginPage } from "./LoginPage";
import { createPublicApiClient } from "../api/client";
import { useMyAuth } from "../auth/MyAuthContext";
import type { AuthResultDto } from "../api/types";

vi.mock("../api/client", async (importOriginal) => ({
  ...(await importOriginal<typeof import("../api/client")>()),
  createPublicApiClient: vi.fn(),
}));

vi.mock("../auth/MyAuthContext", () => ({
  useMyAuth: vi.fn(),
}));

const navigateMock = vi.fn();
vi.mock("react-router-dom", async (importOriginal) => ({
  ...(await importOriginal<typeof import("react-router-dom")>()),
  useNavigate: () => navigateMock,
}));

const mockedCreatePublicApiClient = vi.mocked(createPublicApiClient);
const mockedUseMyAuth = vi.mocked(useMyAuth);

const authResult: AuthResultDto = {
  token: "a-jwt-token",
  expiresAt: "2026-01-01T00:00:00Z",
  user: { id: "11111111-1111-1111-1111-111111111111", email: "user@example.com", createdAt: "2026-01-01T00:00:00Z" },
};

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

/**
 * Covers the Phase 16 login screen (see ai/plan/phase-16-user-self-service.md): submitting
 * credentials, storing the returned session, and navigating to `/my` on success.
 * `createPublicApiClient` and `useMyAuth` are mocked so this exercises the screen's own wiring
 * without a real backend.
 */
describe("LoginPage", () => {
  let client: { POST: ReturnType<typeof vi.fn> };
  let login: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    navigateMock.mockReset();
    login = vi.fn();
    client = { POST: vi.fn() };
    mockedCreatePublicApiClient.mockReturnValue(client as never);
    mockedUseMyAuth.mockReturnValue({ token: null, user: null, login, logout: vi.fn() } as never);
  });

  it("logs in and navigates to /my on success", async () => {
    client.POST.mockResolvedValue({ data: authResult, response: new Response(null, { status: 200 }) });
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText("Email"), "user@example.com");
    await user.type(screen.getByLabelText("Password"), "a-strong-password");
    await user.click(screen.getByRole("button", { name: "Log in" }));

    await waitFor(() =>
      expect(client.POST).toHaveBeenCalledWith("/api/auth/login", {
        body: { email: "user@example.com", password: "a-strong-password" },
      }),
    );
    await waitFor(() => expect(login).toHaveBeenCalledWith(authResult));
    await waitFor(() => expect(navigateMock).toHaveBeenCalledWith("/my"));
  });

  it("shows an error message when login fails", async () => {
    client.POST.mockResolvedValue({
      response: new Response(JSON.stringify({ detail: "Invalid email or password." }), {
        status: 401,
        headers: { "content-type": "application/json" },
      }),
    });
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText("Email"), "user@example.com");
    await user.type(screen.getByLabelText("Password"), "wrong-password");
    await user.click(screen.getByRole("button", { name: "Log in" }));

    expect(await screen.findByText("Invalid email or password.")).toBeInTheDocument();
    expect(navigateMock).not.toHaveBeenCalled();
  });
});
