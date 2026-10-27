// Stores in the Admin Shell's global search (the header box, Ctrl+K): this module's provider, registered with the shell's
// registry (app.config.globalProperties.$globalSearch; see the shell's docs/global-search.md). There are few stores: they
// are loaded once and filtered in the browser. A result opens the store over the stores list, as the list opens it.
import type { App } from "vue";
import type { Store } from "../api_client/virtocommerce.store";

/** The part of the shell's global search registry this module uses */
interface GlobalSearch {
  registerProvider(provider: object): void;
  hasPermission(permission: string | string[]): boolean;
  rankItems<T>(items: T[], query: string, fields: (item: T) => unknown[], take?: number): T[];
  openBlade(options: {
    route: string;
    workspace: string;
    blades?: { name: string; param?: string; options?: Record<string, unknown> }[];
  }): Promise<void>;
}

/** Registers the Stores search provider; nothing in a host without global search */
export function registerStoreSearch(app: App) {
  const search = app.config.globalProperties.$globalSearch as GlobalSearch | undefined;
  if (!search) return;

  let stores: Promise<Store[]> | undefined;
  const loadStores = () =>
    (stores ??= fetch("/api/stores/search", {
      method: "POST",
      headers: { Accept: "application/json", "Content-Type": "application/json" },
      body: JSON.stringify({ take: 1000, skip: 0 }),
    })
      .then((response) => {
        if (!response.ok) throw new Error(`Stores search failed: ${response.status}`);
        return response.json() as Promise<{ results?: Store[] }>;
      })
      .then(
        (result) => result.results ?? [],
        (error) => {
          // the next search asks again
          stores = undefined;
          throw error;
        },
      ));

  search.registerProvider({
    id: "stores",
    title: "STORES.SEARCH.GROUP_TITLE",
    icon: "lucide-store",
    order: 130,
    permission: "store:read",
    isAvailable: () => search.hasPermission("store:access"),
    aliases: ["store", "stores"],
    search: async (query: string, context: { take: number }) =>
      search
        .rankItems(await loadStores(), query, (s) => [s.name, s.id, s.url, s.secureUrl], context.take)
        .map((store) => ({
          id: store.id,
          title: store.name ?? store.id,
          description: [store.id !== store.name ? store.id : undefined, store.secureUrl || store.url]
            .filter(Boolean)
            .join(" · "),
          icon: "lucide-store",
          badge: store.storeState && store.storeState !== "Open" ? store.storeState : undefined,
          execute: () =>
            search.openBlade({
              route: "/stores",
              workspace: "StoresList",
              blades: [{ name: "StoreDetails", param: store.id }],
            }),
        })),
  });
}
