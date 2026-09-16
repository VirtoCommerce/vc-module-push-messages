import { AUDIENCE_FIELDS, WILDCARD_OPERATORS } from "./audienceFields";
import type { ConditionOperator } from "./audienceFields";

export type AudienceMode = "everyone" | "list" | "conditions" | "query";

export interface ConditionRow {
  field: string;
  operator: ConditionOperator;
  value: string;
}

export interface AudienceState {
  mode: AudienceMode;
  join: "all" | "any";
  rows: ConditionRow[];
  memberIds: string[];
  query: string;
}

export const EVERYONE_QUERY = "membertype:Contact";

const VALIDATION_PREFIX = "PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.VALIDATION";

/** Mirrors the ANTLR lexer rule SimpleString : [\p{L}\p{N}_\-./@+]+ */
const SIMPLE_STRING = /^[\p{L}\p{N}_\-./@+]+$/u;

/** `field:value`, `!field:value`, `field:a,b`, `field:[a TO]`, `field:[TO b]` — nothing else. */
const CLAUSE = /^(!?)([A-Za-z][A-Za-z0-9_]*):(.+)$/;

const RANGE = /^\[(.*)\s+TO\s*\]$|^\[\s*TO\s+(.*)\]$/;

function quote(value: string): string {
  return SIMPLE_STRING.test(value) ? value : `"${value.replace(/(["\\])/g, "\\$1")}"`;
}

