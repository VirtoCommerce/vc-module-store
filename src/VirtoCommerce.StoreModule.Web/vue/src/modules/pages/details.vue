<template>
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="70%"
  >
    <VcContainer>
      <VcForm class="stores-form tw-space-y-4">
        <VcCard :header="$t('STORES.PAGES.DETAILS.GENERAL')">
          <div class="stores-card">
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcInput
                  v-model="item.name"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.NAME')"
                  :placeholder="$t('STORES.PAGES.DETAILS.PLACEHOLDERS.NAME')"
                  required
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-if="isNew"
                  v-model="item.id"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.CODE')"
                  :placeholder="$t('STORES.PAGES.DETAILS.PLACEHOLDERS.CODE')"
                  :error-message="item.id && !codeValid ? $t('STORES.PAGES.DETAILS.ERRORS.CODE') : undefined"
                  required
                />
                <VcField
                  v-else
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.CODE')"
                  :model-value="item.id"
                  copyable
                />
              </VcCol>
            </VcRow>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcSelect
                  v-model="item.catalog"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.CATALOG')"
                  :placeholder="$t('STORES.PAGES.DETAILS.PLACEHOLDERS.CATALOG')"
                  :options="searchCatalogs"
                  option-label="name"
                  searchable
                  required
                />
              </VcCol>
              <VcCol>
                <VcSelect
                  v-model="item.storeState"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.STATE')"
                  :options="STATE_OPTIONS"
                  :clearable="false"
                  required
                />
              </VcCol>
            </VcRow>
            <VcTextarea
              v-model="item.description"
              :label="$t('STORES.PAGES.DETAILS.FIELDS.DESCRIPTION')"
              :placeholder="$t('STORES.PAGES.DETAILS.PLACEHOLDERS.DESCRIPTION')"
            />
          </div>
        </VcCard>

        <VcCard :header="$t('STORES.PAGES.DETAILS.URLS')">
          <div class="stores-card">
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcInput
                  v-model="item.url"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.URL')"
                  placeholder="https://"
                  type="url"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model="item.secureUrl"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.SECURE_URL')"
                  placeholder="https://"
                  type="url"
                />
              </VcCol>
            </VcRow>
            <VcInput
              v-model="item.assetPublicUrl"
              :label="$t('STORES.PAGES.DETAILS.FIELDS.ASSET_URL')"
              :hint="$t('STORES.PAGES.DETAILS.HINTS.ASSET_URL')"
              placeholder="https://"
              type="url"
            />
          </div>
        </VcCard>

        <VcCard :header="$t('STORES.PAGES.DETAILS.LOCALIZATION')">
          <div class="stores-card">
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcSelect
                  v-model="item.defaultLanguage"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.DEFAULT_LANGUAGE')"
                  :options="languages"
                  :clearable="false"
                  searchable
                  required
                />
              </VcCol>
              <VcCol>
                <VcSelect
                  v-model="additionalLanguages"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.ADDITIONAL_LANGUAGES')"
                  :options="additionalLanguageOptions"
                  multiple
                  searchable
                />
              </VcCol>
            </VcRow>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcSelect
                  v-model="item.defaultCurrency"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.DEFAULT_CURRENCY')"
                  :options="currencies"
                  :clearable="false"
                  searchable
                  required
                />
              </VcCol>
              <VcCol>
                <VcSelect
                  v-model="additionalCurrencies"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.ADDITIONAL_CURRENCIES')"
                  :options="additionalCurrencyOptions"
                  multiple
                  searchable
                />
              </VcCol>
            </VcRow>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcSelect
                  v-model="item.country"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.COUNTRY')"
                  :options="countries"
                  option-label="name"
                  searchable
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model="item.region"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.REGION')"
                />
              </VcCol>
            </VcRow>
            <VcSelect
              v-model="item.timeZone"
              :label="$t('STORES.PAGES.DETAILS.FIELDS.TIME_ZONE')"
              :options="TIME_ZONE_OPTIONS"
              searchable
            />
          </div>
        </VcCard>

        <VcCard :header="$t('STORES.PAGES.DETAILS.EMAILS')">
          <div class="stores-card">
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcInput
                  v-model="item.emailName"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.EMAIL_NAME')"
                  maxlength="256"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model="item.email"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.EMAIL')"
                  type="email"
                  maxlength="128"
                />
              </VcCol>
            </VcRow>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcInput
                  v-model="item.adminEmailName"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.ADMIN_EMAIL_NAME')"
                  maxlength="256"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model="item.adminEmail"
                  :label="$t('STORES.PAGES.DETAILS.FIELDS.ADMIN_EMAIL')"
                  type="email"
                  maxlength="128"
                />
              </VcCol>
            </VcRow>
          </div>
        </VcCard>

        <VcCard :header="$t('STORES.PAGES.DETAILS.LINKS')">
          <div class="stores-card">
            <VcSelect
              v-model="item.trustedGroups"
              :label="$t('STORES.PAGES.DETAILS.FIELDS.LINKED_STORES')"
              :placeholder="$t('STORES.PAGES.DETAILS.PLACEHOLDERS.LINKED_STORES')"
              :options="searchStores"
              option-label="name"
              multiple
              searchable
            />
          </div>
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  defineBladeContext,
  useApiClient,
  useAsync,
  useBlade,
  useBladeForm,
  useLoading,
  usePopup,
  type IBladeToolbar,
} from "@vc-shell/framework";
import {
  VcBlade,
  VcCard,
  VcCol,
  VcContainer,
  VcField,
  VcForm,
  VcInput,
  VcRow,
  VcSelect,
  VcTextarea,
} from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { StoreModuleClient, StoreState, type Store } from "../../api_client/virtocommerce.store";
import { api, toOptions, type Option } from "../api";
import { loadCountries, STATE_OPTIONS, TIME_ZONE_OPTIONS } from "../utils";
import { useStoreWidgets } from "../widgets";

