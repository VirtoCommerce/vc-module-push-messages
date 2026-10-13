<template>
  <div class="tw-space-y-2">
    <VcLabel required>{{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.LABEL") }}</VcLabel>

    <div
      class="tw-rounded tw-border tw-border-[color:var(--neutrals-200)] tw-divide-y tw-divide-[color:var(--neutrals-200)] tw-overflow-hidden"
      :class="{ 'tw-opacity-60 tw-pointer-events-none': disabled }"
    >
      <label
        v-for="option in MODES"
        :key="option.mode"
        class="tw-flex tw-items-start tw-gap-3 tw-p-4 tw-cursor-pointer tw-transition-colors"
        :class="
          modelValue === option.mode ? 'tw-bg-[color:var(--primary-50)] tw-ring-1 tw-ring-inset tw-ring-[color:var(--primary-500)]' : 'hover:tw-bg-[color:var(--neutrals-50)]'
        "
      >
        <VcRadioButton
          :model-value="modelValue"
          :value="option.mode"
          :disabled="disabled"
          @update:model-value="emit('update:modelValue', option.mode)"
        />
        <span class="tw-min-w-0">
          <span
            class="tw-flex tw-items-center tw-gap-2 tw-font-medium"
            :class="modelValue === option.mode ? 'tw-text-[color:var(--primary-700)]' : 'tw-text-[color:var(--neutrals-800)]'"
          >
            <VcIcon
              :icon="option.icon"
              size="m"
            />
            {{ $t(`${MODE_PREFIX}.${option.key}.TITLE`) }}
          </span>
          <span class="tw-block tw-mt-1 tw-text-sm tw-text-[color:var(--neutrals-500)]">
            {{ $t(`${MODE_PREFIX}.${option.key}.HINT`) }}
          </span>
        </span>
      </label>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { VcIcon, VcLabel, VcRadioButton } from "@vc-shell/framework/ui";
import type { AudienceMode } from "../utils/audienceQuery";

defineProps<{
  modelValue: AudienceMode;
  disabled?: boolean;
}>();

const emit = defineEmits<{
  "update:modelValue": [value: AudienceMode];
}>();

const MODE_PREFIX = "PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.MODES";
const MODES: { mode: AudienceMode; key: string; icon: string }[] = [
  { mode: "everyone", key: "EVERYONE", icon: "lucide-globe" },
  { mode: "list", key: "LIST", icon: "lucide-users" },
  { mode: "conditions", key: "CONDITIONS", icon: "lucide-filter" },
  { mode: "query", key: "QUERY", icon: "lucide-code" },
];
</script>
