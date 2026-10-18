<template>
  <VcBlade
    :title="$t('STORES.WIDGETS.SETTINGS')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <div class="stores-form tw-space-y-4">
        <SettingsEditor :settings="settings" />
      </div>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { VcBlade, VcContainer } from "@vc-shell/framework/ui";
import SettingsEditor from "../components/SettingsEditor.vue";
import { useStorePart } from "../composables";

defineBlade({
  url: "/store-settings",
  name: "StoreSettings",
});

// module settings of the store (Store.settings), saved with the store like the AngularJS entity settings blade
const { data: settings, loading, bladeToolbar } = useStorePart("settings");
</script>
