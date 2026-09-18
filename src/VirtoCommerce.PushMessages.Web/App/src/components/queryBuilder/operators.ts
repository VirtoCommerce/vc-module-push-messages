import type { ConditionOperator, QueryFieldType } from "./types";

export const OPERATORS_BY_TYPE: Record<QueryFieldType, ConditionOperator[]> = {
  text: ["is", "isNot", "anyOf", "startsWith", "endsWith", "contains"],
  enum: ["is", "isNot", "anyOf"],
  ref: ["is", "isNot", "anyOf"],
  date: ["onOrAfter", "onOrBefore"],
  bool: ["is"],
};

/**
 * These generate a quoted `*` value. A filter builder only takes its wildcard branch when the
 * filter carries a single value, so a second value silently turns the phrase into an exact terms
 * query that matches nobody.
 */
export const WILDCARD_OPERATORS: ConditionOperator[] = ["startsWith", "endsWith", "contains"];

export const OPERATOR_KEYS: Record<ConditionOperator, string> = {
  is: "IS",
  isNot: "IS_NOT",
  anyOf: "ANY_OF",
  startsWith: "STARTS_WITH",
  endsWith: "ENDS_WITH",
  contains: "CONTAINS",
  onOrAfter: "ON_OR_AFTER",
  onOrBefore: "ON_OR_BEFORE",
};
