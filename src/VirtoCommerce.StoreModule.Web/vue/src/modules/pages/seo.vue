<template>
  <VcBlade
    :title="$t('STORES.WIDGETS.SEO')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:selection="selection"
      selection-mode="multiple"
      :items="rows"
      :state-key="'STORE_SEO'"
      :active-item-id="activeItemId"
      :show-all-columns="false"
      :empty-state="{ icon: 'lucide-globe', title: $t('STORES.PAGES.SEO.EMPTY') }"
      @row-click="(e: { data: Row }) => openDetails(e.data.index)"
    >
      <VcColumn
        id="languageCode"
        :title="$t('STORES.PAGES.SEO.FIELDS.LANGUAGE')"
        width="110px"
        always-visible
      />
      <VcColumn
        id="semanticUrl"
        :title="$t('STORES.PAGES.SEO.FIELDS.SEMANTIC_URL')"
        always-visible
      />
      <VcColumn
        id="pageTitle"
        :title="$t('STORES.PAGES.SEO.FIELDS.PAGE_TITLE')"
      />
      <VcColumn
        id="isActive"
        :title="$t('STORES.COMMON.STATUS')"
        always-visible
      >
        <template #body="{ data: seo }">
          <div class="tw-inline-flex">
            <VcStatus :variant="seo.isActive ? 'success' : 'info'">
              {{ $t(seo.isActive ? "STORES.COMMON.ACTIVE" : "STORES.COMMON.INACTIVE") }}
            </VcStatus>
          </div>
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcStatus } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import type { SeoInfo } from "../../api_client/virtocommerce.store";
import { api, qs } from "../api";
import { useStorePart } from "../composables";

defineBlade({
  url: "/store-seo",
  name: "StoreSeo",
});

type Row = SeoInfo & { id: string; index: number };

const { t } = useI18n({ useScope: "global" });
const { param, openBlade, addBanner, exposeToChildren } = useBlade();

// the store's SEO records (Store.seoInfos), saved with the store like the AngularJS SEO blades
const { store, data: seoInfos, loading, bladeToolbar } = useStorePart("seoInfos");
const selection = ref<Row[]>([]);
const activeItemId = ref<string>();

const rows = computed<Row[]>(() => seoInfos.value.map((x, index) => ({ ...x, id: x.id ?? `new-${index}`, index })));

const toolbar = computed<IBladeToolbar[]>(() => [
  ...bladeToolbar.value,
  { id: "add", title: t("STORES.TOOLBAR.ADD"), icon: "lucide-plus", permissions: "store:update", clickHandler: () => openDetails(-1) },
  {
    id: "delete",
    title: t("STORES.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "store:update",
    disabled: !selection.value.length,
    clickHandler() {
      const removed = new Set(selection.value.map((x) => x.index));
      seoInfos.value = seoInfos.value.filter((_, i) => !removed.has(i));
      selection.value = [];
    },
  },
]);

function openDetails(index: number) {
  activeItemId.value = rows.value[index]?.id;
  openBlade({
    name: "StoreSeoDetails",
    param: rows.value[index]?.id,
    options: {
      seo: index >= 0 ? seoInfos.value[index] : { isActive: true, storeId: param.value, objectType: "Store", objectId: param.value },
      index,
      languages: store.value.languages ?? [],
      // semantic URLs must be unique within the store
      taken: seoInfos.value.filter((_, i) => i !== index).map((x) => `${x.languageCode ?? ""}|${x.semanticUrl?.toLowerCase()}`),
    },
    onClose: () => (activeItemId.value = undefined),
  });
}

onMounted(async () => {
  // duplicates are other objects with the same URL (the SEO module's check); resolving them isn't implemented server-side
  const duplicates = await api
    .get<unknown[]>(`/api/seoinfos/duplicates?${qs({ objectId: param.value, objectType: "Store" })}`)
    .catch(() => []);
  if (duplicates.length) addBanner({ variant: "warning", message: t("STORES.PAGES.SEO.DUPLICATES", { count: duplicates.length }) });
});

exposeToChildren({
  // applies an edited record to the list; Save on this blade saves the store
  apply({ index, seo }: { index: number; seo: SeoInfo }) {
    if (index >= 0) seoInfos.value[index] = seo;
    else seoInfos.value.push(seo);
  },
});
</script>
