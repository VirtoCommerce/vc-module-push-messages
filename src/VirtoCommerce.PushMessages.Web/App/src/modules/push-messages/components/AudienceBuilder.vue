<template>
  <div class="tw-space-y-4">
    <AudienceModePicker
      :model-value="mode"
      :disabled="disabled"
      @update:model-value="setMode"
    />

    <VcCard
      v-if="mode === 'conditions' || mode === 'query'"
      :header="$t(`${A_PREFIX}.CONDITIONS_HEADER`)"
    >
      <div class="tw-p-4">
        <!-- Match by conditions -->
        <QueryBuilder
          v-if="mode === 'conditions'"
          v-model:query="conditionQuery"
          :fields="queryFields"
          :starters="starters"
          show-edit-as-query
          :disabled="disabled"
          @update:invalid="conditionsInvalid = $event"
          @edit-as-query="setMode('query')"
          @starter="onStarter"
        />

        <!-- Advanced query -->
        <div
          v-if="mode === 'query'"
          class="tw-space-y-2"
        >
          <VcTextarea
            v-model="rawQuery"
            :disabled="disabled"
            :label="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.QUERY_FIELD.LABEL')"
            maxlength="1024"
          />
          <VcHint v-if="rawQuery && !estimateFailed">
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
      </div>
    </VcCard>

    <VcCard
      v-if="mode !== 'everyone'"
      :header="$t(`${A_PREFIX}.RECIPIENTS_PICKER.LABEL`)"
    >
      <div class="tw-p-4">
        <SpecificRecipients
          v-model="picked"
          :counts="memberCounts"
          :load-members="loadMembers"
          :disabled="disabled"
        />
      </div>
    </VcCard>

    <VcHint
      v-if="queryTooLong"
      error
    >
      {{ $t(`${A_PREFIX}.VALIDATION.QUERY_TOO_LONG`) }}
    </VcHint>

    <AudienceBreakdown
      :estimate="estimate"
      :source-line="sourceLine"
      :audience="{ memberQuery: emittedQuery, memberIds: emittedIds }"
      :generated-query="generatedQuery"
      :picked-count="picked.length"
      :load-members="loadMembers"
      :readonly="disabled"
      :sent-count="sentCount"
    />
  </div>
</template>

<script lang="ts" setup>
import { computed, nextTick, ref, watch } from "vue";
import { useDebounceFn } from "@vueuse/core";
import { useI18n } from "vue-i18n";
import { RoleSearchCriteria, RoleSearchResult, SecurityClient, useApiClient } from "@vc-shell/framework";
import { VcButton, VcCard, VcHint, VcTextarea } from "@vc-shell/framework/ui";

import { CustomerModuleClient, MemberSearchResult, MembersSearchCriteria } from "../../../api_client/virtocommerce.customer";
import { QueryBuilder, parsePhrase } from "../../../components/queryBuilder";
import type { ConditionRow, QueryField, QueryStarter } from "../../../components/queryBuilder";
import { useAudiencePreview } from "../composables/useAudiencePreview";
import { AUDIENCE_FIELDS, AudienceField } from "../utils/audienceFields";
import { AudienceMode, audiencePhrase, detectAudienceMode, MAX_QUERY_LENGTH } from "../utils/audienceQuery";
import { conditionCount, formatSourceLine, sourceParts } from "../utils/audienceSummary";
import type { AudienceEstimate } from "../utils/audienceSync";
import AudienceBreakdown from "./AudienceBreakdown.vue";
import AudienceModePicker from "./AudienceModePicker.vue";
import SpecificRecipients from "./SpecificRecipients.vue";

const props = defineProps<{
  memberQuery?: string;
  memberIds?: string[];
  disabled?: boolean;
  /** A sent message: how many it actually went to. */
  sentCount?: number;
}>();