defineBlade({
  url: "/store",
  name: "StoreDetails",
});

const { t } = useI18n({ useScope: "global" });
const { param, callParent, closeSelf, replaceWith, exposeToChildren } = useBlade();
const { showConfirmation } = usePopup();
const { getApiClient } = useApiClient(StoreModuleClient);

const isNew = computed(() => !param.value);
const item = ref<Store>({ storeState: StoreState.Open, languages: [], currencies: [] });
const languages = ref<Option[]>([]);
const currencies = ref<Option[]>([]);
const countries = ref<{ id: string; name: string }[]>([]);

const codeValid = computed(() => /^[a-zA-Z0-9_-]+$/.test(item.value.id ?? ""));
const requiredFilled = computed(
  () => !!item.value.name && !!item.value.catalog && !!item.value.defaultLanguage && !!item.value.defaultCurrency && (!isNew.value || codeValid.value),
);

const form = useBladeForm({
  data: item,
  canSaveOverride: requiredFilled,
  closeConfirmMessage: computed(() => t("STORES.ALERTS.CLOSE_CONFIRMATION")),
});

const title = computed(() => (isNew.value ? t("STORES.PAGES.DETAILS.NEW_TITLE") : item.value.name ?? ""));

// languages/currencies hold the default one too (as the AngularJS blade did); the UI edits the additional ones
const additionalLanguages = computed({
  get: () => (item.value.languages ?? []).filter((x) => x !== item.value.defaultLanguage),
  set: (v: string[]) => (item.value.languages = v),
});
const additionalCurrencies = computed({
  get: () => (item.value.currencies ?? []).filter((x) => x !== item.value.defaultCurrency),
  set: (v: string[]) => (item.value.currencies = v),
});
// computed: a new array on every render would make the selects rebuild their option caches
const additionalLanguageOptions = computed(() => languages.value.filter((x) => x.id !== item.value.defaultLanguage));
const additionalCurrencyOptions = computed(() => currencies.value.filter((x) => x.id !== item.value.defaultCurrency));

// async VcSelect sources: (keyword, skip, ids) → { results, totalCount }
async function searchCatalogs(keyword?: string, skip = 0, ids?: string[]) {
  return api.post<{ results: unknown[]; totalCount: number }>("/api/catalog/catalogs/search", {
    keyword,
    objectIds: ids,
    skip,
    take: ids?.length || 20,
  });
}
async function searchStores(keyword?: string, skip = 0, ids?: string[]) {
  const result = await (await getApiClient()).searchStores({ keyword, objectIds: ids, skip, take: ids?.length || 20 });
  return { ...result, results: (result.results ?? []).filter((x) => x.id !== item.value.id) };
}

