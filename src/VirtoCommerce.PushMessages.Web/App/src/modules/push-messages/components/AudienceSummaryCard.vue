<template>
  <!-- relative: the loading overlay is absolute and covers its nearest positioned ancestor. -->
  <!-- Wraps rather than squeezes: below the text's basis the button takes a line of its own. -->
  <div class="tw-relative tw-flex tw-flex-wrap tw-items-center tw-justify-between tw-gap-4 tw-rounded tw-border tw-border-[color:var(--neutrals-200)] tw-p-4">
    <VcLoading
      :active="loading"
      class="tw-inset-0 tw-rounded"
    />

    <div class="tw-flex tw-min-w-0 tw-flex-[1_1_14rem] tw-items-center tw-gap-3">
      <span class="tw-flex tw-h-12 tw-w-12 tw-shrink-0 tw-items-center tw-justify-center tw-rounded tw-bg-[color:var(--primary-50)] tw-text-[color:var(--primary-600)]">
        <VcIcon
          icon="lucide-users"
          size="m"
        />
      </span>
      <div class="tw-min-w-0">
        <!-- Nothing to show until it is counted: a zero here would read as "nobody". -->
        <div
          class="tw-whitespace-nowrap tw-text-lg tw-font-semibold"
          :class="{ 'tw-invisible': loading && total === undefined }"
        >
          {{ failed ? "—" : (total ?? 0) }} {{ $t(`${P}.RECIPIENTS`, total ?? 0) }}
        </div>
        <div class="tw-text-sm tw-text-[color:var(--neutrals-500)]">
          {{ sourceLine || $t(`${CARD}.NONE`) }}
        </div>
      </div>
    </div>

    <VcButton
      class="tw-ml-auto tw-shrink-0"
      variant="primary"
      :disabled="disabled"
      @click="emit('open')"
    >
      {{ $t(`${CARD}.${readonly ? "VIEW" : "SELECT"}`) }}
      <VcIcon
        icon="lucide-chevron-right"
        size="s"
      />
    </VcButton>
  </div>
</template>

<script lang="ts" setup>
import { VcButton, VcIcon, VcLoading } from "@vc-shell/framework/ui";
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
