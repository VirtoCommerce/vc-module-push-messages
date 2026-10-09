<template>
  <div class="tw-space-y-2">
    <VcLabel required>{{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.LABEL") }}</VcLabel>

    <!-- relative: the loading overlay is absolute and covers its nearest positioned ancestor. -->
    <div class="tw-relative tw-flex tw-items-center tw-justify-between tw-gap-4 tw-rounded tw-border tw-border-[color:var(--neutrals-200)] tw-p-4">
      <VcLoading :active="loading" class="tw-inset-0 tw-rounded" />

      <div class="tw-flex tw-min-w-0 tw-items-center tw-gap-3">
        <span class="tw-flex tw-h-10 tw-w-10 tw-shrink-0 tw-items-center tw-justify-center tw-rounded tw-bg-[color:var(--primary-50)] tw-text-[color:var(--primary-600)]">
          <VcIcon icon="lucide-users" size="m" />
        </span>
        <div class="tw-min-w-0">
          <div class="tw-text-lg tw-font-semibold">
            {{ failed ? "—" : (total ?? 0) }} {{ $t(`${P}.RECIPIENTS`, total ?? 0) }}
          </div>
          <div class="tw-truncate tw-text-sm tw-text-[color:var(--neutrals-500)]">
            {{ sourceLine || $t(`${CARD}.NONE`) }}
          </div>
        </div>
      </div>

      <VcButton class="tw-shrink-0" variant="primary" icon="lucide-chevron-right" :disabled="disabled" @click="emit('open')">
        {{ $t(`${CARD}.${readonly ? "VIEW" : "SELECT"}`) }}
      </VcButton>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { VcButton, VcIcon, VcLabel, VcLoading } from "@vc-shell/framework/ui";
import { SUMMARY_PREFIX as P } from "../utils/audienceSummary";

defineProps<{
  total?: number;
  failed: boolean;
  loading: boolean;
  sourceLine: string;
  readonly: boolean;
  disabled?: boolean;
}>();

const emit = defineEmits<{ open: [] }>();

const CARD = "PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.CARD";
</script>
