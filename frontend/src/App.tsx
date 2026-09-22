import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import { ApiKeyGate } from "./auth/ApiKeyGate";
import { ApiKeyProvider } from "./auth/ApiKeyContext";
import { Layout } from "./layout/Layout";
import { AlertRulesPage } from "./pages/AlertRulesPage";
import { ChannelsPage } from "./pages/ChannelsPage";
import { NotificationsPage } from "./pages/NotificationsPage";
import { StatusPage } from "./pages/StatusPage";
import { SubscriptionsPage } from "./pages/SubscriptionsPage";

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ApiKeyProvider>
        <ApiKeyGate>
          <BrowserRouter>
            <Routes>
              <Route path="/" element={<Layout />}>
                <Route index element={<StatusPage />} />
                <Route path="alert-rules" element={<AlertRulesPage />} />
                <Route path="channels" element={<ChannelsPage />} />
                <Route path="subscriptions" element={<SubscriptionsPage />} />
                <Route path="notifications" element={<NotificationsPage />} />
              </Route>
            </Routes>
          </BrowserRouter>
        </ApiKeyGate>
      </ApiKeyProvider>
    </QueryClientProvider>
  );
}

export default App;

