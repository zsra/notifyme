/**
 * Hand-written labels for the Domain enums exposed by the Admin API. These serialize as plain
 * numbers over the wire (System.Text.Json's default enum behavior), and the API's OpenAPI
 * document has no way to carry the C# member names alongside them, so - unlike the DTOs in
 * schema.d.ts - these maps cannot be generated and must be kept in sync by hand with:
 *   - src/NotifyMe.Domain/Events/EventCategory.cs
 *   - src/NotifyMe.Domain/Common/Severity.cs
 *   - src/NotifyMe.Domain/Notifications/NotificationStatus.cs
 */

export const EVENT_CATEGORY_LABELS: Record<number, string> = {
  0: "Breaking news",
  1: "Market movement",
  2: "Natural disaster",
};

export const SEVERITY_LABELS: Record<number, string> = {
  0: "Low",
  1: "Medium",
  2: "High",
  3: "Critical",
};

export const NOTIFICATION_STATUS_LABELS: Record<number, string> = {
  0: "Pending",
  1: "Sent",
  2: "Failed",
};

export function labelFor(map: Record<number, string>, value: number): string {
  return map[value] ?? `Unknown (${value})`;
}
