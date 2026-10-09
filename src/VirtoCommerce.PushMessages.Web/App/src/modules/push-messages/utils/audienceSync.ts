import type { PushMessageAudienceResult } from "../../../api_client/virtocommerce.pushmessages";

/** The method Blade 1 exposes to the recipients blade. callParent ignores unknown names silently. */
export const SET_AUDIENCE = "setAudience";

export interface AudienceDraft {
  memberQuery?: string;
  memberIds?: string[];
}

export interface AudienceEstimate {
  result?: PushMessageAudienceResult;
  /** The platform could not count the audience — most often a query the index rejects. */
  failed: boolean;
  loading: boolean;
}

export interface SetAudiencePayload {
  audience: AudienceDraft;
  invalid: boolean;
  estimate: AudienceEstimate;
}

export interface SelectRecipientsOptions {
  /** Never mutated: it is the snapshot Cancel restores. */
  audience: AudienceDraft;
  invalid: boolean;
  estimate: AudienceEstimate;
  readonly: boolean;
  sentCount?: number;
}

/** A copy that keeps an absent list absent, so restoring it does not mark the message modified. */
export function copyAudience(audience: AudienceDraft): AudienceDraft {
  return {
    memberQuery: audience.memberQuery,
    memberIds: audience.memberIds ? [...audience.memberIds] : undefined,
  };
}

/** Whether two drafts describe the same audience; an empty list and no list are the same. */
export function sameAudience(a: AudienceDraft, b: AudienceDraft): boolean {
  return (a.memberQuery || "") === (b.memberQuery || "") && JSON.stringify(a.memberIds ?? []) === JSON.stringify(b.memberIds ?? []);
}
