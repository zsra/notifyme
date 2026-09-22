import type { components } from "./schema";

/**
 * Re-exports of the generated DTO/request shapes used across the admin screens, so pages import
 * from one place instead of reaching into `components["schemas"][...]` everywhere.
 */
export type AlertRuleDto = components["schemas"]["AlertRuleDto"];
export type CreateAlertRuleRequest = components["schemas"]["CreateAlertRuleRequest"];
export type UpdateAlertRuleRequest = components["schemas"]["UpdateAlertRuleRequest"];

export type ChannelConfigDto = components["schemas"]["ChannelConfigDto"];
export type CreateChannelConfigRequest = components["schemas"]["CreateChannelConfigRequest"];
export type UpdateChannelConfigRequest = components["schemas"]["UpdateChannelConfigRequest"];

export type SubscriptionDto = components["schemas"]["SubscriptionDto"];
export type CreateSubscriptionRequest = components["schemas"]["CreateSubscriptionRequest"];

export type NotificationDto = components["schemas"]["NotificationDto"];

export type IngestEventsResult = components["schemas"]["IngestEventsResult"];

export type UserDto = components["schemas"]["UserDto"];
export type AuthResultDto = components["schemas"]["AuthResultDto"];
export type RegisterUserRequest = components["schemas"]["RegisterUserRequest"];
export type LoginUserRequest = components["schemas"]["LoginUserRequest"];
