import type { QueryFieldType } from "../../../components/queryBuilder/types";

export interface AudienceField {
  /** Index field name, lower-cased as the search phrase parser expects it. */
  id: string;
  labelKey: string;
  type: QueryFieldType;
  /** Fixed choices. Absent means the value is typed in — the choices live in customer data. */
  options?: string[];
  /** Where a `ref` field's choices come from. */
  source?: "organizations" | "roles";
}

const FIELDS_PREFIX = "PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.FIELDS";

/**
 * Field names come from MemberDocumentBuilder.BuildSchemaAsync() in vc-module-customer.
 * Three are worth knowing about:
 * - `roleid` holds role ids. The sibling `role` field holds NormalizedName — the upper-cased
 *   name — so a role typed the way it reads in the admin never matches; ids are picked instead.
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
  { id: "roleid", labelKey: `${FIELDS_PREFIX}.ROLE`, type: "ref", source: "roles" },
  { id: "parentorganizations", labelKey: `${FIELDS_PREFIX}.COMPANY`, type: "ref", source: "organizations" },
  { id: "hasparentorganizations", labelKey: `${FIELDS_PREFIX}.BELONGS_TO_COMPANY`, type: "bool" },
  { id: "businesscategory", labelKey: `${FIELDS_PREFIX}.BUSINESS_CATEGORY`, type: "enum" },
  { id: "createddate", labelKey: `${FIELDS_PREFIX}.REGISTERED`, type: "date" },
  { id: "defaultlanguage", labelKey: `${FIELDS_PREFIX}.PREFERRED_LANGUAGE`, type: "enum" },
];
