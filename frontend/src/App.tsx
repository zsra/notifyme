import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { ApiKeyGate } from "./auth/ApiKeyGate";
import { ApiKeyProvider } from "./auth/ApiKeyContext";
import { MyAuthGate } from "./auth/MyAuthGate";
import { MyAuthProvider } from "./auth/MyAuthContext";
import { Layout } from "./layout/Layout";
import { MyLayout } from "./layout/MyLayout";
import { AlertRulesPage } from "./pages/AlertRulesPage";
import { ChannelsPage } from "./pages/ChannelsPage";
import { LoginPage } from "./pages/LoginPage";
import { MyAlertsPage } from "./pages/MyAlertsPage";
import { NotificationsPage } from "./pages/NotificationsPage";
import { RegisterPage } from "./pages/RegisterPage";
import { StatusPage } from "./pages/StatusPage";
import { SubscriptionsPage } from "./pages/SubscriptionsPage";

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ApiKeyProvider>
        <MyAuthProvider>
          <BrowserRouter>
            <Routes>
              <Route path="/" element={<Navigate to="/login" replace />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route
                path="/my"
                element={
                  <MyAuthGate>
                    <MyLayout />
                  </MyAuthGate>
                }
              >
                <Route index element={<MyAlertsPage />} />
              </Route>
              <Route
                path="/admin"
                element={
                  <ApiKeyGate>
                    <Layout />
                  </ApiKeyGate>
                }
              >
                <Route index element={<StatusPage />} />
                <Route path="alert-rules" element={<AlertRulesPage />} />
                <Route path="channels" element={<ChannelsPage />} />
                <Route path="subscriptions" element={<SubscriptionsPage />} />
                <Route path="notifications" element={<NotificationsPage />} />
              </Route>
            </Routes>
          </BrowserRouter>
        </MyAuthProvider>
      </ApiKeyProvider>
    </QueryClientProvider>
  );
}

export default App;


