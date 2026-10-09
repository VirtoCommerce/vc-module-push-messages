import type { PushMessageAudienceResult } from "../../../api_client/virtocommerce.pushmessages";
import { parsePhrase } from "../../../components/queryBuilder/phrase";
import type { AudienceMode } from "./audienceQuery";
import type { AudienceEstimate } from "./audienceSync";

export const SUMMARY_PREFIX = "PUSH_MESSAGES.PAGES.DETAILS.FORM.AUDIENCE.BREAKDOWN";

export type SourceKey = "EVERYONE" | "QUERY" | "CONDITIONS" | "PEOPLE" | "COMPANIES";

export interface SourcePart {
  key: SourceKey;
  count?: number;
}

export type Translate = (key: string, plural?: number) => string;

/** Where the recipients come from, one part per type present: "1 condition · 6 companies". */
export function sourceParts(mode: AudienceMode, conditionCount: number, result?: PushMessageAudienceResult): SourcePart[] {
  if (mode === "everyone") {
    return [{ key: "EVERYONE" }];
  }

  const parts: SourcePart[] = [];

  if (mode === "query") {
    parts.push({ key: "QUERY" });
  } else if (mode === "conditions" && conditionCount > 0) {
    parts.push({ key: "CONDITIONS", count: conditionCount });
  }

  if (result?.pickedPeople) {
    parts.push({ key: "PEOPLE", count: result.pickedPeople });
  }

  if (result?.pickedCompanies) {
    parts.push({ key: "COMPANIES", count: result.pickedCompanies });
  }

  return parts;
}

export function formatSourceLine(parts: SourcePart[], t: Translate): string {
  return parts
    .map((part) => (part.count === undefined ? t(`${SUMMARY_PREFIX}.SOURCES.${part.key}`) : t(`${SUMMARY_PREFIX}.SOURCES.${part.key}`, part.count)))
    .join(" · ");
}

/** Rows in a phrase the condition builder can read; zero for anything else. */
export function conditionCount(memberQuery?: string): number {
  return memberQuery ? (parsePhrase(memberQuery)?.rows.length ?? 0) : 0;
}

export interface FlowStep {
  key: "MATCHED" | "FOUND" | "UNIQUE" | "RECIPIENTS";
  value: number;
  /** Shown with a leading "+": it adds to the step before it. */
  signed: boolean;
}

export function flowSteps(result: PushMessageAudienceResult): FlowStep[] {
  return [
    { key: "MATCHED", value: result.matchedPeople ?? 0, signed: false },
    { key: "FOUND", value: result.foundInCompanies ?? 0, signed: true },
    { key: "UNIQUE", value: result.peopleInScope ?? 0, signed: false },
    { key: "RECIPIENTS", value: result.totalCount ?? 0, signed: false },
  ];
}

export interface NotePart {
  key: "OVERLAPS" | "EXTRA_LOGINS";
  count: number;
}

/** What explains the gaps between the steps; a part with nothing to explain is left out. */
export function noteParts(result: PushMessageAudienceResult): NotePart[] {
  const parts: NotePart[] = [];

  if (result.overlaps) {
    parts.push({ key: "OVERLAPS", count: result.overlaps });
  }

  if (result.extraLogins) {
    parts.push({ key: "EXTRA_LOGINS", count: result.extraLogins });
  }

  return parts;
}

/** The backend derives Overlaps so this holds; the widget still refuses to show a flow that does not. */
export function addsUp(result: PushMessageAudienceResult): boolean {
  const matched = result.matchedPeople ?? 0;
  const found = result.foundInCompanies ?? 0;
  const overlaps = result.overlaps ?? 0;
  const unique = result.peopleInScope ?? 0;

  return matched + found - overlaps === unique && unique + (result.extraLogins ?? 0) === (result.totalCount ?? 0);
}

export type AudienceStatus = "READY" | "EMPTY" | "INVALID";

export function audienceStatus(estimate: AudienceEstimate): AudienceStatus {
  if (estimate.failed) {
    return "INVALID";
  }

  return (estimate.result?.totalCount ?? 0) > 0 ? "READY" : "EMPTY";
}
