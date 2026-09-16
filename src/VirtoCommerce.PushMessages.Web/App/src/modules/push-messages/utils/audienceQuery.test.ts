import { describe, expect, it } from "vitest";
import { buildQuery, combineDuplicateFields, detectAudience, hasContradiction, parseQuery, validateRow } from "./audienceQuery";
import type { AudienceState, ConditionRow } from "./audienceQuery";

function state(partial: Partial<AudienceState>): AudienceState {
  return { mode: "conditions", join: "all", rows: [], memberIds: [], query: "", ...partial };
}

describe("buildQuery", () => {
  it("renders Everyone as a contact filter", () => {
    expect(buildQuery(state({ mode: "everyone" }))).toBe("membertype:Contact");
  });

  it("returns an empty phrase for a picked list", () => {
    expect(buildQuery(state({ mode: "list", memberIds: ["org1"] }))).toBe("");
  });

  it("passes an advanced query through untouched", () => {
    expect(buildQuery(state({ mode: "query", query: "role:Purchaser" }))).toBe("role:Purchaser");
  });

  it("joins ALL conditions with a space", () => {
    const rows: ConditionRow[] = [
      { field: "role", operator: "is", value: "Purchaser" },
      { field: "status", operator: "is", value: "Approved" },
    ];
    expect(buildQuery(state({ rows }))).toBe("role:Purchaser status:Approved");
  });

  it("joins ANY conditions with OR", () => {
    const rows: ConditionRow[] = [
      { field: "role", operator: "is", value: "Purchaser" },
      { field: "role", operator: "is", value: "Installer" },
    ];
    expect(buildQuery(state({ join: "any", rows }))).toBe("role:Purchaser OR role:Installer");
  });

  it("quotes values the lexer cannot read bare", () => {
    const rows: ConditionRow[] = [{ field: "parentorganizations", operator: "is", value: "Acme Installatie B.V." }];
    expect(buildQuery(state({ rows }))).toBe('parentorganizations:"Acme Installatie B.V."');
  });

  it("skips rows with no value", () => {
    const rows: ConditionRow[] = [
      { field: "role", operator: "is", value: "Purchaser" },
      { field: "status", operator: "is", value: "" },
    ];
    expect(buildQuery(state({ rows }))).toBe("role:Purchaser");
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
    const phrase = buildQuery(state({ rows: [row] }));
    expect(parseQuery(phrase)).toEqual({ join: "all", rows: [row] });
  });

  it("recovers the ANY join", () => {
    const rows: ConditionRow[] = [
      { field: "role", operator: "is", value: "Purchaser" },
      { field: "role", operator: "is", value: "Installer" },
    ];
    const phrase = buildQuery(state({ join: "any", rows }));
    expect(parseQuery(phrase)).toEqual({ join: "any", rows });
  });

  it("recovers the Everyone phrase as a single condition", () => {
    expect(parseQuery("membertype:Contact")).toEqual({
      join: "all",
      rows: [{ field: "membertype", operator: "is", value: "Contact" }],
    });
  });

  it("keeps a range clause whole despite its inner space", () => {
    expect(parseQuery("createddate:[2025-01-01 TO] role:Purchaser")).toEqual({
      join: "all",
      rows: [
        { field: "createddate", operator: "onOrAfter", value: "2025-01-01" },
        { field: "role", operator: "is", value: "Purchaser" },
      ],
    });
  });

  it("gives up on a clause that is nothing but a wildcard", () => {
    // It used to parse as startsWith with an empty value, which buildQuery then dropped —
    // reopening and saving such a message erased its audience.
    expect(parseQuery('name:"*"')).toBeNull();
    expect(parseQuery('name:""')).toBeNull();
  });

  it("survives a quote inside a wildcard value", () => {
    const row: ConditionRow = { field: "name", operator: "contains", value: 'a"b' };
    const phrase = buildQuery(state({ rows: [row] }));

    expect(phrase).toBe('name:"*a\\"b*"');
    expect(parseQuery(phrase)).toEqual({ join: "all", rows: [row] });
  });

  it("gives up on parentheses", () => {
    expect(parseQuery("(role:Purchaser OR role:Installer) status:Approved")).toBeNull();
  });

  it("gives up when joins are mixed", () => {
    expect(parseQuery("role:Purchaser status:Approved OR role:Installer")).toBeNull();
  });

  it("gives up on a bare keyword with no field", () => {
    expect(parseQuery("acme")).toBeNull();
  });

  it("gives up on an empty phrase", () => {
    expect(parseQuery("")).toBeNull();
    expect(parseQuery("   ")).toBeNull();
  });

  it("gives up on an unbalanced quote", () => {
    expect(parseQuery('name:"Acme')).toBeNull();
  });
});