const emit = defineEmits<{
  "update:memberQuery": [value: string | undefined];
  "update:memberIds": [value: string[] | undefined];
  "update:invalid": [value: boolean];
  "update:estimate": [value: AudienceEstimate];
}>();

const { t } = useI18n({ useScope: "global" });
const { getApiClient: getCustomerApiClient } = useApiClient(CustomerModuleClient);
const { getApiClient: getSecurityApiClient } = useApiClient(SecurityClient);
const { preview, failed: estimateFailed, refresh, countFor, loading: loadingPreview } = useAudiencePreview();

/** Recipient count per picked member, so a chip can say what a company actually brings in. */
const memberCounts = ref<Record<string, number>>({});

const A_PREFIX = "PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE";

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

/** What we last told the parent, so an echo of our own emit is not mistaken for an edit. */
const emittedQuery = ref<string>();
const emittedIds = ref<string[]>();
/** True while a stored audience is being read in, so that read is not mistaken for an edit. */
let applying = false;

/** The audience the builder hands over, whichever mode produced it. */
const generatedQuery = computed(() => audiencePhrase(mode.value, conditionQuery.value, rawQuery.value));

const estimate = computed<AudienceEstimate>(() => ({
  result: preview.value,
  failed: estimateFailed.value,
  loading: loadingPreview.value,
}));

/** Where the recipients come from: "1 condition · 6 companies". */
const sourceLine = computed(() =>
  formatSourceLine(sourceParts(mode.value, conditionCount(generatedQuery.value), preview.value), (key, n) => (n === undefined ? t(key) : t(key, n))),
);

watch(estimate, (value) => emit("update:estimate", value), { immediate: true });

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

  // The roles endpoint does not sort; the list reads alphabetically like the companies beside it.
  const roles = (result.results ?? []).sort((a, b) => (a.name ?? "").localeCompare(b.name ?? ""));
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
  emittedQuery.value = memberQuery || undefined;
  emittedIds.value = memberIds?.length ? [...memberIds] : undefined;
  refreshPreview(emittedQuery.value, emittedIds.value);

  nextTick(() => {
    applying = false;
  });
}

watch(
  picked,
  async (ids) => {
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

/** Counts the audience as it stands now, without waiting out the debounce. */
async function flush() {
  await refresh({ memberQuery: emittedQuery.value, memberIds: emittedIds.value });
}

defineExpose({ flush });

const refreshPreview = useDebounceFn((memberQuery?: string, memberIds?: string[]) => {
  refresh({ memberQuery, memberIds });
}, 400);

watch(
  () => [props.memberQuery, props.memberIds] as const,
  ([incomingQuery, incomingIds]) => {
    const sameQuery = (incomingQuery ?? "") === (emittedQuery.value ?? "");
    const sameIds = JSON.stringify(incomingIds ?? []) === JSON.stringify(emittedIds.value ?? []);

    if (sameQuery && sameIds) {
      return;
    }

    applyIncoming(incomingQuery, incomingIds);
  },
  { immediate: true },
);

const invalid = computed(() => (mode.value === "conditions" && conditionsInvalid.value) || queryTooLong.value || estimateFailed.value);

// The estimate settles after the audience does, so its verdict is reported on its own.
watch(estimateFailed, () => emit("update:invalid", invalid.value));

watch(
  [mode, picked, rawQuery, conditionQuery, conditionsInvalid],
  () => {
    if (applying) {
      return;
    }

    const phrase = generatedQuery.value;
    // Everyone hides the picker, so anything left in it is not part of that audience.
    const ids = mode.value === "everyone" ? [] : [...picked.value];

    emittedQuery.value = phrase || undefined;
    emittedIds.value = ids.length ? ids : undefined;

    emit("update:memberQuery", emittedQuery.value);
    emit("update:memberIds", emittedIds.value);
    emit("update:invalid", invalid.value);

    refreshPreview(emittedQuery.value, emittedIds.value);
  },
  { deep: true },
);
</script>
