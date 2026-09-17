import { describe, expect, it } from "vitest";
import { OPERATORS_BY_TYPE } from "./operators";
import { buildPhrase, combineDuplicateFields, hasContradiction, parsePhrase, toRowValue, validateRow } from "./phrase";
import type { ConditionRow, QueryField } from "./types";

/** The member fields the push-messages module builds on, as the shared builder receives them. */
const FIELDS: QueryField[] = [
  { id: "name", label: "Name", type: "text" },
  { id: "emails", label: "Email", type: "text" },
  { id: "login", label: "Username", type: "text" },
  { id: "membertype", label: "Customer type", type: "enum", options: ["Contact", "Organization"] },
  { id: "status", label: "Status", type: "enum", options: ["Approved", "New"] },
  { id: "groups", label: "Tag", type: "enum" },
  { id: "roleid", label: "Role", type: "ref" },
  { id: "parentorganizations", label: "Company", type: "ref" },
  { id: "hasparentorganizations", label: "Belongs to a company", type: "bool" },
  { id: "createddate", label: "Registered", type: "date" },
];

describe("buildPhrase", () => {
  it("joins ALL conditions with a space", () => {
    const rows: ConditionRow[] = [
      { field: "role", operator: "is", value: "Purchaser" },
      { field: "status", operator: "is", value: "Approved" },
    ];
    expect(buildPhrase(rows, "all")).toBe("role:Purchaser status:Approved");
  });

  it("joins ANY conditions with OR", () => {
    const rows: ConditionRow[] = [
      { field: "role", operator: "is", value: "Purchaser" },
      { field: "role", operator: "is", value: "Installer" },
    ];
    expect(buildPhrase(rows, "any")).toBe("role:Purchaser OR role:Installer");
  });

  it("quotes values the lexer cannot read bare", () => {
    const rows: ConditionRow[] = [{ field: "parentorganizations", operator: "is", value: "Acme Installatie B.V." }];
    expect(buildPhrase(rows, "all")).toBe('parentorganizations:"Acme Installatie B.V."');
  });

  it("skips rows with no value", () => {
    const rows: ConditionRow[] = [
      { field: "role", operator: "is", value: "Purchaser" },
      { field: "status", operator: "is", value: "" },
    ];
    expect(buildPhrase(rows, "all")).toBe("role:Purchaser");
  });
});

describe("round trip", () => {
  const cases: ConditionRow[] = [
    { field: "name", operator: "is", value: "Sanne" },
    { field: "name", operator: "isNot", value: "Sanne" },
    { field: "role", operator: "anyOf", value: "Purchaser,Installer" },
    { field: "emails", operator: "startsWith", value: "s." },
    { field: "emails", operator: "endsWith", value: "@acme.nl" },
    { field: "emails", operator: "contains", value: "acme" },
    { field: "createddate", operator: "onOrAfter", value: "2025-01-01" },
    { field: "createddate", operator: "onOrBefore", value: "2025-12-31" },
    { field: "parentorganizations", operator: "is", value: "Acme Installatie B.V." },
    { field: "hasparentorganizations", operator: "is", value: "true" },
  ];

  it.each(cases)("survives %o", (row) => {
    const phrase = buildPhrase([row], "all");
    expect(parsePhrase(phrase)).toEqual({ join: "all", rows: [row] });
  });

  it("recovers the ANY join", () => {
    const rows: ConditionRow[] = [
      { field: "role", operator: "is", value: "Purchaser" },
      { field: "role", operator: "is", value: "Installer" },
    ];
    const phrase = buildPhrase(rows, "any");
    expect(parsePhrase(phrase)).toEqual({ join: "any", rows });
  });

  it("recovers the Everyone phrase as a single condition", () => {
    expect(parsePhrase("membertype:Contact")).toEqual({
      join: "all",
      rows: [{ field: "membertype", operator: "is", value: "Contact" }],
    });
  });

  it("keeps a range clause whole despite its inner space", () => {
    expect(parsePhrase("createddate:[2025-01-01 TO] role:Purchaser")).toEqual({
      join: "all",
      rows: [
        { field: "createddate", operator: "onOrAfter", value: "2025-01-01" },
        { field: "role", operator: "is", value: "Purchaser" },
      ],
    });
  });

  it("gives up on a clause that is nothing but a wildcard", () => {
    // It used to parse as startsWith with an empty value, which buildPhrase then dropped —
    // reopening and saving such a message erased its audience.
    expect(parsePhrase('name:"*"')).toBeNull();
    expect(parsePhrase('name:""')).toBeNull();
  });

  it("survives a quote inside a wildcard value", () => {
    const row: ConditionRow = { field: "name", operator: "contains", value: 'a"b' };
    const phrase = buildPhrase([row], "all");

    expect(phrase).toBe('name:"*a\\"b*"');
    expect(parsePhrase(phrase)).toEqual({ join: "all", rows: [row] });
  });

  it("gives up on parentheses", () => {
    expect(parsePhrase("(role:Purchaser OR role:Installer) status:Approved")).toBeNull();
  });

  it("gives up when joins are mixed", () => {
    expect(parsePhrase("role:Purchaser status:Approved OR role:Installer")).toBeNull();
  });

  it("gives up on a bare keyword with no field", () => {
    expect(parsePhrase("acme")).toBeNull();
  });

  it("gives up on an empty phrase", () => {
    expect(parsePhrase("")).toBeNull();
    expect(parsePhrase("   ")).toBeNull();
  });

  it("gives up on an unbalanced quote", () => {
    expect(parsePhrase('name:"Acme')).toBeNull();
  });
});

