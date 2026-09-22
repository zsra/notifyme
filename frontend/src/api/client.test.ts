import { describe, it, expect, vi, afterEach } from "vitest";
import { createAdminApiClient, createMyApiClient } from "./client";

describe("createAdminApiClient", () => {
  const originalFetch = globalThis.fetch;

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it("attaches the X-Api-Key header to every request", async () => {
    const fetchMock = vi.fn(
      async (_request: Request) =>
        new Response(JSON.stringify([]), { status: 200, headers: { "content-type": "application/json" } }),
    );
    globalThis.fetch = fetchMock as unknown as typeof fetch;

    const client = createAdminApiClient("test-key", vi.fn());
    await client.GET("/api/admin/alert-rules", { params: { query: {} } });

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const [request] = fetchMock.mock.calls[0];
    expect(request.headers.get("X-Api-Key")).toBe("test-key");
  });

  it("calls onUnauthorized when a response comes back 401", async () => {
    globalThis.fetch = vi.fn(async () => new Response(null, { status: 401 })) as unknown as typeof fetch;
    const onUnauthorized = vi.fn();

    const client = createAdminApiClient("bad-key", onUnauthorized);
    await client.GET("/api/admin/alert-rules", { params: { query: {} } });

    expect(onUnauthorized).toHaveBeenCalledTimes(1);
  });

  it("does not call onUnauthorized for a successful response", async () => {
    globalThis.fetch = vi.fn(
      async () =>
        new Response(JSON.stringify([]), { status: 200, headers: { "content-type": "application/json" } }),
    ) as unknown as typeof fetch;
    const onUnauthorized = vi.fn();

    const client = createAdminApiClient("good-key", onUnauthorized);
    await client.GET("/api/admin/alert-rules", { params: { query: {} } });

    expect(onUnauthorized).not.toHaveBeenCalled();
  });
});

describe("createMyApiClient", () => {
  const originalFetch = globalThis.fetch;

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it("attaches an Authorization bearer header to every request", async () => {
    const fetchMock = vi.fn(
      async (_request: Request) =>
        new Response(JSON.stringify([]), { status: 200, headers: { "content-type": "application/json" } }),
    );
    globalThis.fetch = fetchMock as unknown as typeof fetch;

    const client = createMyApiClient("test-token", vi.fn());
    await client.GET("/api/me/alert-rules", { params: { query: {} } });

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const [request] = fetchMock.mock.calls[0];
    expect(request.headers.get("Authorization")).toBe("Bearer test-token");
  });

  it("calls onUnauthorized when a response comes back 401", async () => {
    globalThis.fetch = vi.fn(async () => new Response(null, { status: 401 })) as unknown as typeof fetch;
    const onUnauthorized = vi.fn();

    const client = createMyApiClient("expired-token", onUnauthorized);
    await client.GET("/api/me/alert-rules", { params: { query: {} } });

    expect(onUnauthorized).toHaveBeenCalledTimes(1);
  });
});
