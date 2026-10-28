import { notification, useBlade, useBladeWidgets, usePermissions } from "@vc-shell/framework";
import { computed, type Ref } from "vue";
import { useI18n } from "vue-i18n";
import type { Store } from "../api_client/virtocommerce.store";

// The store blade's own widgets (the ones the AngularJS Store module registered into "storeDetail").
// Other modules add theirs to this blade from their own plugins (registerExternalWidget, targetBlades: ["StoreDetails"]).
export function useStoreWidgets(item: Ref<Store>, isNew: Ref<boolean>) {
  const { t } = useI18n({ useScope: "global" });
  const { openBlade } = useBlade();
  const { hasAccess } = usePermissions();
  const saved = computed(() => !isNew.value);
  const open = (name: string) => () => openBlade({ name, param: item.value.id });
  // a blade of another module (Notifications, Assets), by name: it isn't registered when that module's UI isn't installed
  const openOther = (event: Parameters<typeof openBlade>[0]) =>
    openBlade(event).catch(() => notification.error(t("STORES.ALERTS.MODULE_UNAVAILABLE"), { timeout: 5000 }));

  return useBladeWidgets([
    {
      id: "StoreSettings",
      icon: "lucide-settings",
      title: "STORES.WIDGETS.SETTINGS",
      badge: computed(() => item.value.settings?.length ?? 0),
      isVisible: saved,
      // Admin Shell's settings blade, connected to the store (docs/object-settings.md in vc-module-admin-shell): it reads
      // the store's settings from this blade (getEntitySettings) and the store saves the changed ones (applyEntitySettings)
      onClick: () =>
        openOther({
          name: "PlatformObjectSettings",
          options: {
            tenantType: "Store",
            tenantId: item.value.id,
            mode: "connected",
            canUpdate: hasAccess("store:update"),
            title: t("STORES.WIDGETS.SETTINGS"),
            subtitle: item.value.name,
          },
        }),
    },
    {
      id: "StoreSeo",
      icon: "lucide-globe",
      title: "STORES.WIDGETS.SEO",
      badge: computed(() => item.value.seoInfos?.length ?? 0),
      isVisible: saved,
      onClick: open("StoreSeo"),
    },
    {
      id: "StoreDynamicProperties",
      icon: "lucide-list-plus",
      title: "STORES.WIDGETS.DYNAMIC_PROPERTIES",
      badge: computed(() => item.value.dynamicProperties?.length ?? 0),
      isVisible: saved,
      onClick: open("StoreDynamicProperties"),
    },
    {
      id: "StoreAuthentication",
      icon: "lucide-key-round",
      title: "STORES.WIDGETS.AUTHENTICATION",
      isVisible: saved,
      onClick: open("StoreAuthenticationSchemes"),
    },
    // the Notifications module's list for the store as its tenant (each type's store version: templates, layouts,
    // preview), as the AngularJS widget opened it; the log is that module's journal filtered to the store
    {
      id: "StoreNotifications",
      icon: "lucide-bell",
      title: "STORES.WIDGETS.NOTIFICATIONS",
      isVisible: saved,
      onClick: () =>
        openOther({ name: "NotificationsList", options: { tenantId: item.value.id, tenantType: "Store", tenantName: item.value.name } }),
    },
    {
      id: "StoreNotificationLog",
      icon: "lucide-mail",
      title: "STORES.WIDGETS.NOTIFICATION_LOG",
      isVisible: saved,
      onClick: () =>
        openOther({
          name: "NotificationsJournal",
          // the journal shows the title as its subtitle
          options: { objectId: item.value.id, objectType: "Store", title: item.value.name },
        }),
    },
    // the Assets module's browser, rooted at the store's folder, as the AngularJS widget opened it
    {
      id: "StoreAssets",
      icon: "lucide-folder",
      title: "STORES.WIDGETS.ASSETS",
      isVisible: saved,
      onClick: () => openOther({ name: "AssetsBrowser", options: { folderUrl: `/stores/${item.value.id}`, title: item.value.name } }),
    },
  ]);
}
