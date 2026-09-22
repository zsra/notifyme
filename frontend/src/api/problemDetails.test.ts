import { describe, it, expect } from "vitest";
import { parseProblemDetails } from "./problemDetails";

describe("parseProblemDetails", () => {
  it("parses a JSON ProblemDetails body", async () => {
    const response = new Response(
      JSON.stringify({ title: "Bad Request", status: 400, detail: "Name is required." }),
      { status: 400, headers: { "content-type": "application/problem+json" } },
    );

    await expect(parseProblemDetails(response)).resolves.toEqual({
      title: "Bad Request",
      status: 400,
      detail: "Name is required.",
    });
  });

  it("returns null when the response isn't JSON", async () => {
    const response = new Response("Internal Server Error", {
      status: 500,
      headers: { "content-type": "text/plain" },
    });

    await expect(parseProblemDetails(response)).resolves.toBeNull();
  });

  it("returns null when the body claims to be JSON but fails to parse", async () => {
    const response = new Response("not json", {
      status: 400,
      headers: { "content-type": "application/json" },
    });

    await expect(parseProblemDetails(response)).resolves.toBeNull();
  });
});
