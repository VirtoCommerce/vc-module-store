<template>
  <VcBlade
    :title="$t('STORES.WIDGETS.DYNAMIC_PROPERTIES')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="stores-form tw-space-y-4">
        <VcCard :header="$t('STORES.WIDGETS.DYNAMIC_PROPERTIES')">
          <template
            v-if="languages.length > 1"
            #actions
          >
            <VcSelect
              v-model="currentLanguage"
              size="small"
              :options="languageOptions"
              :clearable="false"
              :aria-label="$t('STORES.PAGES.SEO.FIELDS.LANGUAGE')"
            />
          </template>
          <div class="stores-card">
            <VcHint v-if="!properties.length">{{ $t("STORES.PAGES.DYNAMIC_PROPERTIES.EMPTY") }}</VcHint>
            <VcDynamicProperty
              v-for="p in adapted"
              :key="p.id + currentLanguage"
              :property="p"
              :model-value="valueOf(p)"
              :options-getter="loadDictionaries"
              :required="!!p.required"
              :multivalue="p.multivalue"
              :multilanguage="p.multilanguage"
              :current-language="currentLanguage"
              :value-type="p.valueType ?? 'ShortText'"
              :dictionary="p.dictionary"
              :name="p.name ?? ''"
              :display-names="p.displayNames"
              @update:model-value="(ev: unknown) => onChange(p, ev)"
            />
          </div>
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { DynamicPropertiesClient, useApiClient, useDynamicProperties, type IBaseProperty } from "@vc-shell/framework";
import { VcBlade, VcCard, VcContainer, VcDynamicProperty, VcForm, VcHint, VcSelect } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import type { DynamicObjectProperty } from "../../api_client/virtocommerce.store";
import { toOptions } from "../api";
import { useStorePart } from "../composables";

defineBlade({
  url: "/store-dynamic-properties",
  name: "StoreDynamicProperties",
});

// the framework's dynamic property control works on IBaseProperty (catalog-like); platform dynamic properties map onto it
type Property = IBaseProperty & { required?: boolean; displayNames?: { name?: string; languageCode?: string }[] };

const { getApiClient } = useApiClient(DynamicPropertiesClient);

// the store's dynamic property values (Store.dynamicProperties), saved with the store
const { store, data: properties, loading, bladeToolbar, form } = useStorePart("dynamicProperties");
const languages = computed(() => store.value.languages ?? []);
const languageOptions = computed(() => toOptions(languages.value));
const currentLanguage = ref("en-US");

// the store holds only properties with values: list every property defined for stores (as the AngularJS widget did);
// the definitions load alongside the store (its objectType is the Store model's full name)
const definitions = getApiClient().then((c) => c.searchDynamicProperties({ objectType: "VirtoCommerce.StoreModule.Core.Model.Store", take: 1000 }));
watch(store, async () => {
  currentLanguage.value = store.value.defaultLanguage ?? languages.value[0] ?? "en-US";
  const defined = await definitions;
  const withValues = new Map(properties.value.map((p) => [p.id, p]));
  properties.value = (defined.results ?? []).map((d) => withValues.get(d.id) ?? ({ ...d, values: [] } as DynamicObjectProperty));
  form.setBaseline();
});

const { getPropertyValue, setPropertyValue, loadDictionaries } = useDynamicProperties({
  searchDictionary: async (criteria) =>
    (await (await getApiClient()).searchDictionaryItems({ ...criteria, take: criteria.take ?? 100 })).results?.map((x) => ({
      id: x.id,
      propertyId: x.propertyId,
      alias: x.name,
      value: x.name,
      localizedValues: x.displayNames?.map((d) => ({ languageCode: d.locale, value: d.name })),
    })),
});

const VALUE_TYPES: Record<string, string> = { Decimal: "Number", Html: "LongText", Json: "LongText", Undefined: "LongText", Image: "ShortText" };

const adapted = computed<Property[]>(() =>
  properties.value.map((p) => ({
    id: p.id,
    name: p.name,
    multilanguage: p.isMultilingual,
    multivalue: p.isArray,
    dictionary: p.isDictionary,
    required: p.isRequired,
    valueType: VALUE_TYPES[p.valueType ?? ""] ?? p.valueType,
    displayNames: p.displayNames?.map((d) => ({ name: d.name, languageCode: d.locale })),
    values: (p.values ?? []).map((v) => ({ value: v.value, languageCode: v.locale ?? undefined, valueId: v.valueId, propertyId: p.id })),
  })),
);

// a free-text multivalue (VcMultivalue) labels its chips by `title`; the values carry `value` (as the Catalog plugin does)
const isFreeMultivalue = (p: Property) => !!p.multivalue && !p.dictionary;

function valueOf(p: Property) {
  const value = getPropertyValue(p, currentLanguage.value);
  return isFreeMultivalue(p) && Array.isArray(value) ? value.map((v) => ({ ...(v as object), title: (v as { value?: unknown }).value })) : value;
}

function onChange(p: Property, ev: unknown) {
  const change = (ev && typeof ev === "object" && "value" in ev ? { ...ev } : { value: ev }) as { value: unknown };
  if (isFreeMultivalue(p) && Array.isArray(change.value)) {
    change.value = change.value.map(({ title, ...v }: { title?: unknown; value?: unknown }) => ({ ...v, value: v.value ?? title }));
  }
  setPropertyValue({ property: p, ...change } as Parameters<typeof setPropertyValue>[0]);
  // back to the platform shape
  const target = properties.value.find((x) => x.id === p.id)!;
  target.values = (p.values ?? []).map((v) => ({
    propertyId: target.id,
    propertyName: target.name,
    objectId: store.value.id,
    objectType: store.value.objectType,
    valueType: target.valueType,
    locale: v.languageCode,
    value: v.value,
    valueId: v.valueId,
  }));
}
</script>
