import { parsePhrase } from "../../../components/queryBuilder/phrase";

export type AudienceMode = "everyone" | "list" | "conditions" | "query";

export const EVERYONE_QUERY = "membertype:Contact";

/** Matches [StringLength(1024)] on PushMessageEntity.MemberQuery. */
export const MAX_QUERY_LENGTH = 1024;

/** The phrase an audience hands over, whichever mode produced it. */
export function audiencePhrase(mode: AudienceMode, conditions: string, rawQuery: string): string {
  if (mode === "everyone") {
    return EVERYONE_QUERY;
  }

  if (mode === "list") {
    return "";
  }

  return mode === "query" ? rawQuery : conditions;
}

/**
 * Works out which mode a stored audience should open in. Order matters and the first match
 * wins: a picked list with no phrase is a list, the Everyone phrase on its own is Everyone, a
 * phrase the condition builder can represent is conditions, and anything else opens as a raw query.
 */
export function detectAudienceMode(memberQuery?: string, memberIds?: string[]): AudienceMode {
  const hasIds = (memberIds?.length ?? 0) > 0;

  if (!memberQuery && hasIds) {
    return "list";
  }

  if (memberQuery === EVERYONE_QUERY && !hasIds) {
    return "everyone";
  }

  if (!memberQuery) {
    return "conditions";
  }

  return parsePhrase(memberQuery) ? "conditions" : "query";
}