describe("validateRow", () => {
  it("rejects a wildcard operator carrying two values", () => {
    const row: ConditionRow = { field: "emails", operator: "startsWith", value: "a,b" };
    expect(validateRow(row)).toBe("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.VALIDATION.WILDCARD_SINGLE_VALUE");
  });

  it("rejects an empty value", () => {
    const row: ConditionRow = { field: "name", operator: "is", value: "" };
    expect(validateRow(row)).toBe("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.VALIDATION.VALUE_REQUIRED");
  });

  it("accepts a well-formed row", () => {
    const row: ConditionRow = { field: "role", operator: "anyOf", value: "Purchaser,Installer" };
    expect(validateRow(row)).toBeNull();
  });

  it("survives a cleared value instead of throwing", () => {
    // VcSelect hands back undefined when its clear button is pressed.
    const row = { field: "role", operator: "is", value: undefined } as unknown as ConditionRow;
    expect(validateRow(row)).toBe("PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.VALIDATION.VALUE_REQUIRED");
  });

  it("accepts a single wildcard value", () => {
    const row: ConditionRow = { field: "emails", operator: "contains", value: "acme" };
    expect(validateRow(row)).toBeNull();
  });
});

describe("detectAudience", () => {
  it("opens a picked list as the list mode", () => {
    expect(detectAudience(undefined, ["org1"]).mode).toBe("list");
    expect(detectAudience("", ["org1"]).mode).toBe("list");
  });

  it("opens the Everyone phrase as Everyone", () => {
    expect(detectAudience("membertype:Contact", []).mode).toBe("everyone");
  });

  it("prefers the list when a phrase is absent but ids are present", () => {
    // Order matters: the Everyone check must not claim this one.
    expect(detectAudience(undefined, ["org1"]).mode).toBe("list");
  });

  it("treats the Everyone phrase alongside a list as ordinary conditions", () => {
    const detected = detectAudience("membertype:Contact", ["org1"]);

    expect(detected.mode).toBe("conditions");
    expect(detected.rows).toEqual([{ field: "membertype", operator: "is", value: "Contact" }]);
  });

  it("opens a representable phrase as conditions", () => {
    const detected = detectAudience("role:Purchaser OR role:Installer");

    expect(detected.mode).toBe("conditions");
    expect(detected.join).toBe("any");
    expect(detected.rows).toHaveLength(2);
  });

  it("opens an unrepresentable phrase as a raw query", () => {
    expect(detectAudience("(role:Purchaser OR role:Installer) status:Approved").mode).toBe("query");
  });

  it("opens a blank message as conditions with one empty row", () => {
    const detected = detectAudience(undefined, []);

    expect(detected.mode).toBe("conditions");
    expect(detected.rows).toEqual([{ field: "name", operator: "is", value: "" }]);
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

  it("does not flag a single is-any-of row, which is the way to express it", () => {
    expect(hasContradiction("all", [{ field: "parentorganizations", operator: "anyOf", value: "a,b" }])).toBe(false);
  });
});

describe("combineDuplicateFields", () => {
  const company = (value: string): ConditionRow => ({ field: "parentorganizations", operator: "is", value });

  it("folds two exact conditions on one field into is-any-of", () => {
    expect(combineDuplicateFields([company("a"), company("b")])).toEqual([
      { field: "parentorganizations", operator: "anyOf", value: "a,b" },
    ]);
  });

  it("folds three into one", () => {
    expect(combineDuplicateFields([company("a"), company("b"), company("c")])).toEqual([
      { field: "parentorganizations", operator: "anyOf", value: "a,b,c" },
    ]);
  });

  it("leaves conditions on different fields alone", () => {
    const rows = [company("a"), { field: "role", operator: "is", value: "Purchaser" } as ConditionRow];
    expect(combineDuplicateFields(rows)).toEqual(rows);
  });

  it("extends an existing is-any-of instead of adding a second row", () => {
    const rows: ConditionRow[] = [
      { field: "parentorganizations", operator: "anyOf", value: "a,b" },
      company("c"),
    ];
    expect(combineDuplicateFields(rows)).toEqual([
      { field: "parentorganizations", operator: "anyOf", value: "a,b,c" },
    ]);
  });

  it("does not fold a negation into the positives", () => {
    const rows: ConditionRow[] = [company("a"), { field: "parentorganizations", operator: "isNot", value: "b" }];
    expect(combineDuplicateFields(rows)).toEqual(rows);
  });

  it("leaves empty rows untouched", () => {
    const rows: ConditionRow[] = [company("a"), company("")];
    expect(combineDuplicateFields(rows)).toEqual(rows);
  });

  it("produces a phrase the parser reads back to the same rows", () => {
    const merged = combineDuplicateFields([company("acme"), company("vdberg")]);
    const phrase = buildQuery({ mode: "conditions", join: "all", rows: merged, memberIds: [], query: "" });
    expect(phrase).toBe("parentorganizations:acme,vdberg");
    expect(parseQuery(phrase)).toEqual({ join: "all", rows: merged });
  });
});
