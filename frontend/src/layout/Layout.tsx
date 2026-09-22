import { NavLink, Outlet } from "react-router-dom";
import { useApiKey } from "../auth/ApiKeyContext";

const navLinkStyle = ({ isActive }: { isActive: boolean }): React.CSSProperties => ({
  marginRight: "1rem",
  fontWeight: isActive ? "bold" : "normal",
});

/**
 * Deliberately unstyled nav shell (see ai/decisions/adr/0009-frontend-stack.md: technical
 * substance over visual polish). One link per Admin API resource area (Phase 14 fills these in)
 * plus the status page.
 */
export function Layout() {
  const { clearApiKey } = useApiKey();

  return (
    <div style={{ fontFamily: "sans-serif", padding: "1rem" }}>
      <header style={{ marginBottom: "1rem" }}>
        <h1 style={{ marginBottom: "0.5rem" }}>NotifyMe Admin</h1>
        <nav>
          <NavLink to="/admin" end style={navLinkStyle}>
            Status
          </NavLink>
          <NavLink to="/admin/alert-rules" style={navLinkStyle}>
            Alert Rules
          </NavLink>
          <NavLink to="/admin/channels" style={navLinkStyle}>
            Channels
          </NavLink>
          <NavLink to="/admin/subscriptions" style={navLinkStyle}>
            Subscriptions
          </NavLink>
          <NavLink to="/admin/notifications" style={navLinkStyle}>
            Notifications
          </NavLink>
          <button type="button" onClick={clearApiKey} style={{ marginLeft: "1rem" }}>
            Forget API key
          </button>
        </nav>
      </header>
      <main>
        <Outlet />
      </main>
    </div>
  );
}
