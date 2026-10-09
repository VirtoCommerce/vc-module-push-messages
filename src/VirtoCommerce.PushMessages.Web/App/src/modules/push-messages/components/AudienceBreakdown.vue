<template>
  <!-- relative: the loading overlay is absolute and covers its nearest positioned ancestor. -->
  <div class="tw-relative tw-rounded tw-border tw-border-[color:var(--neutrals-200)] tw-p-4 tw-space-y-4">
    <VcLoading
      :active="estimate.loading"
      class="tw-inset-0 tw-rounded"
    />

    <div class="tw-flex tw-items-start tw-justify-between tw-gap-3">
      <div class="tw-flex tw-min-w-0 tw-items-center tw-gap-3">
        <span class="tw-flex tw-h-10 tw-w-10 tw-shrink-0 tw-items-center tw-justify-center tw-rounded tw-bg-[color:var(--primary-50)] tw-text-[color:var(--primary-600)]">
          <VcIcon
            icon="lucide-users"
            size="m"
          />
        </span>
        <div class="tw-min-w-0">
          <div class="tw-text-xl tw-font-semibold">{{ estimate.failed ? "—" : total }} {{ $t(`${P}.RECIPIENTS`, total) }}</div>
          <div class="tw-truncate tw-text-sm tw-text-[color:var(--neutrals-500)]">{{ sourceLine }}</div>
        </div>
      </div>
      <VcStatus
        v-if="!readonly"
        class="tw-shrink-0"
        :variant="STATUS_VARIANT[status]"
      >
        {{ $t(`${P}.STATUS.${status}`) }}
      </VcStatus>
    </div>

    <VcHint v-if="readonly && sentCount !== undefined">{{ $t(`${P}.SENT_HINT`, sentCount) }}</VcHint>

    <VcHint
      v-if="estimate.failed"
      error
    >
      {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.FAILED") }}
    </VcHint>

    <template v-else-if="estimate.result && addsUp(estimate.result)">
      <div class="tw-flex tw-items-stretch tw-gap-1 tw-rounded tw-bg-[color:var(--neutrals-50)] tw-p-2">
        <template
          v-for="(step, i) in steps"
          :key="step.key"
        >
          <VcIcon
            v-if="i > 0"
            icon="lucide-chevron-right"
            size="s"
            class="tw-self-center tw-shrink-0 tw-text-[color:var(--neutrals-400)]"
          />
          <div
            class="tw-flex-1 tw-min-w-0 tw-rounded tw-p-2 tw-text-center"
            :class="i === steps.length - 1 ? 'tw-bg-[color:var(--primary-500)] tw-text-white' : ''"
          >
            <div
              class="tw-text-2xl tw-font-semibold"
              :class="{ 'tw-text-[color:var(--primary-500)]': step.signed }"
            >
              {{ step.signed ? `+${step.value}` : step.value }}
            </div>
            <div
              class="tw-text-xs"
              :class="i === steps.length - 1 ? '' : 'tw-text-[color:var(--neutrals-500)]'"
            >
              {{ $t(`${P}.STEPS.${step.key}`) }}
            </div>
          </div>
        </template>
      </div>

      <p
        v-if="notes.length"
        class="tw-text-sm tw-text-[color:var(--neutrals-600)]"
      >
        <template
          v-for="(note, i) in notes"
          :key="note.key"
          ><span v-if="i > 0"> · </span><span class="tw-font-semibold tw-text-[color:var(--neutrals-900)]">{{ $t(`${P}.NOTE.${note.key}`, note.count) }}</span
          >{{ note.key === "OVERLAPS" ? " " : "" }}{{ $t(`${P}.NOTE.${note.key}_REASON`) }}</template
        >
      </p>
    </template>

    <div
      v-if="hasAudience"
      class="tw-flex tw-gap-2"
    >
      <VcButton
        v-if="!estimate.failed"
        variant="primary"
        size="sm"
        icon="lucide-eye"
        @click="openPreview"
      >
        {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.PREVIEW") }}
      </VcButton>
      <VcButton
        variant="outline"
        size="sm"
        icon="lucide-code"
        @click="showQuery = true"
      >
        {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.SHOW_QUERY") }}
      </VcButton>
    </div>

    <VcPopup
      v-model="showPreview"
      modal-width="tw-w-full tw-max-w-4xl"
      :title="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.PREVIEW_TITLE')"
    >
      <template #content>
        <!-- Laid out as a blade lays out its table: the lead keeps its line and the table fills the
             rest, scrolling its own body — VcDataTable takes 100% of its container, so it gets one of
             its own. The popup's inner box is as wide as its content, and the table spreads its
             columns over the width it gets, so once a scrollbar appeared each widened the other
             without end. contain: inline-size stops this block following its content; the popup's
             width comes from modal-width instead, as a blade's comes from its own width. -->
        <div class="tw-w-full tw-flex tw-flex-col tw-min-h-0 [contain:inline-size]">
          <p class="tw-mb-3 tw-shrink-0 tw-text-sm tw-text-[color:var(--neutrals-600)]">
            {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.PREVIEW_LEAD", total) }}
          </p>
          <VcLoading
            v-if="loadingPage && !previewRows.length"
            active
          />
          <p
            v-else-if="previewFailed"
            class="tw-text-sm tw-text-[color:var(--danger-500)]"
          >
            {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.PREVIEW_FAILED") }}
          </p>
          <p
            v-else-if="!previewRows.length"
            class="tw-text-sm tw-text-[color:var(--neutrals-500)]"
          >
            {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.PREVIEW_EMPTY") }}
          </p>
          <div
            v-else
            class="tw-flex tw-flex-col tw-flex-1 tw-min-h-0"
          >
            <VcDataTable
              :items="previewRows"
              :loading="loadingPage"
              :total-count="previewPagination.totalCount"
              :pagination="previewPagination"
              @pagination-click="previewPagination.goToPage"
            >
              <VcColumn
                id="name"
                field="name"
                :width="280"
                :title="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.COLUMN.NAME')"
                always-visible
              />
              <VcColumn
                id="email"
                field="email"
                :width="320"
                :title="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.COLUMN.EMAIL')"
              />
              <VcColumn
                id="login"
                field="login"
                :width="200"
                :title="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.COLUMN.LOGIN')"
              />
            </VcDataTable>
          </div>
        </div>
      </template>
      <template #footer="{ close }">
        <VcButton
          variant="secondary"
          @click="close"
          >{{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.CLOSE") }}</VcButton
        >
      </template>
    </VcPopup>

    <VcPopup
      v-model="showQuery"
      modal-width="tw-max-w-[600px]"
      :title="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.QUERY_TITLE')"
    >
      <template #content>
        <div class="tw-w-full">
          <p class="tw-mb-3 tw-text-sm tw-text-[color:var(--neutrals-600)]">
            {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.QUERY_LEAD") }}
          </p>
          <!-- A query is mostly ids with no spaces, so it breaks anywhere rather than scrolling sideways. -->
          <pre
            v-if="generatedQuery"
            class="tw-p-3 tw-rounded tw-border tw-border-[color:var(--neutrals-200)] tw-bg-[color:var(--neutrals-50)] tw-text-sm tw-font-mono tw-whitespace-pre-wrap tw-break-all"
            >{{ generatedQuery }}</pre>
          <p
            v-else
            class="tw-text-sm tw-text-[color:var(--neutrals-500)]"
          >
            {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.QUERY_NONE") }}
          </p>
          <p
            v-if="pickedCount"
            class="tw-mt-3 tw-text-sm tw-text-[color:var(--neutrals-600)]"
          >
            {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.QUERY_PLUS", { count: pickedCount }) }}
          </p>
        </div>
      </template>
      <template #footer="{ close }">
        <div class="tw-flex tw-gap-2">
          <VcButton
            v-if="generatedQuery && clipboardSupported"
            variant="primary"
            :icon="copied ? 'lucide-check' : 'lucide-copy'"
            @click="copy(generatedQuery)"
          >
            {{ $t(`PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.${copied ? "COPIED" : "COPY"}`) }}
          </VcButton>
          <VcButton
            variant="secondary"
            @click="close"
            >{{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.CLOSE") }}</VcButton
          >
        </div>
      </template>
    </VcPopup>
  </div>
</template>

<script lang="ts" setup>
import { computed, ref } from "vue";
import { useClipboard } from "@vueuse/core";
import { useDataTablePagination } from "@vc-shell/framework";
import { VcButton, VcColumn, VcDataTable, VcHint, VcIcon, VcLoading, VcPopup, VcStatus } from "@vc-shell/framework/ui";
import { useAudiencePreview } from "../composables/useAudiencePreview";
import { addsUp, audienceStatus, AudienceStatus, flowSteps, noteParts, SUMMARY_PREFIX as P } from "../utils/audienceSummary";
import type { AudienceDraft, AudienceEstimate } from "../utils/audienceSync";
import type { MemberLoader } from "./SpecificRecipients.vue";

const props = defineProps<{
  estimate: AudienceEstimate;
  sourceLine: string;
  /** The audience as it stands, for the preview table. */
  audience: AudienceDraft;
  generatedQuery: string;
  pickedCount: number;
  loadMembers: MemberLoader;
  readonly?: boolean;
  sentCount?: number;
}>();

const STATUS_VARIANT: Record<AudienceStatus, "success" | "warning" | "danger"> = {
  READY: "success",
  EMPTY: "warning",
  INVALID: "danger",
};

const { fetchPage } = useAudiencePreview();

const total = computed(() => props.estimate.result?.totalCount ?? 0);
const status = computed(() => audienceStatus(props.estimate));
const steps = computed(() => (props.estimate.result ? flowSteps(props.estimate.result) : []));
const notes = computed(() => (props.estimate.result ? noteParts(props.estimate.result) : []));

/**
 * Whether the audience is defined at all. An audience that matches nobody is still worth looking
 * at — seeing the query and the empty preview is how the author finds out why.
 */
const hasAudience = computed(() => props.generatedQuery.trim().length > 0 || props.pickedCount > 0);

const showPreview = ref(false);
const showQuery = ref(false);
// legacy: fall back to execCommand where the page is not a secure context, as on plain-http stands.
const { copy, copied, isSupported: clipboardSupported } = useClipboard({ legacy: true });
const loadingPage = ref(false);
const previewFailed = ref(false);

interface PreviewRow {
  name: string;
  email: string;
  login: string;
}

const previewRows = ref<PreviewRow[]>([]);
const previewTotal = ref(0);
const previewPagination = useDataTablePagination({
  totalCount: previewTotal,
  onPageChange: ({ skip }) => loadPreviewPage(skip),
});

function openPreview() {
  showPreview.value = true;
  previewRows.value = [];
  previewPagination.reset();
  loadPreviewPage(0);
}

async function loadPreviewPage(skip: number) {
  loadingPage.value = true;
  previewFailed.value = false;
  try {
    const page = await fetchPage(props.audience, skip, previewPagination.pageSize);
    const recipients = page.results ?? [];

    previewTotal.value = page.totalCount ?? 0;

    // The recipient row carries the name and login; the email lives on the member.
    const ids = [...new Set(recipients.map((r) => r.memberId).filter(Boolean))] as string[];
    const members = ids.length ? ((await props.loadMembers(undefined, 0, ids)).results ?? []) : [];
    const emailByMember = new Map(members.map((m) => [m.id, m.emails?.[0] ?? ""]));

    previewRows.value = recipients.map((r) => ({
      name: r.memberName ?? "",
      email: emailByMember.get(r.memberId ?? "") ?? "",
      login: r.userName ?? "",
    }));
  } catch {
    previewRows.value = [];
    previewFailed.value = true;
  } finally {
    loadingPage.value = false;
  }
}
</script>
