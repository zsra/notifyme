import { Outlet, useNavigate } from "react-router-dom";
import { useMyAuth } from "../auth/MyAuthContext";

/**
 * Nav shell for the end-user self-service area (`/my/*`), separate from the Admin panel's
 * `Layout` (see src/layout/Layout.tsx). Deliberately minimal: a single consolidated page (see
 * ai/plan/phase-16-user-self-service.md step 6) doesn't need its own nav links yet.
 */
export function MyLayout() {
  const { user, logout } = useMyAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/login");
  }

  return (
    <div style={{ fontFamily: "sans-serif", padding: "1rem" }}>
      <header style={{ marginBottom: "1rem", display: "flex", alignItems: "baseline", gap: "1rem" }}>
        <h1 style={{ margin: 0 }}>My Alerts</h1>
        {user && <span>{user.email}</span>}
        <button type="button" onClick={handleLogout}>
          Log out
        </button>
      </header>
      <main>
        <Outlet />
      </main>
    </div>
  );
}
