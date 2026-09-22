import { useState, type FormEvent } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useMutation } from "@tanstack/react-query";
import { createPublicApiClient } from "../api/client";
import { unwrap } from "../api/apiError";
import type { LoginUserRequest } from "../api/types";
import { useMyAuth } from "../auth/MyAuthContext";
import { ErrorMessage } from "../components/ErrorMessage";

/**
 * End-user login screen (see ADR-0010). On success, stores the returned JWT via `MyAuthContext`
 * and sends the user to their alerts page. Entirely separate from the Admin API key flow
 * (src/auth/ApiKeyPrompt.tsx) - this is a different auth mechanism for a different audience.
 */
export function LoginPage() {
  const navigate = useNavigate();
  const { login } = useMyAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const loginMutation = useMutation({
    mutationFn: async (request: LoginUserRequest) =>
      unwrap(await createPublicApiClient().POST("/api/auth/login", { body: request })),
    onSuccess: (result) => {
      login(result);
      navigate("/my");
    },
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    loginMutation.mutate({ email, password });
  }

  return (
    <div style={{ maxWidth: "24rem", margin: "4rem auto", fontFamily: "sans-serif" }}>
      <h1>Log in</h1>
      <p>Manage your own alert rules, channels, and subscriptions.</p>
      <form onSubmit={handleSubmit} style={{ display: "flex", flexDirection: "column", gap: "0.5rem" }}>
        <label>
          Email{" "}
          <input
            type="email"
            required
            autoFocus
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            style={{ width: "100%", padding: "0.5rem", boxSizing: "border-box" }}
          />
        </label>
        <label>
          Password{" "}
          <input
            type="password"
            required
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            style={{ width: "100%", padding: "0.5rem", boxSizing: "border-box" }}
          />
        </label>
        <button type="submit" disabled={loginMutation.isPending} style={{ padding: "0.5rem 1rem" }}>
          Log in
        </button>
        <ErrorMessage error={loginMutation.error} />
      </form>
      <p>
        No account yet? <Link to="/register">Register</Link>
      </p>
    </div>
  );
}
