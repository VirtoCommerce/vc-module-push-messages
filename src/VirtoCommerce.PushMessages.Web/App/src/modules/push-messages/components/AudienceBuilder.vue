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
    <div v-if="mode === 'conditions'" class="tw-space-y-3">
      <div class="tw-flex tw-items-center tw-gap-2">
        <span>{{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.MATCH") }}</span>
        <VcButtonGroup attached size="sm">
          <VcButton
            v-for="candidate in JOINS"
            :key="candidate"
            :variant="join === candidate ? 'primary' : 'outline'"
            :disabled="disabled"
            @click="join = candidate"
          >
            {{ $t(`PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.${candidate === "all" ? "ALL" : "ANY"}`) }}
          </VcButton>
        </VcButtonGroup>
        <span>{{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.OF_THESE_CONDITIONS") }}</span>
      </div>

      <div v-for="(row, index) in rows" :key="index" class="tw-space-y-1">
        <div
          v-if="index > 0"
          class="tw-text-xs tw-font-semibold tw-tracking-wider tw-text-[color:var(--neutrals-400)]"
        >
          {{ $t(`PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.JOINER.${join === "any" ? "OR" : "AND"}`) }}
        </div>
        <div class="tw-flex tw-items-center tw-gap-2">
          <VcSelect
            v-model="row.field"
            emit-value
            :clearable="false"
            option-value="id"
            option-label="label"
            class="tw-w-1/3"
            :options="fieldOptions"
            :disabled="disabled"
            @update:model-value="onFieldChange(row)"
          />
          <VcSelect
            v-model="row.operator"
            emit-value
            :clearable="false"
            option-value="id"
            option-label="label"
            class="tw-w-1/4"
            :options="operatorOptions(row)"
            :disabled="disabled"
          />
          <component
            :is="valueControl(row).is"
            v-bind="valueControl(row).props"
            class="tw-flex-1"
            :model-value="controlValue(row)"
            :disabled="disabled"
            @update:model-value="(value: unknown) => (row.value = toRowValue(value))"
          />
          <VcButton
            icon="lucide-x"
            variant="ghost"
            size="icon-sm"
            :disabled="disabled"
            @click="removeRow(index)"
          />
        </div>
        <VcHint v-if="rowError(row)" class="tw-text-[color:var(--danger-500)]">
          {{ $t(rowError(row) as string) }}
        </VcHint>
      </div>

      <div v-if="contradiction" class="tw-space-y-2">
        <VcHint class="tw-text-[color:var(--warning-600)]">
          {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.CONTRADICTION") }}
        </VcHint>
        <VcButton variant="outline" size="sm" icon="lucide-merge" :disabled="disabled" @click="combineDuplicates">
          {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.COMBINE") }}
        </VcButton>
      </div>

      <div class="tw-flex tw-gap-4">
        <VcButton variant="outline" size="sm" icon="lucide-plus" :disabled="disabled" @click="addRow">
          {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ADD_CONDITION") }}
        </VcButton>
        <VcButton variant="link" size="sm" icon="lucide-arrow-right" :disabled="disabled" @click="setMode('query')">
          {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.EDIT_AS_QUERY") }}
        </VcButton>
      </div>

      <div class="tw-space-y-2">
        <p class="tw-text-xs tw-uppercase tw-tracking-wider tw-text-[color:var(--neutrals-400)]">
          {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.STARTERS.LABEL") }}
        </p>
        <div class="tw-flex tw-flex-wrap tw-gap-2">
          <VcButton
            v-for="starter in STARTERS"
            :key="starter.key"
            variant="outline"
            size="xs"
            :disabled="disabled"
            @click="applyStarter(starter)"
          >
            {{ $t(`PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.STARTERS.${starter.key}`) }}
          </VcButton>
        </div>
      </div>
    </div>

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
      :label="mode === 'list'
        ? $t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.RECIPIENTS_PICKER.LABEL')
        : $t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.RECIPIENTS_PICKER.ALSO_INCLUDE')"
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
            · {{ $t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.RECIPIENTS_PICKER.COUNT', { count: countOf(opt) }) }}
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

    <VcHint
      v-for="problem in problems"
      :key="problem"
      class="tw-text-[color:var(--danger-500)]"
    >
      {{ $t(problem) }}
    </VcHint>

    <!-- Estimate -->
    <div class="tw-border tw-border-[color:var(--primary-300)] tw-rounded tw-p-4 tw-space-y-3">
      <div class="tw-flex tw-items-baseline tw-gap-2">
        <span class="tw-text-3xl tw-font-semibold">{{ preview?.totalCount ?? 0 }}</span>
        <span>{{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.RECIPIENTS") }}</span>
        <VcLoading v-if="loadingPreview" active class="tw-ml-2" />
      </div>

      <dl class="tw-font-mono tw-text-sm tw-space-y-1">
        <div v-for="line in estimateLines" :key="line.labelKey" class="tw-flex tw-justify-between">
          <dt>{{ $t(line.labelKey, line.labelArgs) }}</dt>
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

      <div v-if="preview?.totalCount" class="tw-flex tw-gap-2">
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
      modal-width="52rem"
      :title="$t('PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.PREVIEW_TITLE')"
    >
      <template #content>
        <div class="tw-w-full">
        <p class="tw-mb-3 tw-text-sm tw-text-[color:var(--neutrals-600)]">
          {{ $t("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE.PREVIEW_LEAD", { count: preview?.totalCount ?? 0 }) }}
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
      modal-width="44rem"
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
import { useApiClient } from "@vc-shell/framework";
import { VcButton, VcButtonGroup, VcHint, VcIcon, VcInput, VcLabel, VcLoading, VcRadioButton, VcColumn, VcDataTable, VcPopup, VcSelect, VcStatus, VcTextarea } from "@vc-shell/framework/ui";

// Member is referenced by the @vue-generic annotations on the pickers.
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { CustomerModuleClient, Member, MemberSearchResult, MembersSearchCriteria } from "../../../api_client/virtocommerce.customer";
import { useAudiencePreview } from "../composables/useAudiencePreview";
import { AUDIENCE_FIELDS, AudienceField, ConditionOperator, findField, OPERATORS_BY_TYPE, WILDCARD_OPERATORS } from "../utils/audienceFields";
import { AudienceMode, blankRow, buildQuery, ConditionRow, combineDuplicateFields, detectAudience, hasContradiction, MAX_QUERY_LENGTH, parseQuery, validateRow } from "../utils/audienceQuery";

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

const generatedQuery = computed(() => buildQuery(currentState()));

/** Reference fields store ids; the summary has to say the name the author picked. */
const refNames = ref<Record<string, string>>({});

function labelForValue(row: ConditionRow): string {
  const value = row.value ?? "";
  const isRef = fieldOf(row).type === "ref";

  // The row stores a comma-joined list; the sentence reads it out with spaces.
  return value
    .split(",")
    .map((part) => (isRef ? (refNames.value[part] ?? part) : part))
    .join(", ");
}

function isCompany(opt: Member): boolean {
  return opt.memberType === "Organization";
}

function countOf(opt: Member): number | undefined {
  return opt.id ? memberCounts.value[opt.id] : undefined;
}

const A_PREFIX = "PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE";
const MODE_PREFIX = `${A_PREFIX}.MODES`;
const MODES: { mode: AudienceMode; key: string; icon: string }[] = [
  { mode: "everyone", key: "EVERYONE", icon: "lucide-globe" },
  { mode: "list", key: "LIST", icon: "lucide-users" },
  { mode: "conditions", key: "CONDITIONS", icon: "lucide-filter" },
  { mode: "query", key: "QUERY", icon: "lucide-code" },
];
const JOINS = ["all", "any"] as const;

/**
 * One-click starting points, from the design. Each replaces the current rows with a single
 * condition and leaves the value empty for the author to fill.
 */
interface Starter {
  key: string;
  mode: AudienceMode;
  row?: ConditionRow;
}

const STARTERS: Starter[] = [
  { key: "ALL_CUSTOMERS", mode: "everyone" },
  { key: "ONE_COMPANY", mode: "conditions", row: { field: "parentorganizations", operator: "is", value: "" } },
  { key: "BY_ROLE", mode: "conditions", row: { field: "role", operator: "is", value: "" } },
  { key: "EMAIL_DOMAIN", mode: "conditions", row: { field: "emails", operator: "endsWith", value: "" } },
  { key: "REGISTERED_SINCE", mode: "conditions", row: { field: "createddate", operator: "onOrAfter", value: "" } },
  { key: "TAGGED", mode: "conditions", row: { field: "groups", operator: "is", value: "" } },
];

interface OperatorOption {
  id: ConditionOperator;
  label: string;
}

const mode = ref<AudienceMode>("conditions");
const join = ref<"all" | "any">("all");
const rows = ref<ConditionRow[]>([blankRow()]);
const picked = ref<string[]>([]);
const rawQuery = ref("");

/** What we last told the parent, so an echo of our own emit is not mistaken for an edit. */
let emittedQuery: string | undefined;
let emittedIds: string[] | undefined;
/** True while a stored audience is being read in, so that read is not mistaken for an edit. */
let applying = false;

const fieldOptions = computed(() =>
  AUDIENCE_FIELDS.map((field) => ({ ...field, label: t(field.labelKey) })),
);

const contradiction = computed(() => hasContradiction(join.value, rows.value));

/** Reasons the audience cannot be saved, in the author's words. */
const problems = computed<string[]>(() => {
  const found: string[] = [];

  if (mode.value === "conditions") {
    for (const row of rows.value) {
      const error = row.value ? validateRow(row) : null;

      if (error && !found.includes(error)) {
        found.push(error);
      }
    }
  }

  if (generatedQuery.value.length > MAX_QUERY_LENGTH) {
    found.push(`${A_PREFIX}.VALIDATION.QUERY_TOO_LONG`);
  }

  return found;
});

const canReturnToConditions = computed(() => parseQuery(rawQuery.value) !== null);

const estimateLines = computed(() => {
  const value = preview.value;

  if (!value) {
    return [];
  }

  const prefix = "PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.ESTIMATE";
  const lines: { labelKey: string; labelArgs?: Record<string, unknown>; value: string }[] = [
    { labelKey: `${prefix}.MEMBERS_MATCHED`, value: `${value.membersMatched ?? 0}` },
  ];

  if (value.companiesExpanded) {
    lines.push({
      labelKey: `${prefix}.COMPANIES_EXPANDED`,
      labelArgs: { count: value.companiesExpanded },
      value: `+${value.peopleFromCompanies ?? 0}`,
    });
  }

  lines.push({ labelKey: `${prefix}.PEOPLE_IN_SCOPE`, value: `${value.peopleInScope ?? 0}` });

  if (value.extraLogins) {
    lines.push({ labelKey: `${prefix}.EXTRA_LOGINS`, value: `+${value.extraLogins}` });
  }

  return lines;
});

function fieldOf(row: ConditionRow): AudienceField {
  return findField(row.field) ?? AUDIENCE_FIELDS[0];
}

function operatorLabel(operator: ConditionOperator): string {
  const key = OPERATOR_KEYS[operator];

  return key ? t(`${A_PREFIX}.OPERATORS.${key}`) : "";
}

function operatorOptions(row: ConditionRow): OperatorOption[] {
  const available = OPERATORS_BY_TYPE[fieldOf(row).type];

  // A wildcard operator cannot carry a second value, so it is not offered once one is typed.
  // Runs on every render, including right after a control is cleared, so never assume a string.
  const value = row.value ?? "";
  const allowed = value.includes(",") ? available.filter((op) => !WILDCARD_OPERATORS.includes(op)) : available;

  return allowed.map((id) => ({ id, label: t(`PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.OPERATORS.${OPERATOR_KEYS[id]}`) }));
}

const OPERATOR_KEYS: Record<ConditionOperator, string> = {
  is: "IS",
  isNot: "IS_NOT",
  anyOf: "ANY_OF",
  startsWith: "STARTS_WITH",
  endsWith: "ENDS_WITH",
  contains: "CONTAINS",
  onOrAfter: "ON_OR_AFTER",
  onOrBefore: "ON_OR_BEFORE",
};

function valueControl(row: ConditionRow) {
  const field = fieldOf(row);

  if (field.type === "date") {
    return { is: VcInput, props: { type: "date" } };
  }

  if (field.type === "bool") {
    return { is: VcSelect, props: { emitValue: true, options: ["true", "false"] } };
  }

  if (field.type === "ref") {
    return {
      is: VcSelect,
      props: {
        emitValue: true,
        searchable: true,
        multiple: row.operator === "anyOf",
        optionValue: "id",
        optionLabel: "name",
        options: loadOrganizations,
      },
    };
  }

  if (field.options) {
    return { is: VcSelect, props: { emitValue: true, options: field.options } };
  }

  return { is: VcInput, props: {} };
}

/** Multi-value operators hand the control an array; the row always stores a comma-joined string. */
function controlValue(row: ConditionRow): string | string[] {
  const value = row.value ?? "";

  return row.operator === "anyOf" ? value.split(",").filter(Boolean) : value;
}

function toRowValue(value: unknown): string {
  if (value == null) {
    return "";
  }

  return Array.isArray(value) ? value.join(",") : String(value);
}

function rowError(row: ConditionRow): string | null {
  return row.value ? validateRow(row) : null;
}

function onFieldChange(row: ConditionRow) {
  const available = OPERATORS_BY_TYPE[fieldOf(row).type];

  if (!available.includes(row.operator)) {
    row.operator = available[0];
  }

  row.value = "";
}

function applyStarter(starter: Starter) {
  if (starter.mode === "everyone") {
    setMode("everyone");
    return;
  }

  join.value = "all";
  rows.value = starter.row ? [{ ...starter.row }] : [blankRow()];
  mode.value = "conditions";
}

function combineDuplicates() {
  rows.value = combineDuplicateFields(rows.value);
}

function addRow() {
  rows.value.push(blankRow());
}

function removeRow(index: number) {
  rows.value.splice(index, 1);

  if (rows.value.length === 0) {
    rows.value.push(blankRow());
  }
}

function setMode(next: AudienceMode) {
  const previous = mode.value;

  if (next === "query") {
    rawQuery.value = buildQuery(currentState());
  }

  // Only a phrase the author just edited by hand may replace the rows. Coming back from any
  // other mode, rawQuery is stale and the rows are what the author last worked on.
  if (next === "conditions" && previous === "query") {
    const parsed = parseQuery(rawQuery.value);

    if (parsed) {
      join.value = parsed.join;
      rows.value = parsed.rows;
    }
  }

  mode.value = next;
}

function currentState() {
  return {
    mode: mode.value,
    join: join.value,
    rows: rows.value,
    memberIds: picked.value,
    query: rawQuery.value,
  };
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

function applyIncoming(memberQuery?: string, memberIds?: string[]) {
  const detected = detectAudience(memberQuery, memberIds);

  // Reading a stored audience is not an edit. Without this the rebuilt phrase would be written
  // straight back over the stored one — normalising it, marking the blade dirty before the
  // author has touched anything, and erasing audiences whose phrase rebuilds to nothing.
  applying = true;

  picked.value = [...(memberIds ?? [])];
  rawQuery.value = memberQuery ?? "";
  join.value = detected.join;
  rows.value = detected.rows;
  mode.value = detected.mode;

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

interface SummaryPart {
  text: string;
  strong?: boolean;
  em?: boolean;
}

/**
 * Plain-language restatement of the audience, from the design: field names and the values
 * chosen for them are the parts worth reading, so they carry the emphasis.
 */
const summaryParts = computed<SummaryPart[]>(() => {
  const prefix = `${A_PREFIX}.ESTIMATE.SUMMARY`;

  if (mode.value === "everyone") {
    return [{ text: t(`${prefix}.EVERYONE`) }];
  }

  if (mode.value === "list") {
    return [{ text: t(`${prefix}.LIST`, { count: picked.value.length }) }];
  }

  if (mode.value === "query") {
    return [{ text: rawQuery.value.trim() ? t(`${prefix}.QUERY`) : t(`${prefix}.QUERY_EMPTY`) }];
  }

  const filled = rows.value.filter((row) => row.field && row.operator && row.value);

  if (!filled.length && !picked.value.length) {
    return [{ text: t(`${prefix}.NO_CONDITIONS`) }];
  }

  const parts: SummaryPart[] = [];

  if (filled.length) {
    parts.push({ text: t(`${prefix}.CONDITIONS_PREFIX`) + " " });

    filled.forEach((row, index) => {
      if (index > 0) {
        parts.push({ text: " " });
        parts.push({ text: t(`${prefix}.${join.value === "any" ? "OR" : "AND"}`), em: true });
        parts.push({ text: " " });
      }

      parts.push({ text: t(fieldOf(row).labelKey), strong: true });
      parts.push({ text: ` ${operatorLabel(row.operator)} ` });
      parts.push({ text: labelForValue(row), strong: true });
    });
  }

  pickedMembers.value.forEach((member, index) => {
    parts.push({ text: index === 0 ? t(`${prefix}.PLUS_PREFIX`) + " " : ", " });
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
      email: (r.memberId && emailByMember.get(r.memberId)) || "",
      login: r.userName ?? "",
    }));
  } catch {
    previewFailed.value = true;
    previewRows.value = [];
  } finally {
    loadingPage.value = false;
  }
}

watch(
  rows,
  async (current) => {
    const ids = current
      .filter((row) => fieldOf(row).type === "ref" && row.value)
      .flatMap((row) => (row.value ?? "").split(","))
      .filter((id) => id && refNames.value[id] === undefined);

    if (!ids.length) {
      return;
    }

    const members = (await loadMembers(undefined, 0, [...new Set(ids)])).results ?? [];

    for (const member of members) {
      if (member.id) {
        refNames.value[member.id] = member.name ?? member.id;
      }
    }
  },
  { deep: true },
);

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
  [mode, join, rows, picked, rawQuery],
  () => {
    if (applying) {
      return;
    }

    const phrase = buildQuery(currentState());
    // Everyone hides the picker, so anything left in it is not part of that audience.
    const ids = mode.value === "everyone" ? [] : [...picked.value];

    emittedQuery = phrase || undefined;
    emittedIds = ids.length ? ids : undefined;

    emit("update:memberQuery", emittedQuery);
    emit("update:memberIds", emittedIds);
    emit("update:invalid", problems.value.length > 0);

    refreshPreview(emittedQuery, emittedIds);
  },
  { deep: true },
);
</script>
