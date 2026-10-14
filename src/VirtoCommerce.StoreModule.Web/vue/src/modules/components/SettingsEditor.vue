<template>
  <!-- Editors for platform ObjectSettingEntry values (the store's settings), grouped like the AngularJS settings blade -->
  <VcCard
    v-for="[group, rows] in groups"
    :key="group"
    :header="group"
    is-collapsable
  >
    <div class="stores-card">
      <template
        v-for="{ setting, label, hint, options } in rows"
        :key="setting.name"
      >
        <VcSwitch
          v-if="setting.valueType === SettingValueType.Boolean"
          :model-value="Boolean(current(setting))"
          :label="label"
          :hint="hint"
          :disabled="setting.isReadOnly"
          @update:model-value="(v?: boolean) => (setting.value = v)"
        />
        <!-- a vee-validate field, so a required setting left empty keeps the blade's Save disabled -->
        <Field
          v-else
          v-slot="{ errorMessage, handleChange, errors }"
          :name="setting.name!"
          :label="label"
          :rules="setting.isRequired && !setting.isReadOnly ? 'required' : undefined"
          :model-value="current(setting)"
        >
          <VcSelect
            v-if="options"
            :model-value="current(setting)"
            :label="label"
            :hint="hint"
            :options="options"
            :disabled="setting.isReadOnly"
            :required="setting.isRequired"
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="(v: unknown) => handleChange((setting.value = v))"
          />
          <VcTextarea
            v-else-if="setting.valueType === SettingValueType.LongText || setting.valueType === SettingValueType.Json"
            :model-value="text(setting)"
            :label="label"
            :hint="hint"
            :disabled="setting.isReadOnly"
            :required="setting.isRequired"
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="(v?: string) => handleChange((setting.value = v))"
          />
          <VcInput
            v-else
            :model-value="text(setting)"
            :label="label"
            :hint="hint"
            :type="inputType(setting.valueType)"
            :disabled="setting.isReadOnly"
            :required="setting.isRequired"
            :error="!!errors.length"
            :error-message="errorMessage"
            @update:model-value="(v: unknown) => handleChange((setting.value = toValue(setting.valueType, v)))"
          />
        </Field>
      </template>
    </div>
  </VcCard>
</template>

<script setup lang="ts">
import { VcCard, VcInput, VcSelect, VcSwitch, VcTextarea } from "@vc-shell/framework/ui";
import { Field } from "vee-validate";
import { computed, onMounted, shallowRef } from "vue";
import { SettingValueType, type ObjectSettingEntry } from "../../api_client/virtocommerce.store";
import { legacyText, loadLegacyTexts } from "../utils";

const props = defineProps<{ settings: ObjectSettingEntry[] }>();

const texts = shallowRef<Record<string, unknown>>({});
onMounted(async () => (texts.value = await loadLegacyTexts()));

// "Store|General" → "General"; labels, hints and option lists built once per settings/texts change, not per render
const groups = computed(() => {
  const map = new Map<string, { setting: ObjectSettingEntry; label: string; hint?: string; options?: { id: unknown; title: string }[] }[]>();
  for (const setting of props.settings.filter((x) => !x.isHidden)) {
    const group = setting.groupName?.split("|").pop() || "General";
    map.set(group, [
      ...(map.get(group) ?? []),
      {
        setting,
        label: legacyText(texts.value, "settings", setting.name!, "title") ?? setting.displayName ?? setting.name ?? "",
        hint: legacyText(texts.value, "settings", setting.name!, "description"),
        options: setting.allowedValues?.length ? setting.allowedValues.map((x) => ({ id: x, title: String(x) })) : undefined,
      },
    ]);
  }
  return [...map.entries()];
});

const current = (s: ObjectSettingEntry) => s.value ?? s.defaultValue;
const text = (s: ObjectSettingEntry) => {
  const v = current(s);
  return v === null || v === undefined ? "" : typeof v === "object" ? JSON.stringify(v) : String(v);
};

function inputType(valueType?: SettingValueType) {
  switch (valueType) {
    case SettingValueType.Integer:
    case SettingValueType.PositiveInteger:
    case SettingValueType.Decimal:
      return "number";
    case SettingValueType.DateTime:
      return "datetime-local";
    case SettingValueType.SecureString:
      return "password";
    default:
      return "text";
  }
}

function toValue(valueType: SettingValueType | undefined, v: unknown) {
  if (inputType(valueType) !== "number") return v;
  return v === "" || v === null || v === undefined ? null : Number(v);
}
</script>
