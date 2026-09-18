<template>
  <div class="tw-space-y-3">
    <div
      v-if="starters?.length"
      class="tw-space-y-2"
    >
      <p class="tw-text-xs tw-uppercase tw-tracking-wider tw-text-[color:var(--neutrals-400)]">
        {{ $t("QUERY_BUILDER.STARTERS") }}
      </p>
      <div class="tw-flex tw-flex-wrap tw-gap-2">
        <VcButton
          v-for="starter in starters"
          :key="starter.key"
          variant="outline"
          size="xs"
          :disabled="disabled"
          @click="applyStarter(starter)"
        >
          {{ starter.label }}
        </VcButton>
      </div>
    </div>

    <div
      v-if="started"
      class="tw-space-y-3"
    >
      <div class="tw-flex tw-items-center tw-gap-2">
        <span>{{ $t("QUERY_BUILDER.MATCH") }}</span>
        <VcButtonGroup
          attached
          size="sm"
        >
          <VcButton
            v-for="candidate in JOINS"
            :key="candidate"
            :variant="join === candidate ? 'primary' : 'outline'"
            :disabled="disabled"
            @click="join = candidate"
          >
            {{ $t(`QUERY_BUILDER.${candidate === "all" ? "ALL" : "ANY"}`) }}
          </VcButton>
        </VcButtonGroup>
        <span>{{ $t("QUERY_BUILDER.OF_THESE_CONDITIONS") }}</span>
      </div>

      <div
        v-for="(row, index) in rows"
        :key="index"
        class="tw-space-y-1"
      >
        <div
          v-if="index > 0"
          class="tw-text-xs tw-font-semibold tw-tracking-wider tw-text-[color:var(--neutrals-400)]"
        >
          {{ $t(`QUERY_BUILDER.JOINER.${join === "any" ? "OR" : "AND"}`) }}
        </div>
        <div class="tw-flex tw-items-center tw-gap-2">
          <VcSelect
            v-model="row.field"
            emit-value
            :clearable="false"
            option-value="id"
            option-label="label"
            class="tw-w-1/3"
            :options="fields"
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
            :is="valueControl(row, index).is"
            :key="`${fieldOf(row).type}:${listGeneration[index] ?? 0}`"
            v-bind="valueControl(row, index).props"
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
        <VcHint
          v-if="rowError(row)"
          class="tw-text-[color:var(--danger-500)]"
        >
          {{ $t(rowError(row) as string) }}
        </VcHint>
      </div>

      <div
        v-if="contradiction"
        class="tw-space-y-2"
      >
        <VcHint class="tw-text-[color:var(--warning-600)]">
          {{ $t("QUERY_BUILDER.CONTRADICTION") }}
        </VcHint>
        <VcButton
          v-if="canCombine"
          variant="outline"
          size="sm"
          icon="lucide-merge"
          :disabled="disabled"
          @click="combineDuplicates"
        >
          {{ $t("QUERY_BUILDER.COMBINE") }}
        </VcButton>
      </div>

      <div class="tw-flex tw-gap-4">
        <VcButton
          variant="outline"
          size="sm"
          icon="lucide-plus"
          :disabled="disabled"
          @click="addRow"
        >
          {{ $t("QUERY_BUILDER.ADD_CONDITION") }}
        </VcButton>
        <VcButton
          v-if="showEditAsQuery"
          variant="link"
          size="sm"
          icon="lucide-arrow-right"
          :disabled="disabled"
          @click="emit('edit-as-query')"
        >
          {{ $t("QUERY_BUILDER.EDIT_AS_QUERY") }}
        </VcButton>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { computed, nextTick, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { VcButton, VcButtonGroup, VcHint, VcInput, VcSelect } from "@vc-shell/framework/ui";

import { OPERATOR_KEYS, OPERATORS_BY_TYPE, WILDCARD_OPERATORS } from "./operators";
import {
  blankRow,
  buildPhrase,
  combineDuplicateFields,
  hasContradiction,
  parsePhrase,
  toRowValue,
  validateRow,
} from "./phrase";
import type {
  ConditionJoin,
  ConditionOperator,
  ConditionRow,
  DescriptionPart,
  QueryField,
  QueryStarter,
} from "./types";

const props = defineProps<{
  /** The search phrase this builder reads and writes. */
  query?: string;
  /** The fields conditions may be built on, in the order they are offered. */
  fields: QueryField[];
  /** One-click starting points. A starter without a row is reported to the host instead. */
  starters?: QueryStarter[];
  /** Whether to offer the way out to a raw phrase; the host owns that editor. */
  showEditAsQuery?: boolean;
  disabled?: boolean;
}>();

const emit = defineEmits<{
  "update:query": [value: string];
  /** True while a condition cannot be turned into a phrase. */
  "update:invalid": [value: boolean];
  /** The conditions in plain language, for a host that restates the query to the author. */
  "update:description": [value: DescriptionPart[]];
  "edit-as-query": [];
  /** A starter the builder cannot apply itself, because it carries no condition. */
  starter: [value: QueryStarter];
}>();

const { t } = useI18n({ useScope: "global" });

const JOINS: ConditionJoin[] = ["all", "any"];

const join = ref<ConditionJoin>("all");
const rows = ref<ConditionRow[]>([blankRow(props.fields)]);

/**
 * Whether the condition editor is open. It opens on a choice from the starting points and closes
 * again when the last condition is removed.
 */
const started = ref(false);

/** Bumped when a reference list closes, to make the next opening load its choices afresh. */
const listGeneration = ref<Record<number, number>>({});

/** Reference fields store ids; the description has to say the name the author picked. */
const refNames = ref<Record<string, string>>({});

/** What we last told the host, so an echo of our own emit is not mistaken for an edit. */
let emitted: string | undefined;
/** True while an incoming phrase is being read in, so that read is not mistaken for an edit. */
let applying = false;

const contradiction = computed(() => hasContradiction(join.value, rows.value));

/** Some contradictions cannot be folded — a yes/no field has no "is any of" to fold into. */
const canCombine = computed(
  () => combineDuplicateFields(rows.value, props.fields).length < rows.value.length,
);

const problems = computed(() => rows.value.some((row) => (row.value ? validateRow(row) : null)));

function fieldOf(row: ConditionRow): QueryField {
  return props.fields.find((field) => field.id === row.field) ?? props.fields[0];
}

function operatorLabel(operator: ConditionOperator): string {
  return t(`QUERY_BUILDER.OPERATORS.${OPERATOR_KEYS[operator]}`);
}

function operatorOptions(row: ConditionRow): { id: ConditionOperator; label: string }[] {
  const available = OPERATORS_BY_TYPE[fieldOf(row).type];

  // A wildcard operator cannot carry a second value, so it is not offered once one is typed.
  // Runs on every render, including right after a control is cleared, so never assume a string.
  const value = row.value ?? "";
  const allowed = value.includes(",") ? available.filter((op) => !WILDCARD_OPERATORS.includes(op)) : available;

  return allowed.map((id) => ({ id, label: operatorLabel(id) }));
}

function valueControl(row: ConditionRow, index: number) {
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
        options: optionsFor(index),
        onClose: () => {
          listGeneration.value[index] = (listGeneration.value[index] ?? 0) + 1;
        },
      },
    };
  }

  if (field.options) {
    return { is: VcSelect, props: { emitValue: true, options: field.options } };
  }

  return { is: VcInput, props: {} };
}

