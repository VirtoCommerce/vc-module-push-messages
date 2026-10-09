import { describe, expect, it } from "vitest";
import en from "../locales/en.json";
import { copyAudience, sameAudience } from "./audienceSync";
import {
  addsUp,
  audienceStatus,
  conditionCount,
  flowSteps,
  formatSourceLine,
  noteParts,
  sourceParts,
  SUMMARY_PREFIX,
} from "./audienceSummary";

const result = (overrides = {}) => ({
  matchedPeople: 10,
  foundInCompanies: 7,
  overlaps: 11,
  peopleInScope: 6,
  extraLogins: 1,
  totalCount: 7,
  pickedPeople: 0,
  pickedCompanies: 6,
  ...overrides,
});

/** Resolves a key against en.json and picks the vue-i18n plural branch, as the app does. */
function t(key: string, plural?: number): string {
  const message = key.split(".").reduce<any>((node, part) => node?.[part], en) as string;
  const branches = message.split(" | ");
  const text = plural === undefined || branches.length === 1 ? branches[0] : branches[plural === 1 ? 0 : 1];

  return text.replace("{count}", String(plural));
}

describe("sourceParts", () => {
  it("lists only the source types present", () => {
    expect(sourceParts("conditions", 1, result())).toEqual([
      { key: "CONDITIONS", count: 1 },
      { key: "COMPANIES", count: 6 },
    ]);
  });

  it("hides a type whose count is zero", () => {
    expect(sourceParts("list", 0, result({ pickedPeople: 2, pickedCompanies: 0 }))).toEqual([{ key: "PEOPLE", count: 2 }]);
  });

  it("names Everyone on its own, though its phrase parses to one row", () => {
    expect(sourceParts("everyone", conditionCount("membertype:Contact"), result())).toEqual([{ key: "EVERYONE" }]);
  });

  it("names an advanced query without counting conditions", () => {
    expect(sourceParts("query", 3, result({ pickedCompanies: 0 }))).toEqual([{ key: "QUERY" }]);
  });

  it("reads like the design", () => {
    expect(formatSourceLine(sourceParts("conditions", 1, result()), t)).toBe("1 condition · 6 companies");
  });
});

describe("conditionCount", () => {
  it("counts the rows of a phrase the builder can read", () => {
    expect(conditionCount("role:Purchaser status:Approved")).toBe(2);
  });

  it("is zero for no phrase and for one the builder cannot read", () => {
    expect(conditionCount(undefined)).toBe(0);
    expect(conditionCount("(")).toBe(0);
  });
});

describe("flowSteps", () => {
  it("runs matched, found, unique, recipients; the last equals the total", () => {
    const steps = flowSteps(result());

    expect(steps.map((s) => [s.key, s.value, s.signed])).toEqual([
      ["MATCHED", 10, false],
      ["FOUND", 7, true],
      ["UNIQUE", 6, false],
      ["RECIPIENTS", 7, false],
    ]);
  });
});

describe("noteParts", () => {
  it("hides each part whose value is zero", () => {
    expect(noteParts(result({ overlaps: 0 }))).toEqual([{ key: "EXTRA_LOGINS", count: 1 }]);
    expect(noteParts(result({ overlaps: 0, extraLogins: 0 }))).toEqual([]);
  });

  it("uses the singular for one and the plural otherwise", () => {
    expect(t(`${SUMMARY_PREFIX}.NOTE.OVERLAPS`, 1)).toBe("1 overlap");
    expect(t(`${SUMMARY_PREFIX}.NOTE.OVERLAPS`, 11)).toBe("11 overlaps");
    expect(t(`${SUMMARY_PREFIX}.NOTE.EXTRA_LOGINS`, 1)).toBe("+1 login");
    expect(t(`${SUMMARY_PREFIX}.NOTE.EXTRA_LOGINS`, 2)).toBe("+2 logins");
  });
});

describe("addsUp", () => {
  it("accepts a result whose identities hold", () => {
    expect(addsUp(result())).toBe(true);
  });

  it("rejects one that does not add up", () => {
    expect(addsUp(result({ overlaps: 10 }))).toBe(false);
    expect(addsUp(result({ totalCount: 8 }))).toBe(false);
  });
});

describe("audienceStatus", () => {
  it("is ready with recipients, empty without, invalid when counting failed", () => {
    expect(audienceStatus({ result: result(), failed: false, loading: false })).toBe("READY");
    expect(audienceStatus({ result: result({ totalCount: 0 }), failed: false, loading: false })).toBe("EMPTY");
    expect(audienceStatus({ result: undefined, failed: false, loading: false })).toBe("EMPTY");
    expect(audienceStatus({ result: undefined, failed: true, loading: false })).toBe("INVALID");
  });
});

describe("copyAudience / sameAudience", () => {
  it("keeps an absent list absent", () => {
    expect(copyAudience({ memberQuery: "q" })).toEqual({ memberQuery: "q", memberIds: undefined });
  });

  it("does not share the list with the original", () => {
    const original = { memberIds: ["a"] };
    const copy = copyAudience(original);

    copy.memberIds!.push("b");

    expect(original.memberIds).toEqual(["a"]);
  });

  it("treats an empty list and no list as the same audience", () => {
    expect(sameAudience({ memberIds: [] }, {})).toBe(true);
    expect(sameAudience({ memberQuery: "a" }, { memberQuery: "b" })).toBe(false);
  });
});
