import { useState, type FormEvent } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useMutation } from "@tanstack/react-query";
import { createPublicApiClient } from "../api/client";
import { unwrap } from "../api/apiError";
import type { RegisterUserRequest } from "../api/types";
import { useMyAuth } from "../auth/MyAuthContext";
import { ErrorMessage } from "../components/ErrorMessage";

/**
 * End-user registration screen (see ADR-0010). Registration returns the same `AuthResultDto`
 * shape as login (see `Application/Users/RegisterUserUseCase.cs`), so a successful sign-up logs
 * the user straight in rather than requiring a separate login step.
 */
export function RegisterPage() {
  const navigate = useNavigate();
  const { login } = useMyAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const registerMutation = useMutation({
    mutationFn: async (request: RegisterUserRequest) =>
      unwrap(await createPublicApiClient().POST("/api/auth/register", { body: request })),
    onSuccess: (result) => {
      login(result);
      navigate("/my");
    },
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    registerMutation.mutate({ email, password });
  }

  return (
    <div style={{ maxWidth: "24rem", margin: "4rem auto", fontFamily: "sans-serif" }}>
      <h1>Register</h1>
      <p>Create an account to manage your own alert rules, channels, and subscriptions.</p>
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
          Password (min. 8 characters){" "}
          <input
            type="password"
            required
            minLength={8}
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            style={{ width: "100%", padding: "0.5rem", boxSizing: "border-box" }}
          />
        </label>
        <button type="submit" disabled={registerMutation.isPending} style={{ padding: "0.5rem 1rem" }}>
          Register
        </button>
        <ErrorMessage error={registerMutation.error} />
      </form>
      <p>
        Already have an account? <Link to="/login">Log in</Link>
      </p>
    </div>
  );
}