const { loading: itemLoading, action: load } = useAsync(async () => {
  item.value = await (await getApiClient()).getStoreById(param.value!);
});

const { loading: saveLoading, action: save } = useAsync(async () => {
  const store = item.value;
  store.languages = [...new Set([store.defaultLanguage!, ...(store.languages ?? [])])];
  store.currencies = [...new Set([store.defaultCurrency!, ...(store.currencies ?? [])])];
  const client = await getApiClient();
  if (isNew.value) {
    item.value = await client.createStore(store);
  } else {
    await client.updateStore(store);
    await load();
  }
});

const loading = useLoading(itemLoading, saveLoading);

async function saveAndRefresh() {
  await save();
  form.setBaseline();
  callParent("reload");
  if (isNew.value) {
    await replaceWith({ name: "StoreDetails", param: item.value.id });
  }
}

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: computed(() => t(isNew.value ? "STORES.TOOLBAR.CREATE" : "STORES.TOOLBAR.SAVE")),
    icon: "lucide-save",
    permissions: isNew.value ? "store:create" : "store:update",
    disabled: computed(() => !form.canSave.value),
    clickHandler: saveAndRefresh,
  },
  {
    id: "reset",
    title: t("STORES.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => form.revert(),
  },
  {
    id: "open",
    title: t("STORES.TOOLBAR.OPEN"),
    icon: "lucide-external-link",
    isVisible: computed(() => !isNew.value),
    disabled: computed(() => !item.value.url),
    clickHandler: () => window.open(item.value.url, "_blank"),
  },
  {
    id: "delete",
    title: t("STORES.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "store:delete",
    isVisible: computed(() => !isNew.value),
    async clickHandler() {
      if (await showConfirmation(t("STORES.ALERTS.DELETE_CONFIRMATION", { name: item.value.name }))) {
        await (await getApiClient()).deleteStore([item.value.id!]);
        form.setBaseline();
        callParent("reload");
        await closeSelf();
      }
    },
  },
]);

const { refreshAll: reloadWidgets } = useStoreWidgets(item, isNew);

// the store, for widgets other modules add to this blade (registerExternalWidget with targetBlades: ["StoreDetails"])
defineBladeContext({ item });

onMounted(async () => {
  await Promise.all([
    isNew.value ? Promise.resolve() : load(),
    // the values endpoint needs no settings permission (legacy settings.getValues); a failed lookup leaves the list empty
    api
      .get<unknown[]>("/api/platform/settings/values/VirtoCommerce.Core.General.Languages")
      .catch(() => [])
      .then((x) => (languages.value = toOptions(x))),
    api
      .get<{ code: string }[]>("/api/currencies")
      .catch(() => [])
      .then((x) => (currencies.value = x.map((c) => ({ id: c.code, title: c.code })))),
    loadCountries().then((x) => (countries.value = x)),
  ]);
  form.setBaseline();
});

// Re-reads one part of the store (e.g. "settings") and keeps the user's other unsaved edits: the baseline gets the
// fresh part too (revert puts the baseline into item), so only those edits stay modified.
async function refreshPart(key: keyof Store) {
  const fresh = await (await getApiClient()).getStoreById(param.value!);
  const edited = item.value;
  form.revert();
  item.value = { ...item.value, [key]: fresh[key] };
  form.setBaseline();
  item.value = { ...edited, [key]: fresh[key] };
}

// for child blades, also those opened from other modules' widgets: reloadWidgets refreshes the widget badges
exposeToChildren({
  reloadWidgets,
  async reload() {
    await load();
    form.setBaseline();
  },
  // a child blade that saved a part of the store itself (e.g. loyalty's store settings): callParent("refreshPart", "settings")
  refreshPart,
  // child blades that edit a part of the store (settings, SEO, dynamic properties) save it through here: only that
  // part, on the stored store, so the edits still unsaved here aren't saved with it
  async savePart({ key, value }: { key: keyof Store; value: unknown }) {
    const client = await getApiClient();
    await client.updateStore({ ...(await client.getStoreById(param.value!)), [key]: value } as Store);
    await refreshPart(key);
  },
});
</script>

<style lang="scss">
/* the design's blade body padding, as in the Orders detail; .stores-form and .stores-card are also the store's child blades' */
.stores-form {
  padding: 20px 22px 36px;
}

.stores-card {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 16px;
}
</style>
