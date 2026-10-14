import { StoreState } from "../api_client/virtocommerce.store";
import { api, toOptions } from "./api";

// Lookups that don't change within a session, built or fetched once (a failed fetch is retried next time)
export const STATE_OPTIONS = toOptions(Object.values(StoreState));
export const TIME_ZONE_OPTIONS = toOptions((Intl as unknown as { supportedValuesOf(key: string): string[] }).supportedValuesOf("timeZone"));
let countries: Promise<{ id: string; name: string }[]> | undefined;
export function loadCountries() {
  countries ??= api.get<{ id: string; name: string }[]>("/api/platform/common/countries").catch(() => ((countries = undefined), []));
  return countries;
}

// VcStatus variant for a store state (Open / RestrictedAccess / Closed)
export function stateVariant(state?: string): "success" | "warning" | "danger" | "info" {
  return state === "Open" ? "success" : state === "RestrictedAccess" ? "warning" : state === "Closed" ? "danger" : "info";
}

// The platform's merged module localization (the AngularJS admin's texts): titles of settings and names of
// payment/shipping/tax method types live there, keyed by setting/type name.
type Dict = Record<string, unknown>;
let legacy: Promise<Dict> | undefined;
export function loadLegacyTexts() {
  legacy ??= api.get<Dict>("/api/platform/localization?lang=en").catch(() => ({}));
  return legacy;
}

// keys mix dotted names and nesting ("settings" → "Stores" → "States", or "settings" → "VirtoCommerce.Core.General.TaxTypes")
function resolve(obj: unknown, parts: string[]): unknown {
  if (!parts.length) return obj;
  if (!obj || typeof obj !== "object") return undefined;
  for (let i = parts.length; i > 0; i--) {
    const next = (obj as Dict)[parts.slice(0, i).join(".")];
    if (next !== undefined) {
      const found = resolve(next, parts.slice(i));
      if (found !== undefined) return found;
    }
  }
  return undefined;
}

export function legacyText(texts: Dict, ...path: string[]): string | undefined {
  const value = resolve(texts, path.flatMap((p) => p.split(".")));
  return typeof value === "string" ? value : undefined;
}
