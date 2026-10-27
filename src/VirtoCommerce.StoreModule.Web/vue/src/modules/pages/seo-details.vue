<template>
  <VcBlade
    :title="item.semanticUrl || $t('STORES.PAGES.SEO.NEW_TITLE')"
    :toolbar-items="bladeToolbar"
    width="40%"
  >
    <VcContainer>
      <VcForm class="stores-form tw-space-y-4">
        <VcSwitch
          v-model="item.isActive"
          :label="$t('STORES.COMMON.IS_ACTIVE')"
        />
        <VcSelect
          v-model="item.languageCode"
          :label="$t('STORES.PAGES.SEO.FIELDS.LANGUAGE')"
          :options="languages"
        />
        <VcInput
          v-model="item.semanticUrl"
          :label="$t('STORES.PAGES.SEO.FIELDS.SEMANTIC_URL')"
          :error="!!urlError"
          :error-message="urlError"
          required
        />
        <VcInput
          v-model="item.pageTitle"
          :label="$t('STORES.PAGES.SEO.FIELDS.PAGE_TITLE')"
        />
        <VcTextarea
          v-model="item.metaDescription"
          :label="$t('STORES.PAGES.SEO.FIELDS.META_DESCRIPTION')"
        />
        <VcInput
          v-model="item.metaKeywords"
          :label="$t('STORES.PAGES.SEO.FIELDS.META_KEYWORDS')"
          maxlength="255"
        />
        <VcInput
          v-model="item.imageAltDescription"
          :label="$t('STORES.PAGES.SEO.FIELDS.IMAGE_ALT')"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcForm, VcInput, VcSelect, VcSwitch, VcTextarea } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import type { SeoInfo } from "../../api_client/virtocommerce.store";
import { toOptions } from "../api";
import { clone } from "../composables";

defineBlade({
  url: "/store-seo-record",
  name: "StoreSeoDetails",
});

const { t } = useI18n({ useScope: "global" });
const { options, callParent, closeSelf } = useBlade<{ seo: SeoInfo; index: number; languages: string[]; taken: string[] }>();

const item = ref<SeoInfo>(clone(options.value?.seo ?? { isActive: true }));
const languages = computed(() => toOptions(options.value?.languages));
const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("STORES.ALERTS.CLOSE_CONFIRMATION")) });
form.setBaseline();

// the AngularJS rules: no URL-reserved characters, unique within the store per language
const urlError = computed(() => {
  const url = item.value.semanticUrl ?? "";
  if (/[$+;=%{}[\]|\\/@ ~#!^*&?:'<>,]/.test(url)) return t("STORES.PAGES.SEO.ERRORS.INVALID");
  if (options.value?.taken.includes(`${item.value.languageCode ?? ""}|${url.toLowerCase()}`)) return t("STORES.PAGES.SEO.ERRORS.DUPLICATE");
  return "";
});

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "apply",
    title: t("STORES.TOOLBAR.APPLY"),
    icon: "lucide-check",
    disabled: computed(() => !item.value.semanticUrl || !!urlError.value || !form.isModified.value),
    async clickHandler() {
      await callParent("apply", { index: options.value?.index ?? -1, seo: clone(item.value) });
      form.setBaseline();
      await closeSelf();
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
</script>