describe("validateRow", () => {
  it("rejects a wildcard operator carrying two values", () => {
    const row: ConditionRow = { field: "emails", operator: "startsWith", value: "a,b" };
    expect(validateRow(row)).toBe("QUERY_BUILDER.VALIDATION.WILDCARD_SINGLE_VALUE");
  });

  it("rejects an empty value", () => {
    const row: ConditionRow = { field: "name", operator: "is", value: "" };
    expect(validateRow(row)).toBe("QUERY_BUILDER.VALIDATION.VALUE_REQUIRED");
  });

  it("accepts a well-formed row", () => {
    const row: ConditionRow = { field: "role", operator: "anyOf", value: "Purchaser,Installer" };
    expect(validateRow(row)).toBeNull();
  });

  it("survives a cleared value instead of throwing", () => {
    // VcSelect hands back undefined when its clear button is pressed.
    const row = { field: "role", operator: "is", value: undefined } as unknown as ConditionRow;
    expect(validateRow(row)).toBe("QUERY_BUILDER.VALIDATION.VALUE_REQUIRED");
  });

  it("rejects a row whose operator was cleared", () => {
    const row = { field: "role", operator: undefined, value: "Purchaser" } as unknown as ConditionRow;
    expect(validateRow(row)).toBe("QUERY_BUILDER.VALIDATION.INCOMPLETE");
  });

  it("builds nothing from a row whose operator was cleared", () => {
    const row = { field: "role", operator: undefined, value: "Purchaser" } as unknown as ConditionRow;
    expect(buildPhrase([row], "all")).toBe("");
  });

  it("accepts a single wildcard value", () => {
    const row: ConditionRow = { field: "emails", operator: "contains", value: "acme" };
    expect(validateRow(row)).toBeNull();
  });
});

describe("hasContradiction", () => {
  const company = (value: string): ConditionRow => ({ field: "parentorganizations", operator: "is", value });

  it("flags two exact conditions on the same field under ALL", () => {
    expect(hasContradiction("all", [company("a"), company("b")])).toBe(true);
  });

  it("allows the same field under ANY", () => {
    expect(hasContradiction("any", [company("a"), company("b")])).toBe(false);
  });

  it("allows different fields under ALL", () => {
    expect(hasContradiction("all", [company("a"), { field: "role", operator: "is", value: "Purchaser" }])).toBe(false);
  });

  it("ignores rows with no value yet", () => {
    expect(hasContradiction("all", [company("a"), company("")])).toBe(false);
  });

  it("flags an is-any-of row against another condition on the same field", () => {
    // After combining, adding one more company lands here — still impossible under ALL.
    const rows: ConditionRow[] = [
      { field: "parentorganizations", operator: "anyOf", value: "a,b" },
      company("c"),
    ];
    expect(hasContradiction("all", rows)).toBe(true);
  });

  it("does not flag a single is-any-of row, which is the way to express it", () => {
    expect(hasContradiction("all", [{ field: "parentorganizations", operator: "anyOf", value: "a,b" }])).toBe(false);
  });
});

