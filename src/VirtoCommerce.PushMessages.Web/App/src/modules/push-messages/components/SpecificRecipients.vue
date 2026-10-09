<template>
  <VcSelect
    :model-value="modelValue"
    emit-value
    searchable
    multiple
    :clearable="false"
    option-value="id"
    option-label="name"
    :options="loadMembers"
    :disabled="disabled"
    :placeholder="$t(`${PICKER}.PLACEHOLDER`)"
    :hint="$t(`${PICKER}.HINT`)"
    @update:model-value="onUpdate"
  >
    <!-- Names repeat: the kind and the email are what tell two options apart before one is picked. -->
    <template #option="{ opt }">
      <span class="tw-flex tw-min-w-0 tw-items-center tw-gap-2">
        <VcStatus
          class="tw-shrink-0"
          :variant="isCompany(opt) ? 'primary' : 'info'"
        >
          {{ isCompany(opt) ? $t(`${PICKER}.COMPANY`) : $t(`${PICKER}.PERSON`) }}
        </VcStatus>
        <span class="tw-min-w-0 tw-truncate">{{ opt.name }}</span>
        <span
          v-if="opt.emails?.[0]"
          class="tw-min-w-0 tw-truncate tw-text-sm tw-text-[color:var(--neutrals-500)]"
        >
          {{ opt.emails[0] }}
        </span>
      </span>
    </template>
    <template #selected-item="{ opt, index, removeAtIndex }">
      <!-- A long name gives way first: the kind, the count and the remove button always stay whole. -->
      <span
        class="tw-inline-flex tw-max-w-full tw-min-w-0 tw-items-center tw-gap-2 tw-mr-2 tw-mb-1 tw-pl-2 tw-pr-1 tw-py-1 tw-rounded tw-border tw-border-[color:var(--primary-300)] tw-bg-[color:var(--primary-50)]"
      >
        <VcStatus
          class="tw-shrink-0"
          :variant="isCompany(opt) ? 'primary' : 'info'"
        >
          {{ isCompany(opt) ? $t(`${PICKER}.COMPANY`) : $t(`${PICKER}.PERSON`) }}
        </VcStatus>
        <span
          class="tw-min-w-0 tw-truncate tw-text-sm tw-text-[color:var(--neutrals-800)]"
          :title="opt.name"
          >{{ opt.name }}</span
        >
        <span
          v-if="countOf(opt) !== undefined"
          class="tw-shrink-0 tw-whitespace-nowrap tw-text-sm tw-text-[color:var(--neutrals-500)]"
        >
          · {{ $t(`${PICKER}.COUNT`, countOf(opt) as number) }}
        </span>
        <VcButton
          class="tw-shrink-0"
          icon="lucide-x"
          variant="ghost"
          size="icon-sm"
          :disabled="disabled"
          @click="removeAtIndex(index)"
        />
      </span>
    </template>
  </VcSelect>
</template>

<script lang="ts" setup>
import { VcButton, VcSelect, VcStatus } from "@vc-shell/framework/ui";
import type { Member, MemberSearchResult } from "../../../api_client/virtocommerce.customer";

export type MemberLoader = (keyword?: string, skip?: number, ids?: string[]) => Promise<MemberSearchResult>;

const props = defineProps<{
  modelValue: string[];
  /** Recipient count per picked member, so a chip can say what a company actually brings in. */
  counts: Record<string, number>;
  loadMembers: MemberLoader;
  disabled?: boolean;
}>();

const emit = defineEmits<{
  "update:modelValue": [value: string[]];
}>();

const PICKER = "PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.RECIPIENTS_PICKER";

function onUpdate(value: unknown) {
  emit("update:modelValue", (value as string[] | undefined) ?? []);
}

function isCompany(opt: Member): boolean {
  return opt.memberType === "Organization";
}

function countOf(opt: Member): number | undefined {
  return opt.id ? props.counts[opt.id] : undefined;
}
</script>
