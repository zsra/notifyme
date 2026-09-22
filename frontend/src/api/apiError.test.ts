import { describe, it, expect } from "vitest";
import { unwrap, ApiError } from "./apiError";

describe("unwrap", () => {
  it("returns the data when the response is ok", async () => {
    const data = { id: "1" };
    const response = new Response(null, { status: 200 });

    await expect(unwrap({ data, response })).resolves.toBe(data);
  });

  it("throws an ApiError carrying the ProblemDetails detail when the response is not ok", async () => {
    function problemResponse() {
      return new Response(JSON.stringify({ detail: "Name is required." }), {
        status: 400,
        headers: { "content-type": "application/problem+json" },
      });
    }

    await expect(unwrap({ response: problemResponse() })).rejects.toBeInstanceOf(ApiError);
    await expect(unwrap({ response: problemResponse() })).rejects.toThrow("Name is required.");
  });

  it("falls back to a generic message when the response body isn't ProblemDetails JSON", async () => {
    const response = new Response("Internal Server Error", { status: 500 });

    await expect(unwrap({ response })).rejects.toThrow("Request failed with status 500");
  });
});