describe("combineDuplicateFields", () => {
  const company = (value: string): ConditionRow => ({ field: "parentorganizations", operator: "is", value });

  it("folds two exact conditions on one field into is-any-of", () => {
    expect(combineDuplicateFields([company("a"), company("b")], FIELDS)).toEqual([
      { field: "parentorganizations", operator: "anyOf", value: "a,b" },
    ]);
  });

  it("folds three into one", () => {
    expect(combineDuplicateFields([company("a"), company("b"), company("c")], FIELDS)).toEqual([
      { field: "parentorganizations", operator: "anyOf", value: "a,b,c" },
    ]);
  });

  it("leaves conditions on different fields alone", () => {
    const rows = [company("a"), { field: "role", operator: "is", value: "Purchaser" } as ConditionRow];
    expect(combineDuplicateFields(rows, FIELDS)).toEqual(rows);
  });

  it("extends an existing is-any-of instead of adding a second row", () => {
    const rows: ConditionRow[] = [
      { field: "parentorganizations", operator: "anyOf", value: "a,b" },
      company("c"),
    ];
    expect(combineDuplicateFields(rows, FIELDS)).toEqual([
      { field: "parentorganizations", operator: "anyOf", value: "a,b,c" },
    ]);
  });

  it("does not fold a negation into the positives", () => {
    const rows: ConditionRow[] = [company("a"), { field: "parentorganizations", operator: "isNot", value: "b" }];
    expect(combineDuplicateFields(rows, FIELDS)).toEqual(rows);
  });

  it("leaves empty rows untouched", () => {
    const rows: ConditionRow[] = [company("a"), company("")];
    expect(combineDuplicateFields(rows, FIELDS)).toEqual(rows);
  });

  it("produces a phrase the parser reads back to the same rows", () => {
    const merged = combineDuplicateFields([company("acme"), company("vdberg")], FIELDS);
    const phrase = buildPhrase(merged, "all");
    expect(phrase).toBe("parentorganizations:acme,vdberg");
    expect(parsePhrase(phrase)).toEqual({ join: "all", rows: merged });
  });
});

describe("operators the builder can produce are operators it offers", () => {
  // Combining rows produces "is any of". A field type that does not offer it leaves the select
  // unable to name its own value, which is how a raw "anyOf" ended up on screen.
  it.each(FIELDS)("$id never ends up with an operator it cannot name", (field) => {
    const operator = OPERATORS_BY_TYPE[field.type][0];
    const combined = combineDuplicateFields([
      { field: field.id, operator, value: "a" },
      { field: field.id, operator, value: "b" },
    ], FIELDS);

    for (const row of combined) {
      expect(OPERATORS_BY_TYPE[field.type]).toContain(row.operator);
    }
  });

  it("folds a text field, which offers is any of", () => {
    const rows: ConditionRow[] = [
      { field: "name", operator: "is", value: "a" },
      { field: "name", operator: "is", value: "b" },
    ];
    expect(combineDuplicateFields(rows, FIELDS)).toEqual([{ field: "name", operator: "anyOf", value: "a,b" }]);
  });

  it("leaves a yes/no field alone, because it has nothing to fold into", () => {
    const rows: ConditionRow[] = [
      { field: "hasparentorganizations", operator: "is", value: "true" },
      { field: "hasparentorganizations", operator: "is", value: "false" },
    ];
    expect(combineDuplicateFields(rows, FIELDS)).toEqual(rows);
  });
});

describe("toRowValue", () => {
  it("writes a picked date as the plain date the search phrase understands", () => {
    // The control hands back a Date; its own toString is a locale sentence the parser rejects.
    expect(toRowValue(new Date(2026, 8, 10))).toBe("2026-09-10");
  });

  it("keeps a date picked late in the day on that day", () => {
    // toISOString would move any evening east of UTC to the next morning.
    expect(toRowValue(new Date(2026, 0, 31, 23, 30))).toBe("2026-01-31");
  });

  it("joins a multi-value control and empties a cleared one", () => {
    expect(toRowValue(["a", "b"])).toBe("a,b");
    expect(toRowValue(null)).toBe("");
    expect(toRowValue(undefined)).toBe("");
  });
});

describe("a date condition", () => {
  it("builds a range the parser accepts", () => {
    const rows: ConditionRow[] = [
      { field: "createddate", operator: "onOrAfter", value: toRowValue(new Date(2026, 8, 10)) },
    ];

    expect(buildPhrase(rows, "all")).toBe("createddate:[2026-09-10 TO]");
    expect(parsePhrase(buildPhrase(rows, "all"))).toEqual({
      join: "all",
      rows: [{ field: "createddate", operator: "onOrAfter", value: "2026-09-10" }],
    });
  });
});
