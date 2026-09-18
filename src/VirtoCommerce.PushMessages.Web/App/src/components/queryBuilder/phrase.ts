import { OPERATORS_BY_TYPE, WILDCARD_OPERATORS } from "./operators";
import type { ConditionJoin, ConditionRow, QueryField } from "./types";

const VALIDATION_PREFIX = "QUERY_BUILDER.VALIDATION";

/** Mirrors the ANTLR lexer rule SimpleString : [\p{L}\p{N}_\-./@+]+ */
const SIMPLE_STRING = /^[\p{L}\p{N}_\-./@+]+$/u;

/** `field:value`, `!field:value`, `field:a,b`, `field:[a TO]`, `field:[TO b]` — nothing else. */
const CLAUSE = /^(!?)([A-Za-z]\w*):(.+)$/;

const RANGE = /^\[(.*)\s+TO\s*\]$|^\[\s*TO\s+(.*)\]$/;

function escape(value: string): string {
  return value.replace(/(["\\])/g, "\\$1");
}

function quote(value: string): string {
  return SIMPLE_STRING.test(value) ? value : `"${escape(value)}"`;
}

function unquote(value: string): string {
  return value.startsWith('"') && value.endsWith('"') && value.length > 1
    ? value.slice(1, -1).replace(/\\(["\\])/g, "$1")
    : value;
}

export function rowToPhrase(row: ConditionRow): string {
  const { field, value } = row;

  switch (row.operator) {
    case "is":
      return `${field}:${quote(value)}`;
    case "isNot":
      return `!${field}:${quote(value)}`;
    case "anyOf":
      return `${field}:${[...new Set(value.split(",").map((part) => part.trim()))]
        .map(quote)
        .join(",")}`;
    case "startsWith":
      return `${field}:"${escape(value)}*"`;
    case "endsWith":
      return `${field}:"*${escape(value)}"`;
    case "contains":
      return `${field}:"*${escape(value)}*"`;
    case "onOrAfter":
      return `${field}:[${value} TO]`;
    case "onOrBefore":
      return `${field}:[TO ${value}]`;
    default:
      return "";
  }
}

export function buildPhrase(rows: ConditionRow[], join: ConditionJoin): string {
  const parts = rows.filter((row) => row.value).map(rowToPhrase);

  return join === "any" ? parts.join(" OR ") : parts.join(" ");
}

/**
 * Splits on top-level whitespace. Quotes and range brackets hold their own spaces, so
 * `createddate:[2025-01-01 TO]` and `name:"Acme B.V."` each stay one clause.
 * Returns null for anything the builder cannot represent.
 */
interface Scanner {
  quoted: boolean;
  ranges: number;
}

/** Takes one character into account. False means the phrase is beyond what the builder can show. */
function scan(state: Scanner, char: string, escaped: boolean): boolean {
  if (char === '"' && !escaped) {
    state.quoted = !state.quoted;
    return true;
  }

  if (state.quoted) {
    return true;
  }

  if (char === "(" || char === ")") {
    return false;
  }

  if (char === "[") {
    state.ranges++;
  }

  if (char === "]") {
    state.ranges--;
  }

  return true;
}

function splitTopLevel(phrase: string): string[] | null {
  const state: Scanner = { quoted: false, ranges: 0 };
  const parts: string[] = [];
  let current = "";

  for (let i = 0; i < phrase.length; i++) {
    const char = phrase[i];

    if (!scan(state, char, phrase[i - 1] === "\\")) {
      return null;
    }

    if (char === " " && !state.quoted && state.ranges === 0) {
      if (current) {
        parts.push(current);
      }

      current = "";
      continue;
    }

    current += char;
  }

  if (state.quoted || state.ranges !== 0) {
    return null;
  }

  if (current) {
    parts.push(current);
  }

  return parts;
}

function splitValues(raw: string): string[] {
  const values: string[] = [];
  let current = "";
  let quoted = false;

  for (let i = 0; i < raw.length; i++) {
    const char = raw[i];

    if (char === '"' && raw[i - 1] !== "\\") {
      quoted = !quoted;
    }

    if (char === "," && !quoted) {
      values.push(current);
      current = "";
      continue;
    }

    current += char;
  }

  values.push(current);

  return values;
}

/** Reads `[a TO]` or `[TO b]`, or nothing when the value is not a range at all. */
function rangeRow(field: string, rawValue: string): ConditionRow | null {
  const range = RANGE.exec(rawValue);

  if (!range) {
    return null;
  }

  return range[1] !== undefined
    ? { field, operator: "onOrAfter", value: range[1].trim() }
    : { field, operator: "onOrBefore", value: (range[2] ?? "").trim() };
}

/** Reads a quoted value carrying `*` at one end or both, or nothing when it carries none. */
function wildcardRow(field: string, value: string): ConditionRow | null {
  if (value.startsWith("*") && value.endsWith("*") && value.length > 2) {
    return { field, operator: "contains", value: value.slice(1, -1) };
  }

  if (value.endsWith("*") && value.length > 1) {
    return { field, operator: "startsWith", value: value.slice(0, -1) };
  }

  if (value.startsWith("*") && value.length > 1) {
    return { field, operator: "endsWith", value: value.slice(1) };
  }

  return null;
}

function clauseToRow(clause: string): ConditionRow | null {
  const match = CLAUSE.exec(clause);

  if (!match) {
    return null;
  }

  const [, negation, field, rawValue] = match;

  // Only an exact match reads back negated; every other shape drops out of what the builder shows.
  const range = rangeRow(field, rawValue);

  if (range) {
    return negation ? null : range;
  }

  const values = [...new Set(splitValues(rawValue).map(unquote))];

  if (values.length > 1) {
    return negation ? null : { field, operator: "anyOf", value: values.join(",") };
  }

  const value = values[0];
  const wildcard = rawValue.startsWith('"') ? wildcardRow(field, value) : null;

  if (wildcard) {
    return negation ? null : wildcard;
  }

  if (!value || value.includes("*") || value.includes("?")) {
    return null;
  }

  return { field, operator: negation ? "isNot" : "is", value };
}

/**
 * Turns a search phrase back into condition rows, or returns null when the phrase uses more of
 * the grammar than the builder can show. Null is the documented signal to open the message in
 * Advanced query mode with the phrase untouched.
 */
export function parsePhrase(phrase: string): { join: ConditionJoin; rows: ConditionRow[] } | null {
  const trimmed = phrase.trim();

  if (!trimmed) {
    return null;
  }

  const parts = splitTopLevel(trimmed);

  if (!parts || parts.length === 0) {
    return null;
  }

  const hasOr = parts.includes("OR");

  if (hasOr && parts.includes("AND")) {
    return null;
  }

  // With OR present the phrase must alternate clause, OR, clause, …
  // "a b OR c" mixes joins and is beyond what the builder can show.
  if (hasOr) {
    const alternates = parts.every((part, index) => (index % 2 === 1 ? part === "OR" : part !== "OR"));

    if (!alternates || parts.length % 2 === 0) {
      return null;
    }
  }

  const clauses = parts.filter((part) => part !== "OR" && part !== "AND");
  const rows: ConditionRow[] = [];

  for (const clause of clauses) {
    const row = clauseToRow(clause);

    if (!row) {
      return null;
    }

    rows.push(row);
  }

  return { join: hasOr ? "any" : "all", rows };
}

/**
 * Normalises whatever a value control hands back into the string a row stores. The date control
 * hands back a Date, whose default string is a locale sentence; the search phrase needs the plain
 * date, read in local terms — toISOString would shift it a day east.
 */
export function toRowValue(value: unknown): string {
  if (value == null) {
    return "";
  }

  if (value instanceof Date) {
    const month = `${value.getMonth() + 1}`.padStart(2, "0");
    const day = `${value.getDate()}`.padStart(2, "0");

    return `${value.getFullYear()}-${month}-${day}`;
  }

  // A value repeated in a multi-value control means nothing to the phrase — `role:a,a` is `role:a`.
  return Array.isArray(value) ? [...new Set(value.map(String))].join(",") : String(value);
}

export function validateRow(row: ConditionRow): string | null {
  // A cleared control hands back undefined, not an empty string.
  const value = row.value ?? "";

  if (!row.field || !row.operator) {
    return `${VALIDATION_PREFIX}.INCOMPLETE`;
  }

  if (!value.trim()) {
    return `${VALIDATION_PREFIX}.VALUE_REQUIRED`;
  }

  if (WILDCARD_OPERATORS.includes(row.operator) && value.includes(",")) {
    return `${VALIDATION_PREFIX}.WILDCARD_SINGLE_VALUE`;
  }

  return null;
}

export function blankRow(fields: QueryField[]): ConditionRow {
  const first = fields[0];

  return { field: first?.id ?? "", operator: OPERATORS_BY_TYPE[first?.type ?? "text"][0], value: "" };
}

/**
 * Two exact conditions on the same field joined with ALL can never both hold, so the audience
 * is empty for a reason that is invisible in the rows themselves.
 */
export function hasContradiction(join: ConditionJoin, rows: ConditionRow[]): boolean {
  if (join !== "all") {
    return false;
  }

  // "is" and "is any of" both pin a field to a value; two such rows on one field under ALL can
  // never both hold, whichever of the two forms they take.
  const fields = rows
    .filter((row) => (row.operator === "is" || row.operator === "anyOf") && row.value)
    .map((row) => row.field);

  return new Set(fields).size !== fields.length;
}

/**
 * Folds repeated exact conditions on one field into a single "is any of" row — the shape the
 * search phrase can actually satisfy.
 */
export function combineDuplicateFields(rows: ConditionRow[], fields: QueryField[]): ConditionRow[] {
  const merged: ConditionRow[] = [];

  for (const row of rows) {
    // Only fold into a form the field can actually be given: a yes/no field has no "is any of",
    // and a row whose operator the select cannot name is worse than two rows.
    const field = fields.find((candidate) => candidate.id === row.field);
    const foldable = field ? OPERATORS_BY_TYPE[field.type].includes("anyOf") : false;

    const twin =
      foldable && row.operator === "is" && row.value
        ? merged.find((m) => m.field === row.field && (m.operator === "is" || m.operator === "anyOf"))
        : undefined;

    if (twin) {
      // Folding two conditions on the same value leaves one value, not the same one twice.
      twin.value = [...new Set(`${twin.value},${row.value}`.split(","))].join(",");
      twin.operator = "anyOf";
      continue;
    }

    merged.push({ ...row });
  }

  return merged;
}
