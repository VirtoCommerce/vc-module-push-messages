<template>
  <div class="tw-space-y-4">
    <VcLabel required>{{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.LABEL") }}</VcLabel>

    <div
      class="tw-rounded tw-border tw-border-[color:var(--neutrals-200)] tw-divide-y tw-divide-[color:var(--neutrals-200)] tw-overflow-hidden"
      :class="{ 'tw-opacity-60 tw-pointer-events-none': disabled }"
    >
      <label
        v-for="option in MODES"
        :key="option.mode"
        class="tw-flex tw-items-start tw-gap-3 tw-p-4 tw-cursor-pointer tw-transition-colors"
        :class="mode === option.mode
          ? 'tw-bg-[color:var(--primary-50)] tw-ring-1 tw-ring-inset tw-ring-[color:var(--primary-500)]'
          : 'hover:tw-bg-[color:var(--neutrals-50)]'"
      >
        <VcRadioButton
          :model-value="mode"
          :value="option.mode"
          :disabled="disabled"
          @update:model-value="setMode(option.mode)"
        />
        <span class="tw-min-w-0">
          <span
            class="tw-flex tw-items-center tw-gap-2 tw-font-medium"
            :class="mode === option.mode
              ? 'tw-text-[color:var(--primary-700)]'
              : 'tw-text-[color:var(--neutrals-800)]'"
          >
            <VcIcon :icon="option.icon" size="m" />
            {{ $t(`${MODE_PREFIX}.${option.key}.TITLE`) }}
          </span>
          <span class="tw-block tw-mt-1 tw-text-sm tw-text-[color:var(--neutrals-500)]">
            {{ $t(`${MODE_PREFIX}.${option.key}.HINT`) }}
          </span>
        </span>
      </label>
    </div>

    <!-- Match by conditions -->
    <QueryBuilder
      v-if="mode === 'conditions'"
      v-model:query="conditionQuery"
      :fields="queryFields"
      :starters="starters"
      show-edit-as-query
      :disabled="disabled"
      @update:invalid="conditionsInvalid = $event"
      @update:description="conditionParts = $event"
      @edit-as-query="setMode('query')"
      @starter="onStarter"
    />

    <!-- Advanced query -->
    <div v-if="mode === 'query'" class="tw-space-y-2">
      <VcTextarea
        v-model="rawQuery"
        :disabled="disabled"
        :label="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.QUERY_FIELD.LABEL')"
        maxlength="1024"
      />
      <VcHint v-if="rawQuery">
        {{
          $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.QUERY_FIELD.VALID", {
            count: preview?.totalCount ?? 0,
          })
        }}
      </VcHint>
      <VcButton
        v-if="canReturnToConditions"
        variant="link"
        size="sm"
        icon="lucide-arrow-left"
        :disabled="disabled"
        @click="setMode('conditions')"
      >
        {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.BACK_TO_CONDITIONS") }}
      </VcButton>
    </div>

    <!-- One picker for every mode but Everyone; only its label changes -->
    <VcSelect
      v-if="mode !== 'everyone'"
      v-model="picked"
      emit-value
      searchable
      multiple
      option-value="id"
      option-label="name"
      :options="loadMembers"
      :disabled="disabled"
      :label="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.RECIPIENTS_PICKER.LABEL')"
      :placeholder="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.RECIPIENTS_PICKER.PLACEHOLDER')"
      :hint="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.RECIPIENTS_PICKER.HINT')"
    >
      <template #selected-item="{ opt, index, removeAtIndex }">
        <span
          class="tw-inline-flex tw-items-center tw-gap-2 tw-mr-2 tw-mb-1 tw-pl-2 tw-pr-1 tw-py-1 tw-rounded tw-border tw-border-[color:var(--primary-300)] tw-bg-[color:var(--primary-50)]"
        >
          <VcStatus :variant="isCompany(opt) ? 'primary' : 'info'">
            {{ isCompany(opt)
              ? $t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.RECIPIENTS_PICKER.COMPANY')
              : $t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.RECIPIENTS_PICKER.PERSON') }}
          </VcStatus>
          <span class="tw-text-sm tw-text-[color:var(--neutrals-800)]">{{ opt.name }}</span>
          <span v-if="countOf(opt) !== undefined" class="tw-text-sm tw-text-[color:var(--neutrals-500)]">
            · {{ $t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.RECIPIENTS_PICKER.COUNT', countOf(opt) as number) }}
          </span>
          <VcButton
            icon="lucide-x"
            variant="ghost"
            size="icon-sm"
            :disabled="disabled"
            @click="removeAtIndex(index)"
          />
        </span>
      </template>
    </VcSelect>

    <VcHint v-if="queryTooLong" class="tw-text-[color:var(--danger-500)]">
      {{ $t(`${A_PREFIX}.VALIDATION.QUERY_TOO_LONG`) }}
    </VcHint>

    <!-- Estimate -->
    <div class="tw-border tw-border-[color:var(--primary-300)] tw-rounded tw-p-4 tw-space-y-3">
      <div class="tw-flex tw-items-baseline tw-gap-2">
        <span class="tw-text-3xl tw-font-semibold">{{ preview?.totalCount ?? 0 }}</span>
        <span>{{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.RECIPIENTS", preview?.totalCount ?? 0) }}</span>
        <VcLoading v-if="loadingPreview" active class="tw-ml-2" />
      </div>

      <dl class="tw-font-mono tw-text-sm tw-space-y-1">
        <div v-for="line in estimateLines" :key="line.label" class="tw-flex tw-justify-between">
          <dt>{{ line.label }}</dt>
          <dd>{{ line.value }}</dd>
        </div>
      </dl>

      <p class="tw-text-sm tw-text-[color:var(--neutrals-700)]">
        <template v-for="(part, i) in summaryParts" :key="i"><span
          :class="{
            'tw-font-semibold tw-text-[color:var(--neutrals-900)]': part.strong,
            'tw-italic': part.em,
          }"
        >{{ part.text }}</span></template>
      </p>

      <div v-if="hasAudience" class="tw-flex tw-gap-2">
        <VcButton variant="outline" size="sm" icon="lucide-eye" @click="openPreview">
          {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.PREVIEW") }}
        </VcButton>
        <VcButton variant="outline" size="sm" icon="lucide-code" @click="showQuery = true">
          {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.SHOW_QUERY") }}
        </VcButton>
      </div>
    </div>

    <VcPopup
      v-model="showPreview"
      modal-width="tw-max-w-[600px]"
      :title="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.PREVIEW_TITLE')"
    >
      <template #content>
        <div class="tw-w-full">
        <p class="tw-mb-3 tw-text-sm tw-text-[color:var(--neutrals-600)]">
          {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.PREVIEW_LEAD", preview?.totalCount ?? 0) }}
        </p>
        <VcLoading v-if="loadingPage" active />
        <p v-else-if="previewFailed" class="tw-text-sm tw-text-[color:var(--danger-500)]">
          {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.PREVIEW_FAILED") }}
        </p>
        <p v-else-if="!previewRows.length" class="tw-text-sm tw-text-[color:var(--neutrals-500)]">
          {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.PREVIEW_EMPTY") }}
        </p>
        <div v-else class="tw-max-h-[24rem] tw-overflow-auto">
          <VcDataTable :items="previewRows" :total-count="previewRows.length">
            <VcColumn id="name" field="name" :title="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.COLUMN.NAME')" always-visible />
            <VcColumn id="email" field="email" :title="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.COLUMN.EMAIL')" />
            <VcColumn id="login" field="login" :title="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.COLUMN.LOGIN')" />
          </VcDataTable>
        </div>
        </div>
      </template>
      <template #footer="{ close }">
        <VcButton variant="secondary" @click="close">{{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.CLOSE") }}</VcButton>
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
        <pre
          v-if="generatedQuery"
          class="tw-p-3 tw-rounded tw-border tw-border-[color:var(--neutrals-200)] tw-bg-[color:var(--neutrals-50)] tw-text-sm tw-font-mono tw-overflow-x-auto"
        >{{ generatedQuery }}</pre>
        <p v-else class="tw-text-sm tw-text-[color:var(--neutrals-500)]">
          {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.QUERY_NONE") }}
        </p>
        <p
          v-if="picked.length"
          class="tw-mt-3 tw-text-sm tw-text-[color:var(--neutrals-600)]"
        >
          {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.QUERY_PLUS", { count: picked.length }) }}
        </p>
        </div>
      </template>
      <template #footer="{ close }">
        <VcButton variant="secondary" @click="close">{{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.CLOSE") }}</VcButton>
      </template>
    </VcPopup>
  </div>
</template>

<script lang="ts" setup>
import { computed, nextTick, ref, watch } from "vue";
import { useDebounceFn } from "@vueuse/core";
import { useI18n } from "vue-i18n";
import { RoleSearchCriteria, RoleSearchResult, SecurityClient, useApiClient } from "@vc-shell/framework";
import { VcButton, VcHint, VcIcon, VcLabel, VcLoading, VcRadioButton, VcColumn, VcDataTable, VcPopup, VcSelect, VcStatus, VcTextarea } from "@vc-shell/framework/ui";

// Member is referenced by the @vue-generic annotations on the pickers.
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { CustomerModuleClient, Member, MemberSearchResult, MembersSearchCriteria } from "../../../api_client/virtocommerce.customer";
import { QueryBuilder, parsePhrase } from "../../../components/queryBuilder";
import type { ConditionRow, DescriptionPart, QueryField, QueryStarter } from "../../../components/queryBuilder";
import { useAudiencePreview } from "../composables/useAudiencePreview";
import { AUDIENCE_FIELDS, AudienceField } from "../utils/audienceFields";
import { AudienceMode, audiencePhrase, detectAudienceMode, MAX_QUERY_LENGTH } from "../utils/audienceQuery";

const props = defineProps<{
  memberQuery?: string;
  memberIds?: string[];
  disabled?: boolean;
}>();

const emit = defineEmits<{
  "update:memberQuery": [value: string | undefined];
  "update:memberIds": [value: string[] | undefined];
  "update:invalid": [value: boolean];
}>();

const { t } = useI18n({ useScope: "global" });
const { getApiClient: getCustomerApiClient } = useApiClient(CustomerModuleClient);
const { getApiClient: getSecurityApiClient } = useApiClient(SecurityClient);
const { preview, refresh, countFor, fetchPage, loading: loadingPreview } = useAudiencePreview();

/** Recipient count per picked member, so a chip can say what a company actually brings in. */
const memberCounts = ref<Record<string, number>>({});

const showPreview = ref(false);
const showQuery = ref(false);
const loadingPage = ref(false);
const previewFailed = ref(false);
interface PreviewRow {
  name: string;
  email: string;
  login: string;
}

const previewRows = ref<PreviewRow[]>([]);
/** Names of the picked members, so the summary can spell them out. */
const pickedMembers = ref<Member[]>([]);

const A_PREFIX = "PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE";
const MODE_PREFIX = `${A_PREFIX}.MODES`;
const MODES: { mode: AudienceMode; key: string; icon: string }[] = [
  { mode: "everyone", key: "EVERYONE", icon: "lucide-globe" },
  { mode: "list", key: "LIST", icon: "lucide-users" },
  { mode: "conditions", key: "CONDITIONS", icon: "lucide-filter" },
  { mode: "query", key: "QUERY", icon: "lucide-code" },
];

/**
 * One-click starting points, from the design. Each contributes a single condition with the value
 * left empty for the author to fill; the one without a condition switches the whole mode instead.
 */
const STARTERS: { key: string; row?: ConditionRow }[] = [
  { key: "ALL_CUSTOMERS" },
  { key: "ONE_COMPANY", row: { field: "parentorganizations", operator: "is", value: "" } },
  { key: "BY_ROLE", row: { field: "roleid", operator: "is", value: "" } },
  { key: "EMAIL_DOMAIN", row: { field: "emails", operator: "endsWith", value: "" } },
  { key: "REGISTERED_SINCE", row: { field: "createddate", operator: "onOrAfter", value: "" } },
  { key: "TAGGED", row: { field: "groups", operator: "is", value: "" } },
  { key: "CUSTOM", row: { field: "name", operator: "is", value: "" } },
];

const mode = ref<AudienceMode>("conditions");
const picked = ref<string[]>([]);
/** The phrase the raw editor holds, and the one the condition builder reads and writes. */
const rawQuery = ref("");
const conditionQuery = ref("");
const conditionsInvalid = ref(false);
const conditionParts = ref<DescriptionPart[]>([]);

/** What we last told the parent, so an echo of our own emit is not mistaken for an edit. */
let emittedQuery: string | undefined;
let emittedIds: string[] | undefined;
/** True while a stored audience is being read in, so that read is not mistaken for an edit. */
let applying = false;

/** The audience the builder hands over, whichever mode produced it. */
const generatedQuery = computed(() => audiencePhrase(mode.value, conditionQuery.value, rawQuery.value));

/**
 * Whether the audience is defined at all. An audience that matches nobody is still worth looking
 * at — seeing the query and the empty preview is how the author finds out why.
 */
const hasAudience = computed(() => generatedQuery.value.trim().length > 0 || picked.value.length > 0);

/** The stored phrase has a length limit, and nothing in the conditions themselves shows it. */
const queryTooLong = computed(() => generatedQuery.value.length > MAX_QUERY_LENGTH);

const canReturnToConditions = computed(() => parsePhrase(rawQuery.value) !== null);

/** The member fields, in the shape the shared builder takes: labels read, ids go into the phrase. */
const queryFields = computed<QueryField[]>(() =>
  AUDIENCE_FIELDS.map((field) => ({
    id: field.id,
    label: t(field.labelKey),
    type: field.type,
    options: field.options,
    load: loaderFor(field),
  })),
);

/** Where a reference field's choices come from; a field of any other kind has none. */
function loaderFor(field: AudienceField): QueryField["load"] {
  if (field.source === "roles") {
    return loadRoles;
  }

  return field.source === "organizations" ? loadOrganizations : undefined;
}

const starters = computed<QueryStarter[]>(() =>
  STARTERS.map((starter) => ({
    key: starter.key,
    label: t(`${A_PREFIX}.STARTERS.${starter.key}`),
    row: starter.row,
  })),
);

const estimateLines = computed(() => {
  const value = preview.value;

  if (!value) {
    return [];
  }

  const prefix = `${A_PREFIX}.ESTIMATE`;
  const lines: { label: string; value: string }[] = [
    { label: t(`${prefix}.MEMBERS_MATCHED`), value: `${value.membersMatched ?? 0}` },
  ];

  if (value.companiesExpanded) {
    lines.push({
      label: t(`${prefix}.COMPANIES_EXPANDED`, value.companiesExpanded),
      value: `+${value.peopleFromCompanies ?? 0}`,
    });
  }

  lines.push({ label: t(`${prefix}.PEOPLE_IN_SCOPE`), value: `${value.peopleInScope ?? 0}` });

  if (value.extraLogins) {
    lines.push({ label: t(`${prefix}.EXTRA_LOGINS`), value: `+${value.extraLogins}` });
  }

  return lines;
});

function isCompany(opt: Member): boolean {
  return opt.memberType === "Organization";
}

function countOf(opt: Member): number | undefined {
  return opt.id ? memberCounts.value[opt.id] : undefined;
}

/** A starter with no condition of its own; the only one is "everyone". */
function onStarter() {
  setMode("everyone");
}

function setMode(next: AudienceMode) {
  const previous = mode.value;

  if (next === "query") {
    rawQuery.value = generatedQuery.value;
  }

  // Only a phrase the author just edited by hand may replace the conditions. Coming back from any
  // other mode, rawQuery is stale and the conditions are what the author last worked on.
  if (next === "conditions" && previous === "query") {
    conditionQuery.value = rawQuery.value;
  }

  mode.value = next;
}

async function loadMembers(keyword?: string, skip?: number, ids?: string[]): Promise<MemberSearchResult> {
  const apiClient = await getCustomerApiClient();

  return apiClient.searchMember({
    keyword,
    objectIds: ids,
    deepSearch: true,
    objectType: "Member",
    sort: "MemberType:desc;Name",
    skip: skip || 0,
    take: ids?.length ?? 20,
  } as MembersSearchCriteria);
}

async function loadOrganizations(keyword?: string, skip?: number, ids?: string[]): Promise<MemberSearchResult> {
  const apiClient = await getCustomerApiClient();

  return apiClient.searchMember({
    keyword,
    objectIds: ids,
    deepSearch: true,
    objectType: "Member",
    memberType: "Organization",
    sort: "Name",
    skip: skip || 0,
    take: ids?.length ?? 20,
  } as MembersSearchCriteria);
}

/**
 * The roles endpoint ignores objectIds — asking for one id answers with the first role instead —
 * so a role is found by reading the list and picking from it here. An installation has tens of
 * roles, not thousands.
 */
async function loadRoles(keyword?: string, skip?: number, ids?: string[]): Promise<RoleSearchResult> {
  const apiClient = await getSecurityApiClient();

  const result = await apiClient.searchRoles({
    keyword: ids?.length ? undefined : keyword,
    skip: 0,
    take: 200,
  } as RoleSearchCriteria);

  const roles = result.results ?? [];
  const matched = ids?.length ? roles.filter((role) => role.id && ids.includes(role.id)) : roles;
  const from = skip || 0;

  return { results: matched.slice(from, from + 20), totalCount: matched.length };
}

function applyIncoming(memberQuery?: string, memberIds?: string[]) {
  // Reading a stored audience is not an edit. Without this the rebuilt phrase would be written
  // straight back over the stored one — normalising it, marking the blade dirty before the
  // author has touched anything, and erasing audiences whose phrase rebuilds to nothing.
  applying = true;

  picked.value = [...(memberIds ?? [])];
  rawQuery.value = memberQuery ?? "";
  mode.value = detectAudienceMode(memberQuery, memberIds);
  conditionQuery.value = mode.value === "conditions" ? (memberQuery ?? "") : "";

  // The state watcher stands down while a stored audience is read in, so the estimate has to be
  // asked for here. Without this a saved message reads "0 recipients" until something is touched,
  // and the preview popup opens on an audience nobody defined.
  emittedQuery = memberQuery || undefined;
  emittedIds = memberIds?.length ? [...memberIds] : undefined;
  refreshPreview(emittedQuery, emittedIds);

  nextTick(() => {
    applying = false;
  });
}

watch(
  picked,
  async (ids) => {
    try {
      pickedMembers.value = ids.length ? (await loadMembers(undefined, 0, ids)).results ?? [] : [];
    } catch {
      pickedMembers.value = [];
    }

    for (const id of ids) {
      if (memberCounts.value[id] === undefined) {
        try {
          memberCounts.value[id] = await countFor(id);
        } catch {
          // A chip without a count still names the company; the estimate is the number that counts.
        }
      }
    }
  },
  { immediate: true },
);

/**
 * Plain-language restatement of the audience, from the design. The conditions are described by the
 * builder that owns them; what the audience adds is everything around them.
 */
const summaryParts = computed<DescriptionPart[]>(() => {
  const prefix = `${A_PREFIX}.ESTIMATE.SUMMARY`;

  if (mode.value === "everyone") {
    return [{ text: t(`${prefix}.EVERYONE`) }];
  }

  if (mode.value === "list") {
    return [{ text: t(`${prefix}.LIST`, picked.value.length) }];
  }

  if (mode.value === "query") {
    return [{ text: rawQuery.value.trim() ? t(`${prefix}.QUERY`) : t(`${prefix}.QUERY_EMPTY`) }];
  }

  const described = conditionParts.value;

  if (!described.length && !picked.value.length) {
    return [{ text: t(`${prefix}.NO_CONDITIONS`) }];
  }

  const parts: DescriptionPart[] = [];

  if (described.length) {
    parts.push({ text: t(`${prefix}.CONDITIONS_PREFIX`) + " " });
    parts.push(...described);
  }

  pickedMembers.value.forEach((member, index) => {
    const opening = described.length ? t(`${prefix}.PLUS_PREFIX`) : t(`${prefix}.ONLY_PICKED_PREFIX`);

    parts.push({ text: index === 0 ? opening + " " : ", " });
    parts.push({ text: member.name ?? "", strong: true });

    if (isCompany(member)) {
      parts.push({ text: " " + t(`${prefix}.WHOLE_COMPANY_SUFFIX`) });
    }
  });

  parts.push({ text: "." });

  return parts;
});

async function openPreview() {
  showPreview.value = true;
  loadingPage.value = true;
  previewFailed.value = false;
  try {
    const page = await fetchPage({ memberQuery: emittedQuery, memberIds: emittedIds });
    const recipients = page.results ?? [];

    // The recipient row carries the name and login; the email lives on the member.
    const ids = [...new Set(recipients.map((r) => r.memberId).filter(Boolean))] as string[];
    const members = ids.length ? (await loadMembers(undefined, 0, ids)).results ?? [] : [];
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

const refreshPreview = useDebounceFn((memberQuery?: string, memberIds?: string[]) => {
  refresh({ memberQuery, memberIds });
}, 400);

watch(
  () => [props.memberQuery, props.memberIds] as const,
  ([incomingQuery, incomingIds]) => {
    const sameQuery = (incomingQuery ?? "") === (emittedQuery ?? "");
    const sameIds = JSON.stringify(incomingIds ?? []) === JSON.stringify(emittedIds ?? []);

    if (sameQuery && sameIds) {
      return;
    }

    applyIncoming(incomingQuery, incomingIds);
  },
  { immediate: true },
);

watch(
  [mode, picked, rawQuery, conditionQuery, conditionsInvalid],
  () => {
    if (applying) {
      return;
    }

    const phrase = generatedQuery.value;
    // Everyone hides the picker, so anything left in it is not part of that audience.
    const ids = mode.value === "everyone" ? [] : [...picked.value];

    emittedQuery = phrase || undefined;
    emittedIds = ids.length ? ids : undefined;

    emit("update:memberQuery", emittedQuery);
    emit("update:memberIds", emittedIds);
    emit("update:invalid", (mode.value === "conditions" && conditionsInvalid.value) || queryTooLong.value);

    refreshPreview(emittedQuery, emittedIds);
  },
  { deep: true },
);
</script>