/**
 * One loader per row, kept stable so the select is not handed a new function on every render.
 * It reads the row as it stands when the list is opened, which is what lets it leave out values
 * the row already carries — a value chosen twice means nothing to the phrase, and offering it
 * again reads as though it had not been chosen at all.
 */
const optionLoaders = new Map<number, QueryField["load"]>();

function optionsFor(index: number): QueryField["load"] {
  let loader = optionLoaders.get(index);

  if (!loader) {
    loader = async (keyword?: string, skip?: number, ids?: string[]) => {
      const row = rows.value[index];
      const load = row ? fieldOf(row).load : undefined;

      if (!row || !load) {
        return { results: [], totalCount: 0 };
      }

      const result = await load(keyword, skip, ids);

      // Asking by id is the select resolving what it already holds; those must come back.
      if (ids?.length || row.operator !== "anyOf") {
        return result;
      }

      const chosen = new Set((row.value ?? "").split(",").filter(Boolean));
      const results = (result.results ?? []).filter((option) => !option.id || !chosen.has(option.id));

      return { results, totalCount: result.totalCount ?? results.length };
    };

    optionLoaders.set(index, loader);
  }

  return loader;
}

/** Multi-value operators hand the control an array; the row always stores a comma-joined string. */
function controlValue(row: ConditionRow): string | string[] {
  const value = row.value ?? "";

  return row.operator === "anyOf" ? value.split(",").filter(Boolean) : value;
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

function applyStarter(starter: QueryStarter) {
  if (!starter.row) {
    emit("starter", starter);
    return;
  }

  const row = { ...starter.row };

  // The starting points stay on screen while the conditions are edited, so one can be picked with
  // work already on the page. It is added to that work rather than put in place of it.
  if (started.value && rows.value.some((existing) => existing.value)) {
    rows.value.push(row);
  } else {
    join.value = "all";
    rows.value = [row];
  }

  started.value = true;
}

function combineDuplicates() {
  rows.value = combineDuplicateFields(rows.value, props.fields);
}

function addRow() {
  rows.value.push(blankRow(props.fields));
}

function removeRow(index: number) {
  rows.value.splice(index, 1);

  if (rows.value.length === 0) {
    rows.value.push(blankRow(props.fields));
    started.value = false;
  }
}

/** Reads a phrase the host handed in. Returns false when the builder cannot show it. */
function applyIncoming(phrase?: string): boolean {
  const parsed = phrase ? parsePhrase(phrase) : null;

  applying = true;
  emitted = phrase;

  if (parsed) {
    join.value = parsed.join;
    rows.value = parsed.rows;
    started.value = parsed.rows.some((row) => row.value);
  } else {
    join.value = "all";
    rows.value = [blankRow(props.fields)];
    started.value = false;
  }

  nextTick(() => {
    applying = false;
  });

  return parsed !== null;
}

defineExpose({ applyIncoming });

const description = computed<DescriptionPart[]>(() => {
  const filled = rows.value.filter((row) => row.field && row.operator && row.value);
  const parts: DescriptionPart[] = [];

  filled.forEach((row, index) => {
    if (index > 0) {
      parts.push({ text: " " });
      parts.push({ text: t(`QUERY_BUILDER.${join.value === "any" ? "OR" : "AND"}`), em: true });
      parts.push({ text: " " });
    }

    parts.push({ text: fieldOf(row).label, strong: true });
    parts.push({ text: ` ${operatorLabel(row.operator)} ` });
    parts.push({ text: labelForValue(row), strong: true });
  });

  return parts;
});

function labelForValue(row: ConditionRow): string {
  const value = row.value ?? "";
  const isRef = fieldOf(row).type === "ref";

  // The row stores a comma-joined list; the sentence reads it out with spaces.
  return value
    .split(",")
    .map((part) => (isRef ? (refNames.value[part] ?? part) : part))
    .join(", ");
}

watch(
  () => props.query,
  (phrase) => {
    if ((phrase ?? "") !== (emitted ?? "")) {
      applyIncoming(phrase);
    }
  },
  { immediate: true },
);

// Reference fields store ids, so the names have to be fetched before anything can read them out.
watch(
  rows,
  async (current) => {
    for (const field of props.fields) {
      if (field.type !== "ref" || !field.load) {
        continue;
      }

      const ids = [
        ...new Set(
          current
            .filter((row) => row.field === field.id && row.value)
            .flatMap((row) => (row.value ?? "").split(","))
            .filter((id) => id && refNames.value[id] === undefined),
        ),
      ];

      if (!ids.length) {
        continue;
      }

      for (const option of (await field.load(undefined, 0, ids)).results ?? []) {
        if (option.id) {
          refNames.value[option.id] = option.name ?? option.id;
        }
      }
    }
  },
  { deep: true },
);

// refNames is watched as well: a name arriving late changes the description, not the phrase.
watch(
  [rows, join, refNames],
  () => {
    if (applying) {
      return;
    }

    emitted = buildPhrase(rows.value, join.value);

    emit("update:query", emitted);
    emit("update:invalid", problems.value);
    emit("update:description", description.value);
  },
  { deep: true, immediate: true },
);
</script>
