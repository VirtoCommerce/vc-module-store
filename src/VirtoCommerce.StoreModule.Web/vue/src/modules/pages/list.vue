<template>
  <VcBlade
    :title="$t('STORES.PAGES.LIST.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:search-value="searchValue"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :searchable="true"
      :state-key="'STORES'"
      :active-item-id="activeItemId"
      :show-all-columns="!activeItemId"
      :global-filters="globalFilters"
      :row-actions="rowActions"
      :empty-state="{ icon: 'lucide-store', title: $t('STORES.PAGES.LIST.EMPTY') }"
      @row-click="(e: { data: Store }) => openDetails(e.data.id)"
      @pagination-click="pagination.goToPage"
      @filter="onFilter"
    >
      <VcColumn
        id="name"
        :title="$t('STORES.PAGES.LIST.COLUMNS.NAME')"
        body-class="stores-list__name"
        sortable
        always-visible
      />
      <VcColumn
        id="id"
        :title="$t('STORES.PAGES.LIST.COLUMNS.CODE')"
        sortable
      />
      <VcColumn
        id="url"
        :title="$t('STORES.PAGES.LIST.COLUMNS.URL')"
      />
      <VcColumn
        id="defaultLanguage"
        :title="$t('STORES.PAGES.LIST.COLUMNS.LANGUAGE')"
      />
      <VcColumn
        id="defaultCurrency"
        :title="$t('STORES.PAGES.LIST.COLUMNS.CURRENCY')"
      />
      <VcColumn
        id="storeState"
        :title="$t('STORES.PAGES.LIST.COLUMNS.STATE')"
        always-visible
        sortable
      >
        <template #body="{ data: store }">
          <div class="tw-inline-flex">
            <VcStatus :variant="stateVariant(store.storeState)">{{ store.storeState }}</VcStatus>
          </div>
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import {
  useApiClient,
  useAsync,
  useBlade,
  useDataTablePagination,
  useDataTableSort,
  useFunctions,
  useLatestRequest,
  useTableSearch,
  type IBladeToolbar,
} from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcStatus } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { StoreModuleClient, StoreState, type Store } from "../../api_client/virtocommerce.store";
import { stateVariant } from "../utils";

defineBlade({
  url: "/stores",
  name: "StoresList",
  isWorkspace: true,
  permissions: ["store:access"],
  menuItem: {
    title: "STORES.MENU.TITLE",
    icon: "lucide-store",
    priority: 10,
  },
});

const { t } = useI18n({ useScope: "global" });
const { openBlade, exposeToChildren } = useBlade();
const { debounce } = useFunctions();
const { getApiClient } = useApiClient(StoreModuleClient);

const PAGE_SIZE = 20;
const items = ref<Store[]>([]);
const totalCount = ref(0);
const activeItemId = ref<string>();
const storeState = ref<StoreState>();

const { sortField, sortOrder, sortExpression } = useDataTableSort({ stateKey: "STORES", initialField: "name", initialDirection: "ASC" });
const { searchValue } = useTableSearch({ stateKey: "STORES" });
const pagination = useDataTablePagination({ stateKey: "STORES", pageSize: PAGE_SIZE, totalCount });
const search = useLatestRequest();

const { loading, action: load } = useAsync(async () => {
  const client = await getApiClient();
  const result = await search.latest(
    client.searchStores({
      keyword: searchValue.value || undefined,
      storeStates: storeState.value ? [storeState.value] : undefined,
      sort: sortExpression.value,
      skip: pagination.skip,
      take: PAGE_SIZE,
    }),
  );
  if (!result) return; // superseded by a newer search
  items.value = result.results ?? [];
  totalCount.value = result.totalCount ?? 0;
});

const globalFilters = computed(() => [
  {
    id: "storeState",
    label: t("STORES.PAGES.LIST.COLUMNS.STATE"),
    filter: { options: Object.values(StoreState).map((s) => ({ value: s, label: s })) },
  },
]);

function onFilter(event: { filters: Record<string, unknown> }) {
  storeState.value = (event.filters.storeState as StoreState) || undefined;
}

load();
watch([searchValue, storeState], () => pagination.setPage(1));
watch([sortExpression, searchValue, storeState, () => pagination.skip], debounce(load, 300));

const copy = (text?: string) => text && navigator.clipboard.writeText(text);

const rowActions = (store: Store) => [
  { id: "copyCode", type: "info" as const, title: t("STORES.PAGES.LIST.ACTIONS.COPY_CODE"), icon: "lucide-copy", clickHandler: () => copy(store.id) },
  { id: "copyUrl", type: "info" as const, title: t("STORES.PAGES.LIST.ACTIONS.COPY_URL"), icon: "lucide-link", hidden: !store.url, clickHandler: () => copy(store.url) },
  {
    id: "open",
    type: "info" as const,
    title: t("STORES.PAGES.LIST.ACTIONS.OPEN"),
    icon: "lucide-external-link",
    hidden: !store.url,
    clickHandler: () => window.open(store.url, "_blank"),
  },
];

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("STORES.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
  { id: "add", title: t("STORES.TOOLBAR.ADD"), icon: "lucide-plus", permissions: "store:create", clickHandler: () => openDetails() },
]);

function openDetails(id?: string) {
  activeItemId.value = id;
  openBlade({
    name: "StoreDetails",
    param: id,
    onClose() {
      activeItemId.value = undefined;
    },
  });
}

exposeToChildren({ reload: () => load() });
</script>

<style lang="scss">
.stores-list__name {
  color: var(--neutrals-900);
}
</style>
