import { describe, expect, it } from "vitest";
import { audiencePhrase, detectAudienceMode, EVERYONE_QUERY } from "./audienceQuery";

describe("detectAudienceMode", () => {
  it("opens a picked list as the list mode", () => {
    expect(detectAudienceMode(undefined, ["org1"])).toBe("list");
    expect(detectAudienceMode("", ["org1"])).toBe("list");
  });

  it("opens the Everyone phrase as Everyone", () => {
    expect(detectAudienceMode(EVERYONE_QUERY, [])).toBe("everyone");
  });

  it("prefers the list when a phrase is absent but ids are present", () => {
    // Order matters: the Everyone check must not claim this one.
    expect(detectAudienceMode(undefined, ["org1"])).toBe("list");
  });

  it("treats the Everyone phrase alongside a list as ordinary conditions", () => {
    expect(detectAudienceMode(EVERYONE_QUERY, ["org1"])).toBe("conditions");
  });

  it("opens a representable phrase as conditions", () => {
    expect(detectAudienceMode("roleid:purchaser OR roleid:installer")).toBe("conditions");
  });

  it("opens an unrepresentable phrase as a raw query", () => {
    expect(detectAudienceMode("(roleid:purchaser OR roleid:installer) status:Approved")).toBe("query");
  });

  it("opens a blank message as conditions", () => {
    expect(detectAudienceMode(undefined, [])).toBe("conditions");
  });
});


describe("audiencePhrase", () => {
  it("renders Everyone as a contact filter", () => {
    expect(audiencePhrase("everyone", "name:Alex", "role:x")).toBe(EVERYONE_QUERY);
  });

  it("returns an empty phrase for a picked list", () => {
    expect(audiencePhrase("list", "name:Alex", "role:x")).toBe("");
  });

  it("passes an advanced query through untouched", () => {
    expect(audiencePhrase("query", "name:Alex", "roleid:purchaser")).toBe("roleid:purchaser");
  });

  it("hands over what the condition builder produced", () => {
    expect(audiencePhrase("conditions", "name:Alex", "roleid:purchaser")).toBe("name:Alex");
  });
});
