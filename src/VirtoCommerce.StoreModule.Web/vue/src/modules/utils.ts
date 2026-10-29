import { i18n } from "@vc-shell/framework";
import { StoreState } from "../api_client/virtocommerce.store";
import { api, toOptions } from "./api";

// Lookups that don't change within a session, built or fetched once (a failed fetch is retried next time)
export const STATE_OPTIONS = toOptions(Object.values(StoreState));
export const TIME_ZONE_OPTIONS = toOptions((Intl as unknown as { supportedValuesOf(key: string): string[] }).supportedValuesOf("timeZone"));
let countries: Promise<{ id: string; name: string }[]> | undefined;
export function loadCountries() {
  countries ??= api.get<{ id: string; name: string }[]>("/api/platform/common/countries").catch(() => {
    countries = undefined;
    return [];
  });
  return countries;
}

// A store state's name in the UI's language (STORES.STATES), not the API's value
export const stateLabel = (state?: string) => (state ? i18n.global.t(`STORES.STATES.${state}`) : "");

// VcStatus variant for a store state (Open / RestrictedAccess / Closed)
export function stateVariant(state?: string): "success" | "warning" | "danger" | "info" {
  if (state === "Open") return "success";
  if (state === "RestrictedAccess") return "warning";
  if (state === "Closed") return "danger";
  return "info";
}
