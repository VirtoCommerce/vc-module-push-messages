export type AudienceFieldType = "text" | "enum" | "ref" | "bool" | "date";

export type ConditionOperator =
  | "is"
  | "isNot"
  | "anyOf"
  | "startsWith"
  | "endsWith"
  | "contains"
  | "onOrAfter"
  | "onOrBefore";

export interface AudienceField {
  /** Index field name, lower-cased as the search phrase parser expects it. */
  id: string;
  labelKey: string;
  type: AudienceFieldType;
  /** Fixed choices. Absent means the value is typed in — the choices live in customer data. */
  options?: string[];
}

const FIELDS_PREFIX = "PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.FIELDS";

/**
 * Field names come from MemberDocumentBuilder.BuildSchemaAsync() in vc-module-customer.
 * Two are worth knowing about:
 * - `defaultlanguage` is the real field for preferred language; there is no `preferredlanguage`.
 * - `businesscategory` is indexed on organization documents only, so matching it selects
 *   companies, whose members the send job then expands.
 *
 * The list is flat because VcSelect has no option groups; the order below is the grouping the
 * design shows, and the labels are what make a field readable — "Company", not
 * "parentorganizations".
 */
export const AUDIENCE_FIELDS: AudienceField[] = [
  { id: "name", labelKey: `${FIELDS_PREFIX}.NAME`, type: "text" },
  { id: "emails", labelKey: `${FIELDS_PREFIX}.EMAIL`, type: "text" },
  { id: "login", labelKey: `${FIELDS_PREFIX}.USERNAME`, type: "text" },
  {
    id: "membertype",
    labelKey: `${FIELDS_PREFIX}.CUSTOMER_TYPE`,
    type: "enum",
    options: ["Contact", "Organization", "Employee", "Vendor"],
  },
  {
    id: "status",
    labelKey: `${FIELDS_PREFIX}.STATUS`,
    type: "enum",
    options: ["Approved", "New", "Rejected"],
  },
  { id: "groups", labelKey: `${FIELDS_PREFIX}.TAG`, type: "enum" },
  { id: "role", labelKey: `${FIELDS_PREFIX}.ROLE`, type: "enum" },
  { id: "parentorganizations", labelKey: `${FIELDS_PREFIX}.COMPANY`, type: "ref" },
  { id: "hasparentorganizations", labelKey: `${FIELDS_PREFIX}.BELONGS_TO_COMPANY`, type: "bool" },
  { id: "businesscategory", labelKey: `${FIELDS_PREFIX}.BUSINESS_CATEGORY`, type: "enum" },
  { id: "createddate", labelKey: `${FIELDS_PREFIX}.REGISTERED`, type: "date" },
  { id: "defaultlanguage", labelKey: `${FIELDS_PREFIX}.PREFERRED_LANGUAGE`, type: "enum" },
];

export const OPERATORS_BY_TYPE: Record<AudienceFieldType, ConditionOperator[]> = {
  text: ["is", "isNot", "startsWith", "endsWith", "contains"],
  enum: ["is", "isNot", "anyOf"],
  ref: ["is", "isNot", "anyOf"],
  date: ["onOrAfter", "onOrBefore"],
  bool: ["is"],
};

/**
 * These generate a quoted `*` value. ElasticSearchFiltersBuilder only takes its wildcard branch
 * when the filter carries a single value, so a second value silently turns the phrase into an
 * exact terms query that matches nobody.
 */
export const WILDCARD_OPERATORS: ConditionOperator[] = ["startsWith", "endsWith", "contains"];

export function findField(id: string): AudienceField | undefined {
  return AUDIENCE_FIELDS.find((field) => field.id === id);
}
