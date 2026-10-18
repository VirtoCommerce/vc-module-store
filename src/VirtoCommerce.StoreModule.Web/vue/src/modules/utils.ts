import { i18n } from "@vc-shell/framework";
import { shallowRef, watch } from "vue";
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

// The platform's merged module localization (the AngularJS admin's texts): titles of settings and names of
// payment/shipping/tax method types live there, keyed by setting/type name. The server returns only the asked
// language's texts: the UI's language comes first, English fills what it lacks.
type Dict = Record<string, unknown>;
export type LegacyTexts = Dict[];

// the language the UI shows now (vue-i18n's global locale, a string or a ref)
export function currentLocale(): string {
  const locale = i18n.global.locale as unknown;
  return String((locale as { value?: string } | null)?.value ?? locale ?? "") || "en";
}

const legacy = new Map<string, Promise<Dict>>();
function loadLocale(locale: string) {
  let loaded = legacy.get(locale);
  if (!loaded) {
    loaded = api.get<Dict>(`/api/platform/localization?lang=${encodeURIComponent(locale)}`).catch(() => ({}));
    legacy.set(locale, loaded);
  }
  return loaded;
}

export function loadLegacyTexts(locale = currentLocale()): Promise<LegacyTexts> {
  return Promise.all(locale === "en" ? [loadLocale("en")] : [loadLocale(locale), loadLocale("en")]);
}

// the texts of the UI's language, reloaded when the user switches it
export function useLegacyTexts() {
  const texts = shallowRef<LegacyTexts>([]);
  watch(
    currentLocale,
    async (locale) => {
      const loaded = await loadLegacyTexts(locale);
      if (locale === currentLocale()) texts.value = loaded; // a later switch wins
    },
    { immediate: true },
  );
  return texts;
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

export function legacyText(texts: LegacyTexts, ...path: string[]): string | undefined {
  const parts = path.flatMap((p) => p.split("."));
  for (const dict of texts) {
    const value = resolve(dict, parts);
    if (typeof value === "string") return value;
  }
  return undefined;
}
