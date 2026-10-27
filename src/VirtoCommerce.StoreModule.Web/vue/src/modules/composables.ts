import { useApiClient, useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { computed, onMounted, ref, type Ref } from "vue";
import { useI18n } from "vue-i18n";
import { StoreModuleClient, type Store } from "../api_client/virtocommerce.store";

export const clone = <T>(value: T): T => JSON.parse(JSON.stringify(value ?? null)) ?? value;

// A child blade (param = store id) editing one part of the store (settings, SEO infos, dynamic properties) on its own
// copy; Save hands the part to the store blade, which saves the store (the part has no API of its own).
export function useStorePart<K extends keyof Store>(key: K) {
  const { t } = useI18n({ useScope: "global" });
  const { param, callParent } = useBlade();
  const { getApiClient } = useApiClient(StoreModuleClient);
  const store = ref<Store>({});
  const data = ref([]) as unknown as Ref<NonNullable<Store[K]>>;
  const loading = ref(false);
  const form = useBladeForm({ data, closeConfirmMessage: computed(() => t("STORES.ALERTS.CLOSE_CONFIRMATION")) });

  async function load() {
    loading.value = true;
    try {
      store.value = await (await getApiClient()).getStoreById(param.value!);
      data.value = clone(store.value[key] ?? []) as NonNullable<Store[K]>;
      form.setBaseline();
    } finally {
      loading.value = false;
    }
  }
  onMounted(load);

  const bladeToolbar = ref<IBladeToolbar[]>([
    {
      id: "save",
      title: t("STORES.TOOLBAR.SAVE"),
      icon: "lucide-save",
      permissions: "store:update",
      disabled: computed(() => !form.canSave.value),
      async clickHandler() {
        loading.value = true;
        try {
          await callParent("savePart", { key, value: clone(data.value) });
          form.setBaseline();
        } finally {
          loading.value = false;
        }
      },
    },
    {
      id: "reset",
      title: t("STORES.TOOLBAR.RESET"),
      icon: "lucide-undo-2",
      disabled: computed(() => !form.isModified.value),
      clickHandler: () => form.revert(),
    },
  ]);

  return { store, data, loading, bladeToolbar, form };
}
