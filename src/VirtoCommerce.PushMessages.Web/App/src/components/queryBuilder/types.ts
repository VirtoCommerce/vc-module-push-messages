export type QueryFieldType = "text" | "enum" | "ref" | "bool" | "date";

export type ConditionOperator =
  | "is"
  | "isNot"
  | "anyOf"
  | "startsWith"
  | "endsWith"
  | "contains"
  | "onOrAfter"
  | "onOrBefore";

/** A page of choices for a `ref` field, shaped like every search result the platform returns. */
export interface QueryFieldOptions {
  results?: { id?: string; name?: string }[];
  totalCount?: number;
}

/**
 * One field the author may build a condition on. The id is the field name as the index knows it —
 * it goes into the phrase verbatim — and the label is what the author reads.
 */
export interface QueryField {
  id: string;
  label: string;
  type: QueryFieldType;
  /** Fixed choices, for an `enum` whose values are known up front. */
  options?: string[];
  /** Where a `ref` field's choices come from. Ids are stored; names are displayed. */
  load?: (keyword?: string, skip?: number, ids?: string[]) => Promise<QueryFieldOptions>;
}

export interface ConditionRow {
  field: string;
  operator: ConditionOperator;
  value: string;
}

export type ConditionJoin = "all" | "any";

/** A one-click starting point. Without a row it is a choice the host handles itself. */
export interface QueryStarter {
  key: string;
  label: string;
  row?: ConditionRow;
}

/** A piece of the plain-language restatement of the conditions. */
export interface DescriptionPart {
  text: string;
  strong?: boolean;
  em?: boolean;
}