function unquote(value: string): string {
  return value.startsWith('"') && value.endsWith('"') && value.length > 1
    ? value.slice(1, -1).replace(/\\(["\\])/g, "$1")
    : value;
}

function rowToQuery(row: ConditionRow): string {
  const { field, value } = row;

  switch (row.operator) {
    case "is":
      return `${field}:${quote(value)}`;
    case "isNot":
      return `!${field}:${quote(value)}`;
    case "anyOf":
      return `${field}:${value
        .split(",")
        .map((part) => quote(part.trim()))
        .join(",")}`;
    case "startsWith":
      return `${field}:"${value}*"`;
    case "endsWith":
      return `${field}:"*${value}"`;
    case "contains":
      return `${field}:"*${value}*"`;
    case "onOrAfter":
      return `${field}:[${value} TO]`;
    case "onOrBefore":
      return `${field}:[TO ${value}]`;
  }
}

export function buildQuery(state: AudienceState): string {
  if (state.mode === "everyone") {
    return EVERYONE_QUERY;
  }

  if (state.mode === "list") {
    return "";
  }

  if (state.mode === "query") {
    return state.query;
  }

  const parts = state.rows.filter((row) => row.value).map(rowToQuery);

  return state.join === "any" ? parts.join(" OR ") : parts.join(" ");
}

/**
 * Splits on top-level whitespace. Quotes and range brackets hold their own spaces, so
 * `createddate:[2025-01-01 TO]` and `name:"Acme B.V."` each stay one clause.
 * Returns null for anything the builder cannot represent.
 */
function splitTopLevel(phrase: string): string[] | null {
  const parts: string[] = [];
  let current = "";
  let quoted = false;
  let inRange = false;

  for (let i = 0; i < phrase.length; i++) {
    const char = phrase[i];

    if (char === '"' && phrase[i - 1] !== "\\") {
      quoted = !quoted;
    }

    if (!quoted) {
      if (char === "(" || char === ")") {
        return null;
      }

      if (char === "[") {
        inRange = true;
      }

      if (char === "]") {
        inRange = false;
      }
    }

    if (char === " " && !quoted && !inRange) {
      if (current) {
        parts.push(current);
        current = "";
      }
      continue;
    }

    current += char;
  }

  if (quoted || inRange) {
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

function clauseToRow(clause: string): ConditionRow | null {
  const match = CLAUSE.exec(clause);

  if (!match) {
    return null;
  }

  const [, negation, field, rawValue] = match;
  const range = RANGE.exec(rawValue);

  if (range) {
    if (negation) {
      return null;
    }

    return range[1] !== undefined
      ? { field, operator: "onOrAfter", value: range[1].trim() }
      : { field, operator: "onOrBefore", value: (range[2] ?? "").trim() };
  }

  const values = splitValues(rawValue).map(unquote);

  if (values.length > 1) {
    return negation ? null : { field, operator: "anyOf", value: values.join(",") };
  }

  const value = values[0];
  const wasQuoted = rawValue.startsWith('"');

  if (wasQuoted && value.startsWith("*") && value.endsWith("*") && value.length > 2) {
    return negation ? null : { field, operator: "contains", value: value.slice(1, -1) };
  }

  if (wasQuoted && value.endsWith("*")) {
    return negation ? null : { field, operator: "startsWith", value: value.slice(0, -1) };
  }

  if (wasQuoted && value.startsWith("*")) {
    return negation ? null : { field, operator: "endsWith", value: value.slice(1) };
  }

  if (value.includes("*") || value.includes("?")) {
    return null;
  }

  return { field, operator: negation ? "isNot" : "is", value };
}

/**
 * Turns a search phrase back into condition rows, or returns null when the phrase uses more of
 * the grammar than the builder can show. Null is the documented signal to open the message in
 * Advanced query mode with the phrase untouched.
 */
export function parseQuery(phrase: string): { join: "all" | "any"; rows: ConditionRow[] } | null {
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

export function validateRow(row: ConditionRow): string | null {
  // A cleared control hands back undefined, not an empty string.
  const value = row.value ?? "";

  if (!value.trim()) {
    return `${VALIDATION_PREFIX}.VALUE_REQUIRED`;
  }

  if (WILDCARD_OPERATORS.includes(row.operator) && value.includes(",")) {
    return `${VALIDATION_PREFIX}.WILDCARD_SINGLE_VALUE`;
  }

  return null;
}

export function blankRow(): ConditionRow {
  return { field: AUDIENCE_FIELDS[0].id, operator: "is", value: "" };
}

/**
 * Works out which mode a stored audience should open in. Order matters and the first match
 * wins: a picked list with no phrase is a list, the Everyone phrase on its own is Everyone, a
 * phrase the builder can represent is conditions, and anything else opens as a raw query.
 */
export function detectAudience(
  memberQuery?: string,
  memberIds?: string[],
): { mode: AudienceMode; join: "all" | "any"; rows: ConditionRow[] } {
  const hasIds = (memberIds?.length ?? 0) > 0;

  if (!memberQuery && hasIds) {
    return { mode: "list", join: "all", rows: [blankRow()] };
  }

  if (memberQuery === EVERYONE_QUERY && !hasIds) {
    return { mode: "everyone", join: "all", rows: [blankRow()] };
  }

  if (!memberQuery) {
    return { mode: "conditions", join: "all", rows: [blankRow()] };
  }

  const parsed = parseQuery(memberQuery);

  if (parsed) {
    return { mode: "conditions", join: parsed.join, rows: parsed.rows };
  }

  return { mode: "query", join: "all", rows: [blankRow()] };
}

/**
 * Two exact conditions on the same field joined with ALL can never both hold, so the audience
 * is empty for a reason that is invisible in the rows themselves.
 */
export function hasContradiction(join: "all" | "any", rows: ConditionRow[]): boolean {
  if (join !== "all") {
    return false;
  }

  const fields = rows.filter((row) => row.operator === "is" && row.value).map((row) => row.field);

  return new Set(fields).size !== fields.length;
}

/**
 * Folds repeated exact conditions on one field into a single "is any of" row — the shape the
 * search phrase can actually satisfy.
 */
export function combineDuplicateFields(rows: ConditionRow[]): ConditionRow[] {
  const merged: ConditionRow[] = [];

  for (const row of rows) {
    const twin =
      row.operator === "is" && row.value
        ? merged.find((m) => m.field === row.field && (m.operator === "is" || m.operator === "anyOf"))
        : undefined;

    if (twin) {
      twin.value = `${twin.value},${row.value}`;
      twin.operator = "anyOf";
      continue;
    }

    merged.push({ ...row });
  }

  return merged;
}
