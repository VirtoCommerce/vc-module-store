<template>
  <VcBlade
    :title="$t('STORES.WIDGETS.AUTHENTICATION')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="40%"
  >
    <!-- which sign-in schemes the store's frontend offers -->
    <VcDataTable
      :items="rows"
      :state-key="'STORE_AUTH_SCHEMES'"
      :empty-state="{ icon: 'lucide-key-round', title: $t('STORES.PAGES.AUTH.EMPTY') }"
    >
      <VcColumn
        id="displayName"
        :title="$t('STORES.COMMON.NAME')"
        always-visible
      />
      <VcColumn
        id="isActive"
        :title="$t('STORES.COMMON.IS_ACTIVE')"
        width="120px"
      >
        <template #body="{ data: row }">
          <VcSwitch
            :model-value="schemes[row.index].isActive"
            @update:model-value="(v?: boolean) => (schemes[row.index].isActive = !!v)"
          />
        </template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useApiClient, useAsync, useBlade, useBladeForm, useLoading, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable, VcSwitch } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { StoreAuthenticationSchemeClient, type StoreAuthenticationScheme } from "../../api_client/virtocommerce.store";

defineBlade({
  url: "/store-authentication",
  name: "StoreAuthenticationSchemes",
});

const { t } = useI18n({ useScope: "global" });
const { param } = useBlade();
const { getApiClient } = useApiClient(StoreAuthenticationSchemeClient);

const schemes = ref<StoreAuthenticationScheme[]>([]);
const rows = computed(() => schemes.value.map((x, index) => ({ ...x, id: x.name, index })));
const form = useBladeForm({ data: schemes, closeConfirmMessage: computed(() => t("STORES.ALERTS.CLOSE_CONFIRMATION")) });

const { loading: fetching, action: load } = useAsync(async () => {
  schemes.value = await (await getApiClient()).get(param.value!);
  form.setBaseline();
});

const { loading: saving, action: save } = useAsync(async () => {
  await (await getApiClient()).update(param.value!, schemes.value);
  await load();
});

const loading = useLoading(fetching, saving);

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: t("STORES.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: "store:update",
    disabled: computed(() => !form.canSave.value),
    clickHandler: () => save(),
  },
  {
    id: "reset",
    title: t("STORES.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => form.revert(),
  },
]);

onMounted(load);
</script>
